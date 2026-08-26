using System;

namespace ConductorSharp.Engine.Exceptions
{
    /// <summary>
    /// Thrown by a worker to attach a structured, sanitized error classification to the failed task's output.
    /// When caught by the execution manager, the <see cref="Code"/>/<see cref="Reason"/>/<see cref="ReferenceError"/>
    /// and the diagnostic message are serialized under the <c>structured_error</c> output key (see
    /// <see cref="ConductorSharp.Engine.Util.StructuredErrorSerializer"/>), in addition to the plain
    /// <c>error_message</c>, so downstream consumers can read a stable classification without parsing free-text
    /// reasons. Plain exceptions are unaffected and keep producing only <c>error_message</c>.
    /// </summary>
    /// <remarks>
    /// There is deliberately no <c>Message</c> property here: the diagnostic message is carried by the inherited
    /// <see cref="Exception.Message"/>, which the message-taking constructors set. When no message is supplied it
    /// falls back to <see cref="Reason"/>, matching the behaviour of the original constructors.
    /// </remarks>
    public class StructuredErrorException : Exception
    {
        /// <summary>Stable, opaque classification code. Consumers map this to a failure response.</summary>
        public string Code { get; }

        /// <summary>Short, stable, sanitized reason. Safe to surface across a layer boundary.</summary>
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

        /// <summary>
        /// Declares a diagnostic <paramref name="message"/> distinct from the short, stable
        /// <paramref name="reason"/>. Pass <c>null</c> for <paramref name="message"/> to fall back to the reason,
        /// and <c>null</c> for <paramref name="referenceError"/> when there is no entity to drill down into.
        /// </summary>
        /// <remarks>
        /// <paramref name="message"/> trails <paramref name="referenceError"/> rather than following
        /// <paramref name="reason"/> on purpose. Overload resolution cannot choose between
        /// this constructor and the <see cref="Exception"/> one when the fourth argument is an untyped <c>null</c>,
        /// so the nullable parameter is placed third, where it is typed the same either way. The only call this
        /// leaves ambiguous is <c>(code, reason, referenceError, null)</c> — a declared-but-null inner exception,
        /// which the three-argument constructor already expresses. Disambiguate with a named argument if needed.
        /// </remarks>
        public StructuredErrorException(string code, string reason, string referenceError, string message)
            : base(message ?? reason)
        {
            Code = code;
            Reason = reason;
            ReferenceError = referenceError;
        }

        /// <inheritdoc cref="StructuredErrorException(string, string, string, string)"/>
        public StructuredErrorException(string code, string reason, string referenceError, string message, Exception innerException)
            : base(message ?? reason, innerException)
        {
            Code = code;
            Reason = reason;
            ReferenceError = referenceError;
        }
    }
}
