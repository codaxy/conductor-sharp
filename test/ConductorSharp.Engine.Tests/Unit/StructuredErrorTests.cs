using System.Collections.Generic;
using System.Linq;
using ConductorSharp.Client;
using ConductorSharp.Client.Util;
using ConductorSharp.Engine.Exceptions;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace ConductorSharp.Engine.Tests.Unit
{
    public class StructuredErrorTests
    {
        // Mirrors the execution-manager catch block. It calls StructuredError.FromException — the same mapping both
        // ExecutionManager and TypePollSpreadingExecutionManager use — rather than reimplementing it, so a field
        // added to the payload cannot pass here while being dropped by one of the managers.
        private static IDictionary<string, object> SerializeCatchOutput(System.Exception exception)
        {
            var output = new ErrorOutput { ErrorMessage = exception.Message, StructuredError = StructuredError.FromException(exception) };

            return SerializationHelper.ObjectToDictionary(output, ConductorConstants.IoJsonSerializerSettings);
        }

        private static JToken StructuredErrorOf(IDictionary<string, object> dict) =>
            JObject.Parse(JsonConvert.SerializeObject(dict))[StructuredErrorSerializer.OutputKey];

        [Fact]
        public void StructuredErrorException_produces_snake_case_structured_error()
        {
            var exception = new StructuredErrorException("RESOURCE_UNAVAILABLE", "No port available", "https://example.org/entity/42");

            var dict = SerializeCatchOutput(exception);

            Assert.True(dict.ContainsKey("error_message"));
            Assert.True(dict.ContainsKey(StructuredErrorSerializer.OutputKey));

            var structured = StructuredErrorOf(dict);
            Assert.Equal("RESOURCE_UNAVAILABLE", (string)structured["code"]);
            Assert.Equal("No port available", (string)structured["reason"]);
            Assert.Equal("https://example.org/entity/42", (string)structured["reference_error"]);
            Assert.Equal(StructuredError.CurrentVersion, (int)structured["version"]);
        }

        [Fact]
        public void PlainException_output_is_backward_compatible()
        {
            var dict = SerializeCatchOutput(new System.InvalidOperationException("boom"));

            Assert.True(dict.ContainsKey("error_message"));
            Assert.Equal("boom", (string)dict["error_message"]);
            Assert.False(dict.ContainsKey(StructuredErrorSerializer.OutputKey));
            Assert.Single(dict);
        }

        [Fact]
        public void Message_is_carried_under_snake_case_message_key()
        {
            var exception = new StructuredErrorException(
                "VALIDATION_FAILED",
                "Input field not recognized",
                "https://example.org/entity/7",
                "Field 'widget_id' is not present in schema 'default'."
            );

            var structured = StructuredErrorOf(SerializeCatchOutput(exception));

            Assert.Equal("VALIDATION_FAILED", (string)structured["code"]);
            Assert.Equal("Input field not recognized", (string)structured["reason"]);
            Assert.Equal("Field 'widget_id' is not present in schema 'default'.", (string)structured["message"]);
            Assert.Equal("https://example.org/entity/7", (string)structured["reference_error"]);
        }

        [Fact]
        public void Message_reaches_error_message_and_reason_for_incompletion()
        {
            // error_message is set from Exception.Message, which the message-taking constructor overrides. The same
            // value is what the execution manager sends as TaskResult.ReasonForIncompletion (the Conductor UI banner).
            var exception = new StructuredErrorException("CODE", "Short reason", null, "Long diagnostic detail");

            Assert.Equal("Long diagnostic detail", exception.Message);
            Assert.Equal("Long diagnostic detail", (string)SerializeCatchOutput(exception)["error_message"]);
        }

        [Fact]
        public void Message_falls_back_to_the_reason_when_no_message_was_supplied()
        {
            // Exception.Message defaults to the reason, and the payload always carries it — consumers read
            // message without needing a reason fallback of their own.
            var structured = StructuredErrorOf(
                SerializeCatchOutput(new StructuredErrorException("CODE", "Short reason", "https://example.org/entity/1"))
            );

            Assert.Equal("Short reason", (string)structured["message"]);
            Assert.Equal(
                new[] { "code", "message", "reason", "reference_error", "version" },
                ((JObject)structured).Properties().Select(p => p.Name).OrderBy(n => n)
            );
        }

        [Fact]
        public void Message_repeating_the_reason_is_still_carried()
        {
            var exception = new StructuredErrorException("CODE", "Same text", null, "Same text");

            Assert.Equal("Same text", (string)StructuredErrorOf(SerializeCatchOutput(exception))["message"]);
        }

        [Fact]
        public void Message_survives_the_round_trip()
        {
            var dict = SerializeCatchOutput(
                new StructuredErrorException("CODE", "Short reason", "https://example.org/entity/9", "Long diagnostic detail")
            );

            Assert.True(StructuredErrorSerializer.TryDeserialize(dict, out var parsed));
            Assert.Equal("CODE", parsed.Code);
            Assert.Equal("Short reason", parsed.Reason);
            Assert.Equal("Long diagnostic detail", parsed.Message);
            Assert.Equal("https://example.org/entity/9", parsed.ReferenceError);
        }

        [Fact]
        public void FromException_returns_null_for_a_plain_exception()
        {
            Assert.Null(StructuredError.FromException(new System.InvalidOperationException("boom")));
        }

        [Fact]
        public void RoundTrip_helper_output_is_parsed_back()
        {
            // The signal-sender producer: no exception to catch, so the payload is rendered from the model directly.
            var error = new StructuredError
            {
                Code = "UNCLASSIFIED",
                Reason = "generic failure",
                Message = "downstream call failed: connection refused",
                ReferenceError = "https://example.org/entity/7"
            };

            var outputData = StructuredErrorSerializer.Serialize(error);

            Assert.True(StructuredErrorSerializer.TryDeserialize(outputData, out var parsed));
            Assert.Equal(error.Code, parsed.Code);
            Assert.Equal(error.Reason, parsed.Reason);
            Assert.Equal(error.Message, parsed.Message);
            Assert.Equal(error.ReferenceError, parsed.ReferenceError);
            Assert.Equal(error.Version, parsed.Version);
        }

        [Fact]
        public void RoundTrip_catch_block_output_is_parsed_back()
        {
            var dict = SerializeCatchOutput(new StructuredErrorException("RESOURCE_UNAVAILABLE", "No port available"));

            Assert.True(StructuredErrorSerializer.TryDeserialize(dict, out var parsed));
            Assert.Equal("RESOURCE_UNAVAILABLE", parsed.Code);
            Assert.Equal("No port available", parsed.Reason);
        }

        [Fact]
        public void TryParse_returns_false_when_key_absent()
        {
            var dict = new Dictionary<string, object> { ["error_message"] = "boom" };

            Assert.False(StructuredErrorSerializer.TryDeserialize(dict, out var parsed));
            Assert.Null(parsed);
        }

        [Fact]
        public void TryParse_returns_false_on_null_input()
        {
            Assert.False(StructuredErrorSerializer.TryDeserialize(null, out var parsed));
            Assert.Null(parsed);
        }

        [Fact]
        public void TryParse_returns_false_on_malformed_payload()
        {
            var dict = new Dictionary<string, object> { [StructuredErrorSerializer.OutputKey] = "not-a-structured-error" };

            Assert.False(StructuredErrorSerializer.TryDeserialize(dict, out _));
        }

        [Fact]
        public void TryParse_returns_false_when_code_missing()
        {
            var dict = new Dictionary<string, object>
            {
                [StructuredErrorSerializer.OutputKey] = new Dictionary<string, object> { ["reason"] = "no code here" }
            };

            Assert.False(StructuredErrorSerializer.TryDeserialize(dict, out _));
        }

        [Fact]
        public void TryParse_tolerates_a_message_only_payload_by_degrading()
        {
            // A message without a code is still unstructured: the caller must fall back to the generic path.
            var dict = new Dictionary<string, object>
            {
                [StructuredErrorSerializer.OutputKey] = new Dictionary<string, object> { ["message"] = "detail but no code" }
            };

            Assert.False(StructuredErrorSerializer.TryDeserialize(dict, out _));
        }
    }
}
