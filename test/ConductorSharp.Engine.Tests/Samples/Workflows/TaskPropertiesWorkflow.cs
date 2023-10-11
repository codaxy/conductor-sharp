namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class TaskPropertiesWorkflowInput : WorkflowInput<TaskPropertiesWorkflowOutput> { }

    public class TaskPropertiesWorkflowOutput : WorkflowOutput { }

    public class TaskPropertiesWorkflow : Workflow<TaskPropertiesWorkflow, TaskPropertiesWorkflowInput, TaskPropertiesWorkflowOutput>
    {
        public CustomerGetV1 GetCustomer { get; set; }
        public TaskPropertiesTask TaskProperties { get; set; }

        public TaskPropertiesWorkflow(
            WorkflowDefinitionBuilder<TaskPropertiesWorkflow, TaskPropertiesWorkflowInput, TaskPropertiesWorkflowOutput> builder
        ) : base(builder) { }

        public override void BuildDefinition()
        {
            _builder.AddTask(wf => wf.GetCustomer, wf => new());
            _builder.AddTask(
                wf => wf.TaskProperties,
                wf =>
                    new()
                    {
                        Status = wf.GetCustomer.Status,
                        ReferenceTaskName = wf.GetCustomer.ReferenceTaskName,
                        StartTime = wf.GetCustomer.StartTime,
                        EndTime = wf.GetCustomer.EndTime,
                        ScheduledTime = wf.GetCustomer.ScheduledTime,
                        TaskDefName = wf.GetCustomer.TaskDefName,
                        TaskId = wf.GetCustomer.TaskId,
                        TaskType = wf.GetCustomer.TaskType
                    }
            );
        }
    }
}
