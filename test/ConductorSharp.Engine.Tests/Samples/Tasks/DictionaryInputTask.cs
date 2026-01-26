namespace ConductorSharp.Engine.Tests.Samples.Tasks;

public class DictionaryInputTaskInput : ITaskInput<DictionaryInputTaskOutput>
{
    public IDictionary<string, object> Object { get; set; }
    public IDictionary<string, string> StringObject { get; set; }
    public IDictionary<string, int> IntObject { get; set; }
}

public class DictionaryInputTaskOutput;

public class DictionaryInputTask : SimpleTaskModel<DictionaryInputTaskInput, DictionaryInputTaskOutput>;
