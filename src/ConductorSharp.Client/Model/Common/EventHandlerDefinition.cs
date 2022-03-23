using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConductorSharp.Client.Model.Common;

public class StartWorkflowActionDetails
{
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("version")]
    public int Version { get; set; }
    [JsonProperty("input")]
    public JObject Input { get; set; }
}
public class CompleteTaskAction : Action
{
    [JsonProperty("action")]
    public string ActionType { get; set; } = "complete_task";
    [JsonProperty("complete_task")]
    public CompleteTaskActionDetails CompleteTask { get; set; }
}

public class CompleteTaskActionDetails
{
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("taskRefName")]
    public string TaskRefName { get; set; }
    [JsonProperty("output")]
    public JObject Output { get; set; }
}

public class StartWorkflowAction : Action
{
    [JsonProperty("action")]
    public string ActionType { get; set; } = "start_workflow";
    [JsonProperty("start_workflow")]
    public StartWorkflowActionDetails StartWorkflow { get; set; }
    [JsonProperty("expandInlineJSON")]
    public bool ExpandInlineJson { get; set; } = true;
}

public class FailTaskActionDetails
{
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("taskRefName")]
    public int TaskRefName { get; set; }
    [JsonProperty("output")]
    public JObject Output { get; set; }
}
public class FailTaskAction : Action
{
    [JsonProperty("action")]
    public string ActionType { get; set; } = "fail_task";
    [JsonProperty("expandInlineJSON")]
    public bool ExpandInlineJson { get; set; } = true;
    [JsonProperty("fail_task")]
    public FailTaskActionDetails FailTask { get; set; }
}

public abstract class Action
{
}

public class EventHandlerDefinition
{
    [JsonProperty("active")]
    public bool Active { get; set; } = true;
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("event")]
    public string Event { get; set; }
    [JsonProperty("condition")]
    public string Condition { get; set; }
    [JsonProperty("actions")]
    public Action[] Actions { get; set; }

    public static bool AreEqual(EventHandlerDefinition d1, EventHandlerDefinition d2)
    {
        var o1 = JsonConvert.SerializeObject(d1);
        var o2 = JsonConvert.SerializeObject(d2);

        return o1.Equals(o2);
    }
}
