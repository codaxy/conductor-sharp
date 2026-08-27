using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Client.Service;
using ConductorSharp.Engine.Model;
using TaskStatus = ConductorSharp.Client.Generated.TaskStatus;

namespace ConductorSharp.Engine.Util
{
    /// <summary>
    /// Walks a failed workflow execution through sub-workflows to the deepest failed task and reads the
    /// structured error it declared via the shared <c>structured_error</c> task-output contract
    /// (<see cref="StructuredErrorSerializer"/>).
    /// <para>
    /// This is the single reader for the contract: consumers that harvest a failure classification out of an
    /// execution (failure workflows, notification builders, drill-downs) should use it instead of re-implementing
    /// the descent, so they cannot drift apart on which task "the" failure is.
    /// </para>
    /// </summary>
    public class FailedTaskStructuredErrorReader
    {
        private readonly IWorkflowService _workflowService;

        public FailedTaskStructuredErrorReader(IWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }

        /// <summary>
        /// Reads the structured error declared by the deepest failed task of <paramref name="workflowId"/>,
        /// following the Try pattern: returns <c>false</c> (with a <c>null</c> <paramref name="error"/>) when
        /// the execution has no failed task or the failed task declared nothing. Synchronous — an <c>out</c>
        /// parameter rules out <c>async</c> — so the underlying Conductor call blocks the calling thread.
        /// </summary>
        public bool TryRead(string workflowId, out StructuredError error, CancellationToken cancellationToken)
        {
            var failed = FindDeepestFailedTaskAsync(workflowId, cancellationToken).GetAwaiter().GetResult();

            if (failed?.OutputData != null && StructuredErrorSerializer.TryDeserialize(failed.OutputData, out var structured))
            {
                error = structured;
                return true;
            }

            error = null;
            return false;
        }

        /// <summary>
        /// Like <see cref="TryRead"/>, but always yields an error: a failure that declared nothing is
        /// classified as <see cref="StructuredError.UnclassifiedCode"/> with <paramref name="genericReason"/> as the
        /// sanitized reason, so raw internals never cross a boundary by default. The returned
        /// <see cref="StructuredError.Message"/> always carries the most specific diagnostic available: the declared
        /// message when there is one, otherwise an internal locator (workflow id, task id, reference name, raw
        /// reason) suitable for drill-down.
        /// </summary>
        /// <param name="workflowId">Execution to walk.</param>
        /// <param name="genericReason">
        /// Sanitized reason used when the failure declared no reason of its own. Callers own this text — it is
        /// what crosses their boundary.
        /// </param>
        /// <param name="fallbackReason">
        /// Optional raw failure reason already known to the caller (e.g. the failure workflow's input); used only
        /// inside the diagnostic message, never as the sanitized reason.
        /// </param>
        public async Task<StructuredError> ReadOrFallbackAsync(
            string workflowId,
            string genericReason,
            string fallbackReason,
            CancellationToken cancellationToken
        )
        {
            var failed = await FindDeepestFailedTaskAsync(workflowId, cancellationToken);

            if (failed?.OutputData != null && StructuredErrorSerializer.TryDeserialize(failed.OutputData, out var structured))
            {
                return new StructuredError
                {
                    Code = structured.Code,
                    Reason = string.IsNullOrEmpty(structured.Reason) ? genericReason : structured.Reason,
                    Message = string.IsNullOrEmpty(structured.Message) ? BuildDiagnosticMessage(failed, fallbackReason) : structured.Message,
                    ReferenceError = structured.ReferenceError,
                    Version = structured.Version
                };
            }

            return new StructuredError
            {
                Code = StructuredError.UnclassifiedCode,
                Reason = genericReason,
                Message = BuildDiagnosticMessage(failed, fallbackReason)
            };
        }

        /// <summary>
        /// Walks the execution to the task that actually failed. A phase often runs as a SUB_WORKFLOW inside a
        /// dynamic fork, and the aggregating JOIN task is frequently the <i>last</i> failed task in the parent —
        /// so any failed sub-workflow is descended into first (walking from the last), and FORK/JOIN aggregators
        /// are skipped when picking a leaf. Returns <c>null</c> when the execution has no failed task.
        /// </summary>
        public async Task<ConductorSharp.Client.Generated.Task> FindDeepestFailedTaskAsync(string workflowId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(workflowId))
                return null;

            var workflow = await _workflowService.GetExecutionStatusAsync(workflowId, true, cancellationToken);

            var failedTasks = (workflow.Tasks ?? []).Where(t => t.Status is TaskStatus.FAILED or TaskStatus.FAILED_WITH_TERMINAL_ERROR).ToList();

            if (failedTasks.Count == 0)
                return null;

            for (var i = failedTasks.Count - 1; i >= 0; i--)
            {
                var subWorkflowId = failedTasks[i].SubWorkflowId;
                if (!string.IsNullOrEmpty(subWorkflowId))
                {
                    var deeper = await FindDeepestFailedTaskAsync(subWorkflowId, cancellationToken);
                    if (deeper != null)
                        return deeper;
                }
            }

            return failedTasks.LastOrDefault(t => t.TaskType is not ("JOIN" or "FORK")) ?? failedTasks[^1];
        }

        private static string BuildDiagnosticMessage(ConductorSharp.Client.Generated.Task task, string fallbackReason)
        {
            if (task == null)
                return fallbackReason ?? "No failed task found.";

            var raw = task.ReasonForIncompletion ?? fallbackReason;
            return $"workflowId={task.WorkflowInstanceId}; taskId={task.TaskId}; ref={task.ReferenceTaskName}; reason={raw}";
        }
    }
}
