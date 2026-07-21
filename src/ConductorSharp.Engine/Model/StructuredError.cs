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

        /// <summary>Human-readable, sanitized reason.</summary>
        public string Reason { get; set; }

        /// <summary>Optional URI pointing at the entity where the failure originated (drill-down link).</summary>
        public string ReferenceError { get; set; }

        /// <summary>Payload shape version marker. Defaults to <see cref="CurrentVersion"/>.</summary>
        public int Version { get; set; } = CurrentVersion;
    }
}
