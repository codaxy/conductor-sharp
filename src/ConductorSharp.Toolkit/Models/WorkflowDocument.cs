namespace ConductorSharp.Toolkit.Models
{
    public class WorkflowDocument
    {
        public string SourcePath { get; set; } //path to .md file of the workflow
        public string DestinationPath { get; set; }
        public string WorkflowName { get; set; }
        public string WorkflowDescription { get; set; } //short description of workflow
        public List<string> DiagramCode { get; set; }
        public List<string> InputTable { get; set; }
        public List<string> TaskTable { get; set; }
        public List<string> OutputTable { get; set; }
        public string OwnerApp { get; set; }
        public string OwnerEmail { get; set; }
        public string Version { get; set; }
        public string FailureWorkflow { get; set; }
        public string RegistrationTime { get; set; }
    }
}
