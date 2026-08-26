using System;
using System.Collections.Generic;
using System.Threading;
using ConductorSharp.Client.Generated;
using ConductorSharp.Client.Service;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util;
using Xunit;
using GeneratedTask = ConductorSharp.Client.Generated.Task;
using Task = System.Threading.Tasks.Task;
using TaskStatus = ConductorSharp.Client.Generated.TaskStatus;

namespace ConductorSharp.Engine.Tests.Unit
{
    public class FailedTaskStructuredErrorReaderTests
    {
        private const string GenericReason = "The request could not be completed.";

        // Serves executions from an in-memory map so the descent through sub-workflows can be exercised
        // without a Conductor server. Only GetExecutionStatusAsync is meaningful.
        private sealed class FakeWorkflowService(Dictionary<string, Workflow> executions) : IWorkflowService
        {
            public Task<Workflow> GetExecutionStatusAsync(
                string workflowId,
                bool? includeTasks = false,
                CancellationToken cancellationToken = default
            ) => Task.FromResult(executions[workflowId]);

            public Task DecideAsync(string workflowId, CancellationToken cancellationToken = default) => throw new NotImplementedException();

            public Task DeleteAsync(string workflowId, bool? archiveWorkflow = null, CancellationToken cancellationToken = default) =>
                throw new NotImplementedException();

            public Task<IDictionary<string, ICollection<Workflow>>> GetCorrelatedAsync(
                string name,
                IEnumerable<string> correlationIds,
                bool? includeClosed = false,
                bool? includeTasks = false,
                CancellationToken cancellationToken = default
            ) => throw new NotImplementedException();

            public Task<ICollection<Workflow>> ListCorrelatedAsync(
                string name,
                string correlationId,
                bool? includeClosed = false,
                bool? includeTasks = false,
                CancellationToken cancellationToken = default
            ) => throw new NotImplementedException();

            public Task GetExternalStorageLocationAsync(
                string path,
                string operation,
                string payloadType,
                CancellationToken cancellationToken = default
            ) => throw new NotImplementedException();

            public Task<ICollection<string>> ListRunningAsync(
                string name,
                int? version,
                long? startTime = null,
                long? endTime = null,
                CancellationToken cancellationToken = default
            ) => throw new NotImplementedException();

            public Task PauseAsync(string workflowId, CancellationToken cancellationToken = default) => throw new NotImplementedException();

            public Task<string> RerunAsync(
                string workflowId,
                RerunWorkflowRequest rerunWorkflowRequest,
                CancellationToken cancellationToken = default
            ) => throw new NotImplementedException();

            public Task ResetCallbacksAsync(string workflowId, CancellationToken cancellationToken = default) => throw new NotImplementedException();

            public Task RestartAsync(string workflowId, bool? useLatestDefinitions = null, CancellationToken cancellationToken = default) =>
                throw new NotImplementedException();

            public Task ResumeAsync(string workflowId, CancellationToken cancellationToken = default) => throw new NotImplementedException();

            public Task RetryAsync(string workflowId, bool? resumeSubworkflowTasks = null, CancellationToken cancellationToken = default) =>
                throw new NotImplementedException();

            public Task<SearchResultWorkflowSummary> SearchAsync(
                int? start = null,
                int? size = null,
                string sort = null,
                string freeText = null,
                string query = null,
                CancellationToken cancellationToken = default
            ) => throw new NotImplementedException();

            public Task<SearchResultWorkflowSummary> SearchByTasksAsync(
                int? start = null,
                int? size = null,
                string sort = null,
                string freeText = null,
                string query = null,
                CancellationToken cancellationToken = default
            ) => throw new NotImplementedException();

            public Task<SearchResultWorkflow> SearchV2Async(
                int? start = null,
                int? size = null,
                string sort = null,
                string freeText = null,
                string query = null,
                CancellationToken cancellationToken = default
            ) => throw new NotImplementedException();

            public Task<SearchResultWorkflow> SearchV2ByTasksAsync(
                int? start = null,
                int? size = null,
                string sort = null,
                string freeText = null,
                string query = null,
                CancellationToken cancellationToken = default
            ) => throw new NotImplementedException();

            public Task SkipTaskAsync(
                string workflowId,
                string taskReferenceName,
                SkipTaskRequest skipTaskRequest,
                CancellationToken cancellationToken = default
            ) => throw new NotImplementedException();

            public Task<string> StartAsync(StartWorkflowRequest startWorkflowRequest, CancellationToken cancellationToken = default) =>
                throw new NotImplementedException();

            public Task TerminateAsync(string workflowId, string reason = null, CancellationToken cancellationToken = default) =>
                throw new NotImplementedException();

            public Task<Workflow> TestAsync(WorkflowTestRequest workflowTestRequest, CancellationToken cancellationToken = default) =>
                throw new NotImplementedException();
        }

        private static FailedTaskStructuredErrorReader Reader(Dictionary<string, Workflow> executions) => new(new FakeWorkflowService(executions));

