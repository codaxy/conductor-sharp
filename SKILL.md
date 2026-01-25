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

### Interface-Based (Recommended)

```csharp
using ConductorSharp.Engine.Builders.Metadata;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;
using MediatR;

[OriginalName("MY_TASK_name")]
public class MyTaskHandler : TaskRequestHandler<MyTaskRequest, MyTaskResponse>
{
    private readonly ConductorSharpExecutionContext _context;
    
    public MyTaskHandler(ConductorSharpExecutionContext context)
    {
        _context = context; // Access workflow/task metadata
    }

    public async Task<MyTaskResponse> Handle(MyTaskRequest request, CancellationToken cancellationToken)
    {
        // Access context: _context.WorkflowId, _context.TaskId, _context.CorrelationId
        return new MyTaskResponse { /* ... */ };
    }
}
```

### Abstract Class-Based

```csharp
[OriginalName("MY_TASK_name")]
public class MyTaskHandler : TaskRequestHandler<MyTaskRequest, MyTaskResponse>
{
    public override async Task<MyTaskResponse> Handle(MyTaskRequest request, CancellationToken cancellationToken)
    {
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
    public SomeTaskV1 FirstTask { get; set; }
    public AnotherTaskV1 SecondTask { get; set; }

    public override void BuildDefinition()
    {
        _builder.AddTask(wf => wf.FirstTask, wf => new() { Input = wf.WorkflowInput.CustomerId });
        _builder.AddTask(wf => wf.SecondTask, wf => new() { Input = wf.FirstTask.Output.Result });
        
        _builder.SetOutput(wf => new() { Result = wf.SecondTask.Output.Value });
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
public MyTaskV1 MyTask { get; set; }

_builder.AddTask(wf => wf.MyTask, wf => new() { Input = wf.WorkflowInput.Value });
```

### Sub-Workflow Task

```csharp
public SubWorkflowTaskModel<ChildWorkflowInput, ChildWorkflowOutput> ChildWorkflow { get; set; }

_builder.AddTask(wf => wf.ChildWorkflow, wf => new() { CustomerId = wf.WorkflowInput.CustomerId });
```

### Switch Task (Conditional Branching)

```csharp
public SwitchTaskModel SwitchTask { get; set; }
public TaskA TaskInCaseA { get; set; }
public TaskB TaskInCaseB { get; set; }

_builder.AddTask(
    wf => wf.SwitchTask,
    wf => new SwitchTaskInput { SwitchCaseValue = wf.WorkflowInput.Operation },
    new DecisionCases<MyWorkflow>
    {
        ["caseA"] = builder => builder.AddTask(wf => wf.TaskInCaseA, wf => new() { }),
        ["caseB"] = builder => builder.AddTask(wf => wf.TaskInCaseB, wf => new() { }),
        DefaultCase = builder => { /* default case tasks */ }
    }
);
```

### Decision Task (Deprecated - Use Switch)

```csharp
#pragma warning disable CS0618
public DecisionTaskModel Decision { get; set; }

_builder.AddTask(
    wf => wf.Decision,
    wf => new() { CaseValueParam = "test" },
    new()
    {
        ["test"] = builder => builder.AddTask(wf => wf.GetCustomer, wf => new() { CustomerId = 1 }),
        DefaultCase = builder => builder.AddTask(wf => wf.Terminate, wf => new() { TerminationStatus = TerminationStatus.Failed })
    }
);
#pragma warning restore CS0618
```

### Dynamic Task

```csharp
public DynamicTaskModel<ExpectedInput, ExpectedOutput> DynamicHandler { get; set; }

_builder.AddTask(
    wf => wf.DynamicHandler,
    wf => new()
    {
        TaskInput = new() { CustomerId = wf.WorkflowInput.CustomerId },
        TaskToExecute = wf.WorkflowInput.TaskName  // Resolved at runtime
    }
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

```csharp
public DoWhileTaskModel DoWhile { get; set; }
public CustomerGetV1 GetCustomer { get; set; }

_builder.AddTask(
    wf => wf.DoWhile,
    wf => new() { Value = wf.WorkflowInput.Loops },
    "$.do_while.iteration < $.value",  // Loop condition
    builder =>
    {
        builder.AddTask(wf => wf.GetCustomer, wf => new() { CustomerId = "CUSTOMER-1" });
    }
);
```

### Lambda Task (JavaScript)

```csharp
public LambdaTaskModel<LambdaInput, LambdaOutput> LambdaTask { get; set; }

