using System.Collections.Generic;
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
        // Mirrors the execution-manager catch block: builds the ErrorOutput (setting StructuredError for a
        // StructuredErrorException) and serializes it. TryParse below asserts this output round-trips through the
        // shared serializer, pinning the property-derived key/shape to StructuredErrorSerializer.OutputKey.
        private static IDictionary<string, object> SerializeCatchOutput(System.Exception exception)
        {
            var output = new ErrorOutput { ErrorMessage = exception.Message };

            if (exception is StructuredErrorException structuredException)
            {
                output.StructuredError = new StructuredError
                {
                    Code = structuredException.Code,
                    Reason = structuredException.Reason,
                    ReferenceError = structuredException.ReferenceError
                };
            }

            return SerializationHelper.ObjectToDictionary(output, ConductorConstants.IoJsonSerializerSettings);
        }

        [Fact]
        public void StructuredErrorException_produces_snake_case_structured_error()
        {
            var exception = new StructuredErrorException("RESOURCE_UNAVAILABLE", "No port available", "https://rom/resourceOrder/42");

            var dict = SerializeCatchOutput(exception);

            Assert.True(dict.ContainsKey("error_message"));
            Assert.True(dict.ContainsKey(StructuredErrorSerializer.OutputKey));

            var structured = JObject.Parse(JsonConvert.SerializeObject(dict))["structured_error"];
            Assert.Equal("RESOURCE_UNAVAILABLE", (string)structured["code"]);
            Assert.Equal("No port available", (string)structured["reason"]);
            Assert.Equal("https://rom/resourceOrder/42", (string)structured["reference_error"]);
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
        public void RoundTrip_helper_output_is_parsed_back()
        {
            var error = new StructuredError
            {
                Code = "UNCLASSIFIED",
                Reason = "generic failure",
                ReferenceError = "https://rom/resourceOrder/7"
            };

            var outputData = StructuredErrorSerializer.ToOutputData(error);

            Assert.True(StructuredErrorSerializer.TryParse(outputData, out var parsed));
            Assert.Equal(error.Code, parsed.Code);
            Assert.Equal(error.Reason, parsed.Reason);
            Assert.Equal(error.ReferenceError, parsed.ReferenceError);
            Assert.Equal(error.Version, parsed.Version);
        }

        [Fact]
        public void RoundTrip_catch_block_output_is_parsed_back()
        {
            var dict = SerializeCatchOutput(new StructuredErrorException("RESOURCE_UNAVAILABLE", "No port available"));

            Assert.True(StructuredErrorSerializer.TryParse(dict, out var parsed));
            Assert.Equal("RESOURCE_UNAVAILABLE", parsed.Code);
            Assert.Equal("No port available", parsed.Reason);
        }

        [Fact]
        public void TryParse_returns_false_when_key_absent()
        {
            var dict = new Dictionary<string, object> { ["error_message"] = "boom" };

            Assert.False(StructuredErrorSerializer.TryParse(dict, out var parsed));
            Assert.Null(parsed);
        }

        [Fact]
        public void TryParse_returns_false_on_null_input()
        {
            Assert.False(StructuredErrorSerializer.TryParse(null, out var parsed));
            Assert.Null(parsed);
        }

        [Fact]
        public void TryParse_returns_false_on_malformed_payload()
        {
            var dict = new Dictionary<string, object> { [StructuredErrorSerializer.OutputKey] = "not-a-structured-error" };

            Assert.False(StructuredErrorSerializer.TryParse(dict, out _));
        }

        [Fact]
        public void TryParse_returns_false_when_code_missing()
        {
            var dict = new Dictionary<string, object>
            {
                [StructuredErrorSerializer.OutputKey] = new Dictionary<string, object> { ["reason"] = "no code here" }
            };

            Assert.False(StructuredErrorSerializer.TryParse(dict, out _));
        }
    }
}