        private static GeneratedTask FailedTask(
            string taskType = "SIMPLE",
            string subWorkflowId = null,
            IDictionary<string, object> outputData = null,
            string reason = null,
            string taskId = null,
            string reference = null,
            string workflowInstanceId = null
        ) =>
            new()
            {
                Status = TaskStatus.FAILED,
                TaskType = taskType,
                SubWorkflowId = subWorkflowId,
                OutputData = outputData,
                ReasonForIncompletion = reason,
                TaskId = taskId,
                ReferenceTaskName = reference,
                WorkflowInstanceId = workflowInstanceId
            };

        private static Workflow Execution(params GeneratedTask[] tasks) => new() { Tasks = tasks };

        [Fact]
        public async Task TryRead_returns_the_declared_structured_error()
        {
            var output = StructuredErrorSerializer.ToOutputData(new StructuredError { Code = "RESOURCE_UNAVAILABLE", Reason = "No port available" });
            var reader = Reader(new() { ["wf"] = Execution(FailedTask(outputData: output)) });

            var error = await reader.TryReadAsync("wf", CancellationToken.None);

            Assert.NotNull(error);
            Assert.Equal("RESOURCE_UNAVAILABLE", error.Code);
            Assert.Equal("No port available", error.Reason);
        }

        [Fact]
        public async Task TryRead_returns_null_when_the_failed_task_declared_nothing()
        {
            var reader = Reader(new() { ["wf"] = Execution(FailedTask(reason: "raw internals")) });

            Assert.Null(await reader.TryReadAsync("wf", CancellationToken.None));
        }

        [Fact]
        public async Task TryRead_returns_null_when_nothing_failed()
        {
            var reader = Reader(new() { ["wf"] = Execution() });

            Assert.Null(await reader.TryReadAsync("wf", CancellationToken.None));
        }

        [Fact]
        public async Task Descends_into_the_failed_sub_workflow_instead_of_stopping_on_the_join()
        {
            // Parent: a failed SUB_WORKFLOW and the aggregating JOIN that failed after it. The declared error
            // lives on the leaf task inside the child execution.
            var output = StructuredErrorSerializer.ToOutputData(new StructuredError { Code = "LEAF", Reason = "leaf failed" });
            var reader = Reader(
                new()
                {
                    ["parent"] = Execution(FailedTask(taskType: "SUB_WORKFLOW", subWorkflowId: "child"), FailedTask(taskType: "JOIN")),
                    ["child"] = Execution(FailedTask(outputData: output))
                }
            );

            var error = await reader.TryReadAsync("parent", CancellationToken.None);

            Assert.NotNull(error);
            Assert.Equal("LEAF", error.Code);
        }

        [Fact]
        public async Task Skips_fork_and_join_aggregators_when_picking_the_leaf()
        {
            var output = StructuredErrorSerializer.ToOutputData(new StructuredError { Code = "SIMPLE_LEAF", Reason = "r" });
            var reader = Reader(
                new() { ["wf"] = Execution(FailedTask(taskType: "FORK"), FailedTask(outputData: output), FailedTask(taskType: "JOIN")) }
            );

            var error = await reader.TryReadAsync("wf", CancellationToken.None);

            Assert.NotNull(error);
            Assert.Equal("SIMPLE_LEAF", error.Code);
        }

        [Fact]
        public async Task ReadOrFallback_falls_back_to_unclassified_with_the_sanitized_generic_reason()
        {
            var reader = Reader(
                new() { ["wf"] = Execution(FailedTask(reason: "raw stack trace", taskId: "t1", reference: "ref1", workflowInstanceId: "wf")) }
            );

            var error = await reader.ReadOrFallbackAsync("wf", GenericReason, fallbackReason: null, CancellationToken.None);

            Assert.Equal(StructuredError.UnclassifiedCode, error.Code);
            Assert.Equal(GenericReason, error.Reason);
            // Raw internals go into the diagnostic message for drill-down, never into the reason.
            Assert.Contains("taskId=t1", error.Message);
            Assert.Contains("raw stack trace", error.Message);
        }

        [Fact]
        public async Task ReadOrFallback_keeps_the_declared_message_and_fills_a_diagnostic_one_when_absent()
        {
            var withMessage = StructuredErrorSerializer.ToOutputData(
                new StructuredError
                {
                    Code = "C",
                    Reason = "r",
                    Message = "declared detail"
                }
            );
            var withoutMessage = new StructuredError { Code = "C", Reason = "r" };
            withoutMessage.Message = null;
            var reader = Reader(
                new()
                {
                    ["with"] = Execution(FailedTask(outputData: withMessage)),
                    ["without"] = Execution(FailedTask(outputData: StructuredErrorSerializer.ToOutputData(withoutMessage), taskId: "t9"))
                }
            );

            Assert.Equal("declared detail", (await reader.ReadOrFallbackAsync("with", GenericReason, null, CancellationToken.None)).Message);
            Assert.Contains("taskId=t9", (await reader.ReadOrFallbackAsync("without", GenericReason, null, CancellationToken.None)).Message);
        }

        [Fact]
        public async Task ReadOrFallback_with_no_workflow_id_still_returns_a_classification()
        {
            var reader = Reader(new());

            var error = await reader.ReadOrFallbackAsync(null, GenericReason, "reason from failure workflow input", CancellationToken.None);

            Assert.Equal(StructuredError.UnclassifiedCode, error.Code);
            Assert.Equal(GenericReason, error.Reason);
            Assert.Equal("reason from failure workflow input", error.Message);
        }
    }
}
