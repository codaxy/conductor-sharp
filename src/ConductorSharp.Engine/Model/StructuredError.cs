using System;
using ConductorSharp.Engine.Exceptions;

namespace ConductorSharp.Engine.Model
{
    /// <summary>
    /// Sanitized, structured error classification transported across the <c>structured_error</c> task-output key.
    /// The field shape is versioned via <see cref="Version"/> so it can evolve without silent misparses.
    /// </summary>
    public class StructuredError
    {
        /// <summary>Current structured-error payload shape version.</summary>
        public const int CurrentVersion = 1;

        /// <summary>Stable, opaque classification code (e.g. an implementation-defined code, or <c>UNCLASSIFIED</c>).</summary>
        public string Code { get; set; }

        /// <summary>Short, stable, sanitized reason. Consumers may key off this text, so keep it terse.</summary>
        public string Reason { get; set; }

        /// <summary>
        /// Diagnostic detail, longer and more specific than <see cref="Reason"/> — the explanation an
        /// operator needs, kept out of <see cref="Reason"/> so that stays short and stable. Mirrors
        /// <see cref="Exception.Message"/>, which falls back to the reason when the thrower supplied no
        /// distinct detail — so on the exception path the field is always populated, and consumers can
        /// read it without a reason fallback of their own.
        /// </summary>
        public string Message { get; set; }

        /// <summary>Optional URI pointing at the entity where the failure originated (drill-down link).</summary>
        public string ReferenceError { get; set; }

        /// <summary>Payload shape version marker. Defaults to <see cref="CurrentVersion"/>.</summary>
        public int Version { get; set; } = CurrentVersion;

        /// <summary>
        /// Maps a thrown exception onto the payload, returning <c>null</c> for anything that is not a
        /// <see cref="StructuredErrorException"/> so plain exceptions keep producing only <c>error_message</c>.
        /// This is the single exception-to-payload mapping: both execution managers and the contract tests call it,
        /// so the two poll strategies cannot drift apart as the shape evolves.
        /// </summary>
        public static StructuredError FromException(Exception exception)
        {
            if (exception is not StructuredErrorException structuredException)
                return null;

            return new StructuredError
            {
                Code = structuredException.Code,
                Reason = structuredException.Reason,
                Message = structuredException.Message,
                ReferenceError = structuredException.ReferenceError
            };
        }
    }
}
