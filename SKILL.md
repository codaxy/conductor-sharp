---
name: conductor-sharp
description: Comprehensive guide for using ConductorSharp library to build Conductor workflows in .NET. Use when creating task handlers, workflow definitions, configuring execution engines, scaffolding definitions, or integrating ConductorSharp into .NET projects. Covers all task types, client services, patterns package, and toolkit usage.
---

# ConductorSharp Library Guide

Complete guide for building Conductor workflows using ConductorSharp's strongly-typed DSL, task handlers, and execution engine.

## Quick Reference

### Packages
- `ConductorSharp.Client` - API client
- `ConductorSharp.Engine` - Workflow engine, builder DSL, handlers
- `ConductorSharp.Patterns` - Built-in tasks (WaitSeconds, ReadWorkflowTasks, C# Lambda)
- `ConductorSharp.KafkaCancellationNotifier` - Kafka cancellation support
- `ConductorSharp.Toolkit` - CLI scaffolding tool

## Project Setup

### Adding to Existing Project

```csharp
// Install packages
dotnet add package ConductorSharp.Client
dotnet add package ConductorSharp.Engine
```

### Creating New Console Project

```bash
dotnet new console -n MyConductorApp
cd MyConductorApp
dotnet add package ConductorSharp.Client
dotnet add package ConductorSharp.Engine
```

### Basic Configuration

```csharp
using ConductorSharp.Engine.Extensions;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services
    .AddConductorSharp(baseUrl: "http://localhost:8080")
    .AddExecutionManager(
        maxConcurrentWorkers: 10,
        sleepInterval: 500,
        longPollInterval: 100,
        domain: null,
        typeof(Program).Assembly
    );

builder.Services.RegisterWorkflow<MyWorkflow>();

var host = builder.Build();
await host.RunAsync();
```

## Writing Task Handlers


```csharp
using ConductorSharp.Engine.Builders.Metadata;
using ConductorSharp.Engine;
using ConductorSharp.Engine.Util;

[OriginalName("MY_TASK_name")]
public class MyTaskHandler : TaskRequestHandler<MyTaskRequest, MyTaskResponse>
{
    private readonly ConductorSharpExecutionContext _context;
    
    public MyTaskHandler(ConductorSharpExecutionContext context)
    {
        _context = context; // Access workflow/task metadata
    }

    public override async Task<MyTaskResponse> Handle(MyTaskRequest request, CancellationToken cancellationToken)
    {
        // Access context: _context.WorkflowId, _context.TaskId, _context.CorrelationId
        return new MyTaskResponse { /* ... */ };
    }
}
```

### Request/Response Models

```csharp
public class MyTaskRequest : IRequest<MyTaskResponse>
{
    [Required]
    public string InputValue { get; set; }
}

public class MyTaskResponse
{
    public string OutputValue { get; set; }
}
```

### Registering Standalone Tasks

```csharp
services.RegisterWorkerTask<MyTaskHandler>(options =>
{
    options.OwnerEmail = "team@example.com";
    options.Description = "My task description";
});
```

## Writing Workflow Definitions

### Basic Structure

```csharp
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Builders.Metadata;

public class MyWorkflowInput : WorkflowInput<MyWorkflowOutput>
{
    public int CustomerId { get; set; }
}

public class MyWorkflowOutput : WorkflowOutput
{
    public string Result { get; set; }
}

[OriginalName("MY_workflow")]
[WorkflowMetadata(OwnerEmail = "team@example.com")]
public class MyWorkflow : Workflow<MyWorkflow, MyWorkflowInput, MyWorkflowOutput>
{
    public MyWorkflow(WorkflowDefinitionBuilder<MyWorkflow, MyWorkflowInput, MyWorkflowOutput> builder) 
        : base(builder) { }

    // Task properties
    public SomeTaskHandler FirstTask { get; set; }
    public AnotherTaskHandler SecondTask { get; set; }

    public override void BuildDefinition()
    {
        _builder.AddTask(wf => wf.FirstTask, wf => new SomeTaskRequest { Input = wf.WorkflowInput.CustomerId });
        _builder.AddTask(wf => wf.SecondTask, wf => new AnotherTaskRequest { Input = wf.FirstTask.Output.Result });
        
        _builder.SetOutput(wf => new MyWorkflowOutput { Result = wf.SecondTask.Output.Value });
    }
}
```

### Workflow Metadata

```csharp
[WorkflowMetadata(
    OwnerEmail = "team@example.com",
    OwnerApp = "my-app",
    Description = "Workflow description",
    FailureWorkflow = typeof(FailureHandlerWorkflow)
)]
```

### Versioning

```csharp
[Version(2)]  // Version number for sub-workflow references
public class MyWorkflow : Workflow<...> { }
```

## Task Types

### Simple Task

```csharp
public MyTaskHandler MyTask { get; set; }

_builder.AddTask(wf => wf.MyTask, wf => new MyTaskRequest { InputValue = wf.WorkflowInput.Value });
```

### Sub-Workflow Task

Sub-workflows allow referencing other workflows as tasks. Define a model class that inherits from `SubWorkflowTaskModel`:

```csharp
// Define the sub-workflow model (usually scaffolded or defined separately)
[OriginalName("CHILD_workflow")]
public class ChildWorkflow : SubWorkflowTaskModel<ChildWorkflowInput, ChildWorkflowOutput> { }

// In the parent workflow:
public ChildWorkflow ChildWorkflow { get; set; }

_builder.AddTask(wf => wf.ChildWorkflow, wf => new ChildWorkflowInput { CustomerId = wf.WorkflowInput.CustomerId });
```

### Switch Task (Conditional Branching)

The Switch task evaluates a case value and executes tasks in the matching branch:

```csharp
public SwitchTaskModel SwitchTask { get; set; }
public CustomerGetHandler GetCustomer { get; set; }
public TerminateTaskModel Terminate { get; set; }

_builder.AddTask(
    wf => wf.SwitchTask,
    wf => new SwitchTaskInput { SwitchCaseValue = wf.WorkflowInput.Operation },
    new DecisionCases<MyWorkflow>
    {
        ["process"] = builder => builder.AddTask(wf => wf.GetCustomer, wf => new CustomerGetRequest { CustomerId = 1 }),
        ["skip"] = builder => { /* skip processing - no tasks */ },
        DefaultCase = builder => builder.AddTask(wf => wf.Terminate, wf => new TerminateTaskInput { TerminationStatus = TerminationStatus.Failed })
    }
);
```

### Decision Task (Deprecated - Use Switch)

```csharp
#pragma warning disable CS0618
public DecisionTaskModel Decision { get; set; }
public CustomerGetHandler GetCustomer { get; set; }
public TerminateTaskModel Terminate { get; set; }

_builder.AddTask(
    wf => wf.Decision,
    wf => new DecisionTaskInput { CaseValueParam = "test" },
    new DecisionCases<MyWorkflow>
    {
        ["test"] = builder => builder.AddTask(wf => wf.GetCustomer, wf => new CustomerGetRequest { CustomerId = 1 }),
        DefaultCase = builder => builder.AddTask(wf => wf.Terminate, wf => new TerminateTaskInput { TerminationStatus = TerminationStatus.Failed })
    }
);
#pragma warning restore CS0618
```

### Dynamic Task

Dynamic tasks allow selecting which task to execute at runtime. The task name is determined by a workflow input or computed value. You define the expected input/output types that the dynamically selected task should conform to:

```csharp
// Define the expected input/output for the dynamic task
public class ExpectedDynamicInput : IRequest<ExpectedDynamicOutput>
{
    public int CustomerId { get; set; }
}

public class ExpectedDynamicOutput
{
    public string Name { get; set; }
    public string Address { get; set; }
}

// In the workflow:
public class MyWorkflowInput : WorkflowInput<MyWorkflowOutput>
{
    public string TaskName { get; set; }  // e.g., "CUSTOMER_get_v1" or "CUSTOMER_get_v2"
    public int CustomerId { get; set; }
}

public DynamicTaskModel<ExpectedDynamicInput, ExpectedDynamicOutput> DynamicHandler { get; set; }

_builder.AddTask(
    wf => wf.DynamicHandler,
    wf => new DynamicTaskInput<ExpectedDynamicInput, ExpectedDynamicOutput>
    {
        TaskInput = new ExpectedDynamicInput { CustomerId = wf.WorkflowInput.CustomerId },
        TaskToExecute = wf.WorkflowInput.TaskName  // Task name resolved at runtime
    }
);

// Access the output after the dynamic task executes
_builder.AddTask(
    wf => wf.PrepareEmail,
    wf => new PrepareEmailRequest { Name = wf.DynamicHandler.Output.Name, Address = wf.DynamicHandler.Output.Address }
);
```

### Dynamic Fork-Join Task

```csharp
public DynamicForkJoinTaskModel DynamicFork { get; set; }

_builder.AddTask(
    wf => wf.DynamicFork,
    wf => new DynamicForkJoinInput
    {
        DynamicTasks = /* list of task names */,
        DynamicTasksInput = /* corresponding inputs */
    }
);
```

### Do-While Loop Task

The Do-While task executes a set of tasks repeatedly while a condition is true. The loop condition uses JSONPath expressions where:
- `$.do_while.iteration` - the current iteration number (0-based)
- `$.value` - the value passed in the `DoWhileInput.Value` property

```csharp
public class MyWorkflowInput : WorkflowInput<MyWorkflowOutput>
{
    public int Loops { get; set; }  // Number of iterations
}

public DoWhileTaskModel DoWhile { get; set; }
public CustomerGetHandler GetCustomer { get; set; }

_builder.AddTask(
    wf => wf.DoWhile,
    wf => new DoWhileInput { Value = wf.WorkflowInput.Loops },  // Value used in condition
    "$.do_while.iteration < $.value",  // Loop while iteration < Loops
    builder =>
    {
        // Tasks to execute in each iteration
        builder.AddTask(wf => wf.GetCustomer, wf => new CustomerGetRequest { CustomerId = "CUSTOMER-1" });
    }
);
```

The loop continues as long as the condition evaluates to true. In this example, if `Loops = 3`, the inner tasks execute 3 times (iterations 0, 1, 2).

Note: ConductorSharp does not provide a strongly typed output for the `DoWhile` task, as can be seen from the implementation:

```csharp
public class DoWhileTaskModel : TaskModel<DoWhileInput, NoOutput>
{
}
```

### Lambda Task (JavaScript)

The Lambda task executes inline JavaScript code. Define input/output models:

```csharp
public class LambdaInput : IRequest<LambdaOutput>
{
    public string Value { get; set; }
}

public class LambdaOutput
{
    public string Something { get; set; }
}

public LambdaTaskModel<LambdaInput, LambdaOutput> LambdaTask { get; set; }


_builder.AddTask(
    wf => wf.LambdaTask,
    wf => new LambdaInput { Value = wf.WorkflowInput.Input },
    script: "return { something: $.Value.toUpperCase() }"  // JavaScript expression
);
```

For context, in the above parameterized generic class `LambdaTaskModel`, the `LambdaOutput` instance is available as `Output.Result.Something`. This is less than ideal, but is the current way of things. Reasoning can be seen in the implementation:

```csharp
public abstract class LambdaOutputModel<O>
{
  public O Result { get; set; }
}

public abstract class LambdaTaskModel<I, O> where I : IRequest<O>
{
  public I Input { get; set; }

  public LambdaOutputModel<O> Output { get; set; }
}```

### C# Lambda Task (Patterns Package)

The C# Lambda task executes inline C# code. Requires the Patterns package.

```csharp
// Requires: .AddCSharpLambdaTasks()
public class LambdaInput : IRequest<LambdaOutput>
{
    public string Value { get; set; }
}

public class LambdaOutput
{
    public string Result { get; set; }
}

public CSharpLambdaTaskModel<LambdaInput, LambdaOutput> InlineLambda { get; set; }

_builder.AddTask(
    wf => wf.InlineLambda,
    wf => new LambdaInput { Value = wf.WorkflowInput.Input },
    input => new LambdaOutput { Result = input.Value.ToUpperInvariant() }  // C# lambda expression
);
```

### Wait Task

The Wait task pauses workflow execution for a duration or until a specific time:

```csharp
public WaitTaskModel WaitTask { get; set; }

// Wait for a duration (supports: s, m, h, d for seconds, minutes, hours, days)
_builder.AddTask(
    wf => wf.WaitTask,
    wf => new WaitTaskInput { Duration = "1s" }
);

// Or wait until a specific datetime
_builder.AddTask(
    wf => wf.WaitTask,
    wf => new WaitTaskInput { Until = "2024-12-31 11:59" }
);
```

### WaitSeconds Task (Patterns Package)

A convenience task for waiting a specific number of seconds:

```csharp
// Requires: .AddConductorSharpPatterns()
public WaitSeconds WaitTask { get; set; }

_builder.AddTask(wf => wf.WaitTask, wf => new WaitSecondsRequest { Seconds = 30 });
```

### Terminate Task

The Terminate task ends the workflow execution with a specific status and output:

```csharp
public TerminateTaskModel TerminateTask { get; set; }

_builder.AddTask(
    wf => wf.TerminateTask,
    wf => new TerminateTaskInput
    {
        TerminationStatus = TerminationStatus.Completed,  // or TerminationStatus.Failed
        WorkflowOutput = new { Property = "Test", Result = "Done" }
    }
);
```


### Human Task

The Human task pauses the workflow until a human completes an action (e.g., approval):

```csharp
public class HumanTaskOutput
{
    public string CustomerId { get; set; }
    public bool Approved { get; set; }
}

public HumanTaskModel<HumanTaskOutput> HumanTask { get; set; }
public CustomerGetHandler GetCustomer { get; set; }

// Add the human task
_builder.AddTask(wf => wf.HumanTask, wf => new HumanTaskInput<HumanTaskOutput> { });

// Use the human task output in subsequent tasks
_builder.AddTask(wf => wf.GetCustomer, wf => new CustomerGetRequest { CustomerId = wf.HumanTask.Output.CustomerId });
```

### JSON JQ Transform Task

The JSON JQ Transform task applies JQ expressions to transform data:

```csharp
public class JqInput : IRequest<JqOutput>
{
    public string QueryExpression { get; set; }
    public object Data { get; set; }
}

public class JqOutput
{
    public object Result { get; set; }
}

public JsonJqTransformTaskModel<JqInput, JqOutput> TransformTask { get; set; }

_builder.AddTask(
    wf => wf.TransformTask,
    wf => new JqInput 
    { 
        QueryExpression = ".data | map(.name)", 
        Data = wf.WorkflowInput.Items 
    }
);
```

### ReadWorkflowTasks Task (Patterns Package)

Reads task data from another workflow execution:

```csharp
// Requires: .AddConductorSharpPatterns()
public ReadWorkflowTasks ReadTasks { get; set; }

_builder.AddTask(
    wf => wf.ReadTasks,
    wf => new ReadWorkflowTasksInput 
    { 
        WorkflowId = wf.WorkflowInput.TargetWorkflowId,
        TaskNames = "task1,task2"  // Comma-separated task reference names
    }
);
```

### Optional Tasks

Mark a task as optional so workflow continues even if the task fails:

```csharp
_builder.AddTask(wf => wf.OptionalTask, wf => new OptionalTaskRequest { Value = "test" }).AsOptional();
```

### PassThrough Task (Raw Definition)

For unsupported task types:

```csharp
_builder.AddTasks(new WorkflowTask
{
    Name = "CUSTOM_task",
    TaskReferenceName = "custom_ref",
    Type = "CUSTOM",
    InputParameters = new Dictionary<string, object> { ["key"] = "value" }
});
```

## Configuration

### Execution Manager

```csharp
services
    .AddConductorSharp(baseUrl: "http://localhost:8080")
    .AddExecutionManager(
        maxConcurrentWorkers: 10,    // Max concurrent task executions
        sleepInterval: 500,          // Base polling interval (ms)
        longPollInterval: 100,       // Long poll timeout (ms)
        domain: "my-domain",         // Optional worker domain
        typeof(Program).Assembly     // Assemblies containing handlers
    );
```

### Multiple Conductor Instances

```csharp
services
    .AddConductorSharp(baseUrl: "http://primary-conductor:8080")
    .AddAlternateClient(
        baseUrl: "http://secondary-conductor:8080",
        key: "Secondary",
        apiPath: "api",
        ignoreInvalidCertificate: false
    );

// Usage with keyed services
public class MyController(
    IWorkflowService primaryService,
    [FromKeyedServices("Secondary")] IWorkflowService secondaryService
) { }
```

### Poll Timing Strategies

```csharp
// Default: Inverse exponential backoff
.AddExecutionManager(...)

// Constant interval polling
.AddExecutionManager(...)
.UseConstantPollTimingStrategy()
```

### Beta Execution Manager

```csharp
.AddExecutionManager(...)
.UseBetaExecutionManager()  // Uses TypePollSpreadingExecutionManager
```

### Patterns Package

```csharp
.AddExecutionManager(...)
.AddConductorSharpPatterns()      // Adds WaitSeconds, ReadWorkflowTasks
.AddCSharpLambdaTasks()           // Adds C# lambda task support
```

### Kafka Cancellation Notifier

```csharp
.AddExecutionManager(...)
.AddKafkaCancellationNotifier(
    kafkaBootstrapServers: "localhost:9092",
    topicName: "conductor.status.task",
    groupId: "my-worker-group",
    createTopicOnStartup: true
)
```

### Pipeline Behaviors

```csharp
.AddPipelines(pipelines =>
{
    // Custom behavior (runs first)
    pipelines.AddCustomBehavior(typeof(MyCustomBehavior<,>));
    
    // Built-in behaviors
    pipelines.AddExecutionTaskTracking();  // Track task execution metrics
    pipelines.AddContextLogging();         // Add context to log scopes
    pipelines.AddRequestResponseLogging(); // Log requests/responses
    pipelines.AddValidation();             // Validate using DataAnnotations
})
```

### Custom Behavior

```csharp
public class TimingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var response = await next();
        Console.WriteLine($"Execution took {sw.ElapsedMilliseconds}ms");
        return response;
    }
}
```

### Health Checks

```csharp
// In Program.cs
builder.Services.AddHealthChecks()
    .AddCheck<ConductorSharpHealthCheck>("conductor-worker");

// Configure health service
.AddExecutionManager(...)
.SetHealthCheckService<FileHealthService>()  // or InMemoryHealthService
```

## Client Services

Available services for direct Conductor API access:

- `IWorkflowService` - Start, pause, resume, terminate workflows
- `ITaskService` - Update tasks, get logs, poll for tasks
- `IMetadataService` - Manage workflow/task definitions
- `IAdminService` - Admin operations, queue management
- `IEventService` - Event handlers
- `IQueueAdminService` - Queue administration
- `IWorkflowBulkService` - Bulk workflow operations
- `IHealthService` - Conductor server health
- `IExternalPayloadService` - External payload storage

### Example Usage

```csharp
public class WorkflowController : ControllerBase
{
    private readonly IWorkflowService _workflowService;
    private readonly IMetadataService _metadataService;

    public WorkflowController(IWorkflowService workflowService, IMetadataService metadataService)
    {
        _workflowService = workflowService;
        _metadataService = metadataService;
    }

    [HttpPost("start")]
    public async Task<string> StartWorkflow([FromBody] StartRequest request)
    {
        return await _workflowService.StartAsync(new StartWorkflowRequest
        {
            Name = "MY_workflow",
            Version = 1,
            Input = new Dictionary<string, object> { ["customerId"] = request.CustomerId }
        });
    }

    [HttpGet("definitions")]
    public async Task<ICollection<WorkflowDef>> GetDefinitions()
    {
        return await _metadataService.ListWorkflowsAsync();
    }
}
```

## Scaffolding with Toolkit

### Installation

```bash
dotnet tool install --global ConductorSharp.Toolkit --version 3.0.1-beta3
```

### Configuration

Create `conductorsharp.yaml`:

```yaml
baseUrl: http://localhost:8080
apiPath: api
namespace: MyApp.Generated
destination: ./Generated
```

### Usage

```bash
# Scaffold all tasks and workflows
dotnet-conductorsharp

# Use custom config file
dotnet-conductorsharp -f myconfig.yaml

# Filter by name
dotnet-conductorsharp -n CUSTOMER_get -n ORDER_create

# Filter by owner email
dotnet-conductorsharp -e team@example.com

# Filter by owner app
dotnet-conductorsharp -a my-application

# Skip tasks or workflows
dotnet-conductorsharp --no-tasks
dotnet-conductorsharp --no-workflows

# Preview without generating files
dotnet-conductorsharp --dry-run
```

## Execution Context

Access workflow/task metadata in handlers:

```csharp
public class MyHandler : TaskRequestHandler<MyRequest, MyResponse>
{
    private readonly ConductorSharpExecutionContext _context;

    public MyHandler(ConductorSharpExecutionContext context)
    {
        _context = context;
    }

    public override async Task<MyResponse> Handle(MyRequest request, CancellationToken cancellationToken)
    {
        var workflowId = _context.WorkflowId;
        var taskId = _context.TaskId;
        var correlationId = _context.CorrelationId;
        // ...
    }
}
```

## Task Domain Assignment

```csharp
[TaskDomain("my-domain")]
public class MyTaskHandler : TaskRequestHandler<...> { }
```

## Common Patterns

### Workflow with Multiple Tasks

```csharp
public GetCustomerHandler GetCustomer { get; set; }
public PrepareEmailHandler PrepareEmail { get; set; }

public override void BuildDefinition()
{
    _builder.AddTask(wf => wf.GetCustomer, wf => new GetCustomerRequest { CustomerId = wf.WorkflowInput.CustomerId });
    _builder.AddTask(wf => wf.PrepareEmail, wf => new PrepareEmailRequest 
    { 
        Name = wf.GetCustomer.Output.Name,
        Address = wf.GetCustomer.Output.Address 
    });
    _builder.SetOutput(wf => new MyWorkflowOutput { EmailBody = wf.PrepareEmail.Output.EmailBody });
}
```

### Conditional Workflow

```csharp
public SwitchTaskModel SwitchTask { get; set; }
public ProcessTaskHandler ProcessTask { get; set; }
public DefaultTaskHandler DefaultTask { get; set; }

_builder.AddTask(
    wf => wf.SwitchTask,
    wf => new SwitchTaskInput { SwitchCaseValue = wf.WorkflowInput.Operation },
    new DecisionCases<MyWorkflow>
    {
        ["process"] = builder => builder.AddTask(wf => wf.ProcessTask, wf => new ProcessTaskRequest { Value = "data" }),
        ["skip"] = builder => { /* skip processing - no tasks */ },
        DefaultCase = builder => builder.AddTask(wf => wf.DefaultTask, wf => new DefaultTaskRequest { })
    }
);
```

### Error Handling with Failure Workflow

```csharp
[WorkflowMetadata(FailureWorkflow = typeof(HandleFailureWorkflow))]
public class MyWorkflow : Workflow<...> { }
```

## Best Practices

1. **Use `[OriginalName]` attribute** for custom task/workflow names in Conductor
2. **Register workflows** with `services.RegisterWorkflow<MyWorkflow>()`
3. **Use strongly-typed models** for inputs/outputs instead of dictionaries
4. **Add validation** using DataAnnotations and `.AddValidation()` pipeline
5. **Use patterns package** for common tasks (WaitSeconds, ReadWorkflowTasks, C# Lambda)
6. **Configure health checks** for production deployments
7. **Use scaffolding tool** to generate models from existing Conductor definitions
