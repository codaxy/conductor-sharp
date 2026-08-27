using System.Collections.Generic;
using ConductorSharp.Client;
using ConductorSharp.Engine.Model;
using Newtonsoft.Json;

namespace ConductorSharp.Engine.Util
{
    /// <summary>
    /// Defines the <c>structured_error</c> task-output contract (key + shape): the write side
    /// (<see cref="Serialize"/>) and the tolerant read side used by all consumers (<see cref="TryDeserialize"/>).
    /// The execution-manager catch block emits the same shape without this class, by serializing
    /// <see cref="ErrorOutput.StructuredError"/> with the same serializer settings when a worker throws a
    /// <see cref="ConductorSharp.Engine.Exceptions.StructuredErrorException"/>. Internal on purpose: consumers
    /// read the contract through <see cref="FailedTaskStructuredErrorReader"/>, and producers outside this
    /// assembly declare errors by throwing, not by rendering the payload themselves.
    /// </summary>
    internal static class StructuredErrorSerializer
    {
        /// <summary>Well-known task-output key carrying the structured error payload.</summary>
        public const string OutputKey = "structured_error";

        /// <summary>
        /// Renders a <see cref="StructuredError"/> to an output-data fragment (<c>{ "structured_error": { ... } }</c>)
        /// using the standard snake_case IO serializer settings. Returns an empty dictionary for a null error.
        /// </summary>
        public static IDictionary<string, object> Serialize(StructuredError error)
        {
            if (error == null)
                return new Dictionary<string, object>();

            var json = JsonConvert.SerializeObject(error, ConductorConstants.IoJsonSerializerSettings);
            var value = JsonConvert.DeserializeObject<IDictionary<string, object>>(json, ConductorConstants.IoJsonSerializerSettings);

            return new Dictionary<string, object> { [OutputKey] = value };
        }

        /// <summary>
        /// Tolerantly extracts a <see cref="StructuredError"/> from a failed task's output data. Presence-checks the
        /// single <see cref="OutputKey"/> and deserializes only that subtree. A missing, malformed, or code-less
        /// payload returns <c>false</c> and never throws, so a parse problem degrades error quality (falling back to
        /// the generic path) rather than failing the failure workflow.
        /// </summary>
        public static bool TryDeserialize(IDictionary<string, object> taskOutput, out StructuredError error)
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
