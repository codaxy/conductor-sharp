using System;

namespace ConductorSharp.Engine.Exceptions
{
    /// <summary>
    /// Thrown by a worker to attach a structured, sanitized error classification to the failed task's output.
    /// When caught by the execution manager, the <see cref="Code"/>/<see cref="Reason"/>/<see cref="ReferenceError"/>
    /// are serialized under the <c>structured_error</c> output key (see
    /// <see cref="ConductorSharp.Engine.Util.StructuredErrorSerializer"/>), in addition to the plain
    /// <c>error_message</c>, so downstream consumers can read a stable classification without parsing free-text
    /// reasons. Plain exceptions are unaffected and keep producing only <c>error_message</c>.
    /// </summary>
    public class StructuredErrorException : Exception
    {
        /// <summary>Stable, opaque classification code. Consumers map this to a failure response.</summary>
        public string Code { get; }

        /// <summary>Human-readable, sanitized reason. Safe to surface across a layer boundary.</summary>
        public string Reason { get; }

        /// <summary>Optional URI pointing at the entity where the failure originated (drill-down link).</summary>
        public string ReferenceError { get; }

        public StructuredErrorException(string code, string reason, string referenceError = null)
            : base(reason)
        {
            Code = code;
            Reason = reason;
            ReferenceError = referenceError;
        }

        public StructuredErrorException(string code, string reason, string referenceError, Exception innerException)
            : base(reason, innerException)
        {
            Code = code;
            Reason = reason;
            ReferenceError = referenceError;
        }
    }
}