_builder.AddTask(
    wf => wf.LambdaTask,
    wf => new() { Value = wf.WorkflowInput.Input },
    script: "return { result: $.Value.toUpperCase() }"
);
```

### C# Lambda Task (Patterns Package)

```csharp
// Requires: .AddCSharpLambdaTasks()
public CSharpLambdaTaskModel<LambdaInput, LambdaOutput> InlineLambda { get; set; }

_builder.AddTask(
    wf => wf.InlineLambda,
    wf => new() { Value = wf.WorkflowInput.Input },
    input => new LambdaOutput { Result = input.Value.ToUpperInvariant() }
);
```

### Wait Task

```csharp
public WaitTaskModel WaitTask { get; set; }

_builder.AddTask(
    wf => wf.WaitTask,
    wf => new WaitTaskInput { Duration = "1h" }  // or Until = "2024-01-01T00:00:00Z"
);
```

### WaitSeconds Task (Patterns Package)

```csharp
// Requires: .AddConductorSharpPatterns()
public WaitSeconds WaitTask { get; set; }

_builder.AddTask(wf => wf.WaitTask, wf => new() { Seconds = 30 });
```

### Terminate Task

```csharp
public TerminateTaskModel TerminateTask { get; set; }

_builder.AddTask(
    wf => wf.TerminateTask,
    wf => new TerminateTaskInput
    {
        TerminationStatus = "COMPLETED",
        WorkflowOutput = new { Result = "Done" }
    }
);
```

### Event Task

```csharp
public EventTaskModel<EventInput> EventTask { get; set; }

_builder.AddTask(
    wf => wf.EventTask,
    wf => new() { EventData = wf.WorkflowInput.Data },
    sink: "kafka:my-topic"
);
```

### Human Task

```csharp
public HumanTaskModel<HumanTaskOutput> HumanTask { get; set; }

_builder.AddTask(
    wf => wf.HumanTask,
    wf => new HumanTaskInput<HumanTaskOutput> { /* ... */ }
);
```

### JSON JQ Transform Task

```csharp
public JsonJqTransformTaskModel<JqInput, JqOutput> TransformTask { get; set; }

_builder.AddTask(
    wf => wf.TransformTask,
    wf => new() 
    { 
        QueryExpression = ".data | map(.name)", 
        Data = wf.WorkflowInput.Items 
    }
);
```

### ReadWorkflowTasks Task (Patterns Package)

```csharp
// Requires: .AddConductorSharpPatterns()
public ReadWorkflowTasks ReadTasks { get; set; }

_builder.AddTask(
    wf => wf.ReadTasks,
    wf => new() 
    { 
        WorkflowId = wf.WorkflowInput.TargetWorkflowId,
        TaskNames = "task1,task2"  // Comma-separated reference names
    }
);
```

### Optional Tasks

```csharp
_builder.AddTask(wf => wf.OptionalTask, wf => new() { }).AsOptional();
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

    public async Task<MyResponse> Handle(MyRequest request, CancellationToken cancellationToken)
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
public override void BuildDefinition()
{
    _builder.AddTask(wf => wf.GetCustomer, wf => new() { CustomerId = wf.WorkflowInput.CustomerId });
    _builder.AddTask(wf => wf.PrepareEmail, wf => new() 
    { 
        CustomerName = wf.GetCustomer.Output.Name,
        Address = wf.GetCustomer.Output.Address 
    });
    _builder.SetOutput(wf => new() { EmailBody = wf.PrepareEmail.Output.EmailBody });
}
```

### Conditional Workflow

```csharp
_builder.AddTask(
    wf => wf.SwitchTask,
    wf => new SwitchTaskInput { SwitchCaseValue = wf.WorkflowInput.Operation },
    new DecisionCases<MyWorkflow>
    {
        ["process"] = builder => builder.AddTask(wf => wf.ProcessTask, wf => new() { }),
        ["skip"] = builder => { /* skip processing */ },
        DefaultCase = builder => builder.AddTask(wf => wf.DefaultTask, wf => new() { })
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
