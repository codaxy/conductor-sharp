namespace ConductorSharp.Engine.Tests.Samples.Tasks
{
    public class ArrayTaskInput : ITaskInput<ArrayTaskOutput>
    {
        public class TestModel
        {
            public string String { get; set; }
        }

        public int[] Integers { get; set; }
        public List<TestModel> TestModelList { get; set; }
        public TestModel[] Models { get; set; }
        public object Objects { get; set; }
    }

    public class ArrayTaskOutput;

    public class ArrayTask : SimpleTaskModel<ArrayTaskInput, ArrayTaskOutput>;
}
