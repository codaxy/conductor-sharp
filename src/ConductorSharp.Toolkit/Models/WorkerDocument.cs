using System.Collections;

namespace ConductorSharp.Toolkit.Models
{
    public class WorkerDocument
    {
        public WorkerDocument()
        {
            InputTable = new List<string>();
            OutputTable = new List<string>();
            UpdatedWorkers = new Hashtable();
        }

        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public string WorkerName { get; set; }
        public string WorkerDescription { get; set; } //short description of worker
        public List<string> InputTable { get; set; }
        public List<string> OutputTable { get; set; }
        public Hashtable UpdatedWorkers { get; set; } //to prevent us from adding the same task twice or more, because it can happen that several workflows use the same worker
        public string OwnerApp { get; set; }
        public string OwnerEmail { get; set; }
        public string RegistrationTime { get; set; }
    }
}
