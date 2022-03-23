using System;

namespace ConductorSharp.Client.Util;

internal static class ApiUrls
{
    private readonly static Uri _getAllTaskDefinitions =
        new("metadata/taskdefs", UriKind.Relative);
    private readonly static Uri _createTaskDefinitions =
        new("metadata/taskdefs", UriKind.Relative);
    private readonly static Uri _updateTaskDefinition =
        new("metadata/taskdefs", UriKind.Relative);

    private readonly static Uri _updateTask = new("tasks", UriKind.Relative);
    private readonly static Uri _getAllQueues = new("tasks/queue/all", UriKind.Relative);

    private readonly static Uri _createWorkflowDefinition =
        new("metadata/workflow", UriKind.Relative);
    private readonly static Uri _updateWorkflowDefinition =
        new("metadata/workflow", UriKind.Relative);
    private readonly static Uri _getAllWorkflowDefinitions =
        new("metadata/workflow", UriKind.Relative);
    private readonly static Uri _createWorkflowDefinitions =
        new("metadata/workflow", UriKind.Relative);

    private readonly static Uri _queueWorkflow = new("workflow", UriKind.Relative);

    private readonly static Uri _getAllEventDefinitions = new("event", UriKind.Relative);
    private readonly static Uri _updateEventHandlerDefinition = new("event", UriKind.Relative);
    private readonly static Uri _createEventHandlerDefinition = new("event", UriKind.Relative);

    private readonly static Uri _health = new("health", UriKind.Relative);

    // External storage
    public static Uri GetExternalStorage(string filename) =>
        "external/postgres/{0}".ToRelativeUri(filename);
    // Tasks
    public static Uri GetAllTaskDefinitions() => _getAllTaskDefinitions;
    public static Uri CreateTaskDefinitions() => _createTaskDefinitions;
    public static Uri GetTaskDefinition(string taskType) =>
        "metadata/taskdefs/{0}".ToRelativeUri(taskType);
    public static Uri DeleteTaskDefinition(string taskType) =>
        "metadata/taskdefs/{0}".ToRelativeUri(taskType);
    public static Uri UpdateTaskDefinition() => _updateTaskDefinition;
    public static Uri BatchPollTasks(string name, string workerId, int count, int timeout) =>
        "tasks/poll/batch/{0}?workerId={1}&count={2}&timeout={3}".ToRelativeUri(
            name,
            workerId,
            count,
            timeout
        );
    public static Uri BatchPollTasks(
        string name,
        string workerId,
        int count,
        int timeout,
        string domain
    ) =>
        "tasks/poll/batch/{0}?workerId={1}&count={2}&timeout={3}&domain={4}".ToRelativeUri(
            name,
            workerId,
            count,
            timeout,
            domain
        );
    public static Uri PollTasks(string name, string workerId) =>
        "tasks/poll/{0}?workerId={1}".ToRelativeUri(name, workerId);
    public static Uri PollTasks(string name, string workerId, string domain) =>
        "tasks/poll/{0}?workerId={1}&domain={2}".ToRelativeUri(name, workerId, domain);
    public static Uri UpdateTask() => _updateTask;
    public static Uri SearchTask(int size, string query) =>
        "tasks/search?size={0}&query={1}".ToRelativeUri(size, query);
    public static Uri GetTaskQueue(string name) => "tasks/queue/{taskName}".ToRelativeUri(name);
    public static Uri GetAllQueues() => _getAllQueues;

    // External storage
    public static Uri FetchExternalStorageLocation(string name) =>
        "tasks/externalstoragelocation?path={0}&operation=READ&payloadType=TASK_INPUT".ToRelativeUri(
            name
        );

    // Workflows
    public static Uri CreateWorkflowDefinition() => _createWorkflowDefinition;
    public static Uri GetWorkflowDefinition(string name, int version) =>
        "metadata/workflow/{0}?version={1}".ToRelativeUri(name, version);
    public static Uri UpdateWorkflowDefinition() => _updateWorkflowDefinition;
    public static Uri DeleteWorkflowDefinition(string name, int version) =>
        "metadata/workflow/{0}/{1}".ToRelativeUri(name, version);
    public static Uri GetAlleWorkflowDefinitions() => _getAllWorkflowDefinitions;
    public static Uri CreateWorkflowDefinitions() => _createWorkflowDefinitions;
    public static Uri QueueWorkflow() => _queueWorkflow;
    public static Uri GetWorkflowStatus(string workflowId, bool includeTasks)
    {
        if (includeTasks)
            return "workflow/{0}?includeTasks=true".ToRelativeUri(workflowId);
        else
            return "workflow/{0}?includeTasks=false".ToRelativeUri(workflowId);
    }

    // Events
    public static Uri GetAllEventDefinitions() => _getAllEventDefinitions;
    public static Uri UpdateEventHandlerDefinition() => _updateEventHandlerDefinition;
    public static Uri DeleteEventHandlerDefinition(string name) =>
        "event/{0}".ToRelativeUri(name);
    public static Uri CreateEventHandlerDefinition() => _createEventHandlerDefinition;
    public static Uri GetEventHandlerDefinition(string name) => "event/{0}".ToRelativeUri(name);

    // Health
    public static Uri Health() => _health;
}
