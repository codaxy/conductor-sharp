using System.Collections.Generic;
using ConductorSharp.Client;
using ConductorSharp.Engine.Model;
using Newtonsoft.Json;

namespace ConductorSharp.Engine.Util
{
    /// <summary>
    /// Defines the <c>structured_error</c> task-output contract (key + shape) and the read side used by all consumers
    /// (<see cref="TryParse"/>). There are two producers, both emitting the same shape because they serialize the same
    /// <see cref="StructuredError"/> type with the same serializer settings:
    /// <list type="bullet">
    /// <item>the execution-manager catch block, which serializes <see cref="ErrorOutput.StructuredError"/> when a
    /// worker throws a <see cref="ConductorSharp.Engine.Exceptions.StructuredErrorException"/>; and</item>
    /// <item>external signal senders (which have no exception to catch), which render via <see cref="ToOutputData"/>.</item>
    /// </list>
    /// A round-trip contract test pins both producers to <see cref="TryParse"/> so the key/shape cannot drift.
    /// </summary>
    public static class StructuredErrorSerializer
    {
        /// <summary>Well-known task-output key carrying the structured error payload.</summary>
        public const string OutputKey = "structured_error";

        /// <summary>
        /// Renders a <see cref="StructuredError"/> to an output-data fragment (<c>{ "structured_error": { ... } }</c>)
        /// using the standard snake_case IO serializer settings. Intended for producers with no exception to catch —
        /// signal senders on a signal-based wait path merge this into the failed WAIT task's outputData. (The
        /// execution-manager catch block does not use this; it serializes <see cref="ErrorOutput.StructuredError"/>
        /// directly.) Returns an empty dictionary for a null error.
        /// </summary>
        public static IDictionary<string, object> ToOutputData(StructuredError error)
        {
            if (error == null)
                return new Dictionary<string, object>();

            return new Dictionary<string, object> { [OutputKey] = ToOutputValue(error) };
        }

        /// <summary>Renders just the value placed under <see cref="OutputKey"/>, in the canonical snake_case shape.</summary>
        public static object ToOutputValue(StructuredError error)
        {
            var json = JsonConvert.SerializeObject(error, ConductorConstants.IoJsonSerializerSettings);
            return JsonConvert.DeserializeObject<IDictionary<string, object>>(json, ConductorConstants.IoJsonSerializerSettings);
        }

        /// <summary>
        /// Tolerantly extracts a <see cref="StructuredError"/> from a failed task's output data. Presence-checks the
        /// single <see cref="OutputKey"/> and deserializes only that subtree. A missing, malformed, or code-less
        /// payload returns <c>false</c> and never throws, so a parse problem degrades error quality (falling back to
        /// the generic path) rather than failing the failure workflow.
        /// </summary>
        public static bool TryParse(IDictionary<string, object> taskOutput, out StructuredError error)
        {
            error = null;

            if (taskOutput == null || !taskOutput.TryGetValue(OutputKey, out var raw) || raw == null)
                return false;

            try
            {
                // raw may be a JObject (Newtonsoft round-trip), a nested dictionary, or a raw JSON string.
                var json = raw is string s ? s : JsonConvert.SerializeObject(raw, ConductorConstants.IoJsonSerializerSettings);
                var parsed = JsonConvert.DeserializeObject<StructuredError>(json, ConductorConstants.IoJsonSerializerSettings);

                // A structured error is only meaningful with a classification code; anything else is treated as
                // unstructured and degraded to the generic fallback by the caller.
                if (parsed == null || string.IsNullOrEmpty(parsed.Code))
                    return false;

                error = parsed;
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
