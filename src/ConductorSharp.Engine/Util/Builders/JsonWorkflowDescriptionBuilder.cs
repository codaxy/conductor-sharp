using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConductorSharp.Engine.Util.Builders
{
    public class JsonWorkflowDescriptionBuilder : IWorkflowDescriptionBuilder
    {
        public string Build(BuildContext context) =>
            new JObject()
            {
                new JProperty("description", context.WorkflowOptions.Description),
                new JProperty("labels", context.WorkflowOptions.Labels)
            }.ToString(Formatting.None);
    }
}
