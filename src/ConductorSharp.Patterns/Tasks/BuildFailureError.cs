using System;
using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Engine;
using ConductorSharp.Engine.Builders.Metadata;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util;
using MediatR;

namespace ConductorSharp.Patterns.Tasks
{
    #region models
    public class BuildFailureErrorRequest : IRequest<BuildFailureErrorResponse>
    {
        /// <summary>
        /// Id of the failed workflow execution to classify (typically the failure workflow's <c>workflowId</c> input).
        /// </summary>
        public string? WorkflowId { get; set; }

        /// <summary>
        /// Sanitized reason used when the failure declared no reason of its own. This is the text that crosses the
        /// caller's boundary — supply domain wording here; defaults to a neutral generic.
        /// </summary>
        public string? GenericReason { get; set; }

        /// <summary>
        /// Optional raw failure reason already known to the caller (e.g. the failure workflow's <c>reason</c> input);
        /// used only inside the diagnostic message, never as the sanitized reason.
        /// </summary>
        public string? FallbackReason { get; set; }
    }

    public record BuildFailureErrorResponse(StructuredError Error);

    #endregion

    /// <summary>
    /// Walks the given failed execution to its deepest failed task (via
    /// <see cref="FailedTaskStructuredErrorReader"/>) and returns the structured error it declared — or the
    /// <see cref="StructuredError.UnclassifiedCode"/> fallback with the sanitized <c>GenericReason</c> when it
    /// declared nothing. Intended for failure workflows that persist a failure classification atomically with the
    /// failed state.
    /// <para>
    /// Registered by <c>AddConductorSharpPatterns</c> under the shared task name. The task is a stateless,
    /// read-only Conductor API lookup, so on a shared cluster it is safe for multiple services to register
    /// and poll the same queue — whichever picks the task up produces the same result. Use Conductor task
    /// domains if poller isolation is ever required.
    /// </para>
    /// </summary>
    [OriginalName(Constants.TaskNamePrefix + "_build_failure_error")]
    public class BuildFailureError(FailedTaskStructuredErrorReader errorReader)
        : TaskRequestHandler<BuildFailureErrorRequest, BuildFailureErrorResponse>
    {
        public const string DefaultGenericReason = "The workflow failed.";

        private readonly FailedTaskStructuredErrorReader _errorReader = errorReader;

        public override async Task<BuildFailureErrorResponse> Handle(BuildFailureErrorRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.WorkflowId))
                throw new Exception("No workflowId provided");

            var error = await _errorReader.ReadOrFallbackAsync(
                request.WorkflowId,
                string.IsNullOrEmpty(request.GenericReason) ? DefaultGenericReason : request.GenericReason,
                request.FallbackReason,
                cancellationToken
            );

            return new BuildFailureErrorResponse(error);
        }
    }
}
