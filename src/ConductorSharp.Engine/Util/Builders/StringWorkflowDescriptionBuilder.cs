namespace ConductorSharp.Engine.Util.Builders
{
    public class StringWorkflowDescriptionBuilder : IWorkflowDescriptionBuilder
    {
        public string Build(BuildContext context) => context.WorkflowOptions.Description;
    }
}
