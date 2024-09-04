namespace ConductorSharp.Engine.Tests.Samples.Tasks;

public class DictionaryInputTaskInput : IRequest<DictionaryInputTaskOutput>
{
    public IDictionary<string, object> Input { get; set; }
}

public class DictionaryInputTaskOutput;

public class DictionaryInputTask : SimpleTaskModel<DictionaryInputTaskInput, DictionaryInputTaskOutput>;
