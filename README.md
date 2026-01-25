# ConductorSharp

A comprehensive .NET client library for [Conductor](https://github.com/conductor-oss/conductor) workflow orchestration engine. Features a strongly-typed workflow builder DSL, task handlers, and quality-of-life additions for building robust workflow applications.

[![NuGet](https://img.shields.io/nuget/v/ConductorSharp.Client.svg)](https://www.nuget.org/packages/ConductorSharp.Client)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)


**Note: This documentation has been AI generated and human reviewed.**

## Table of Contents

- [Installation](#installation)
- [Quick Start](#quick-start)
- [Core Concepts](#core-concepts)
  - [Workflow Definition](#workflow-definition)
  - [Task Handlers](#task-handlers)
  - [Input/Output Models](#inputoutput-models)
  - [Task Input Specification](#task-input-specification)
- [Task Types](#task-types)
- [Configuration](#configuration)
- [Pipeline Behaviors](#pipeline-behaviors)
- [Health Checks](#health-checks)
- [Patterns Package](#patterns-package)
- [Kafka Cancellation Notifier](#kafka-cancellation-notifier)
- [Toolkit CLI](#toolkit-cli)
- [API Services](#api-services)
- [Running the Examples](#running-the-examples)

## Installation

### Core Packages

```bash
# API client for Conductor
dotnet add package ConductorSharp.Client --version 3.5.0

# Workflow engine with builder DSL, task handlers, and worker scheduling
dotnet add package ConductorSharp.Engine --version 3.5.0
```

### Additional Packages

```bash
# Built-in tasks (WaitSeconds, ReadWorkflowTasks, C# Lambda Tasks)
dotnet add package ConductorSharp.Patterns --version 3.5.0

# Kafka-based task cancellation notifications
dotnet add package ConductorSharp.KafkaCancellationNotifier --version 3.5.0

# CLI tool for scaffolding task/workflow definitions
dotnet tool install --global ConductorSharp.Toolkit --version 3.0.1-beta3
```

## Quick Start

### 1. Configure Services

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
    )
    .AddPipelines(pipelines =>
    {
        pipelines.AddRequestResponseLogging();
        pipelines.AddValidation();
    });

builder.Services.RegisterWorkflow<MyWorkflow>();

var host = builder.Build();
await host.RunAsync();
```

### 2. Define a Task Handler

```csharp
using ConductorSharp.Engine.Builders.Metadata;
using ConductorSharp.Engine;

public class PrepareEmailRequest : IRequest<PrepareEmailResponse>
{
    public string CustomerName { get; set; }
    public string Address { get; set; }
}

public class PrepareEmailResponse
{
    public string EmailBody { get; set; }
}

[OriginalName("EMAIL_prepare")]
public class PrepareEmailHandler : TaskRequestHandler<PrepareEmailRequest, PrepareEmailResponse>
{
    public override async Task<PrepareEmailResponse> Handle(PrepareEmailRequest request, CancellationToken cancellationToken)
    {
        var body = $"Hello {request.CustomerName} at {request.Address}!";
        return new PrepareEmailResponse { EmailBody = body };
    }
}
```

### 3. Define a Workflow

```csharp
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Builders.Metadata;

public class SendNotificationInput : WorkflowInput<SendNotificationOutput>
{
    public int CustomerId { get; set; }
}

public class SendNotificationOutput : WorkflowOutput
{
    public string EmailBody { get; set; }
}

[OriginalName("NOTIFICATION_send")]
[WorkflowMetadata(OwnerEmail = "team@example.com")]
public class SendNotificationWorkflow : Workflow<SendNotificationWorkflow, SendNotificationInput, SendNotificationOutput>
{
    public SendNotificationWorkflow(
        WorkflowDefinitionBuilder<SendNotificationWorkflow, SendNotificationInput, SendNotificationOutput> builder
    ) : base(builder) { }

    public GetCustomerHandler GetCustomer { get; set; }
    public PrepareEmailHandler PrepareEmail { get; set; }

    public override void BuildDefinition()
    {
        _builder.AddTask(
            wf => wf.GetCustomer,
            wf => new() { CustomerId = wf.WorkflowInput.CustomerId }
        );

        _builder.AddTask(
            wf => wf.PrepareEmail,
            wf => new() 
            { 
                CustomerName = wf.GetCustomer.Output.Name,
                Address = wf.GetCustomer.Output.Address 
            }
        );

        _builder.SetOutput(wf => new()
        {
            EmailBody = wf.PrepareEmail.Output.EmailBody
        });
    }
}
```

## Core Concepts

### Workflow Definition

Workflows are defined by inheriting from `Workflow<TWorkflow, TInput, TOutput>`:

```csharp
public class MyWorkflow : Workflow<MyWorkflow, MyWorkflowInput, MyWorkflowOutput>
{
    public MyWorkflow(WorkflowDefinitionBuilder<MyWorkflow, MyWorkflowInput, MyWorkflowOutput> builder) 
        : base(builder) { }

    // Task properties - these become task references in the workflow
    public SomeTaskHandler FirstTask { get; set; }
    public AnotherTaskHandler SecondTask { get; set; }

    public override void BuildDefinition()
    {
        // Add tasks with strongly-typed input expressions
        _builder.AddTask(wf => wf.FirstTask, wf => new() { Input = wf.WorkflowInput.SomeValue });
        _builder.AddTask(wf => wf.SecondTask, wf => new() { Input = wf.FirstTask.Output.Result });
        
        // Set workflow output
        _builder.SetOutput(wf => new() { Result = wf.SecondTask.Output.Value });
    }
}
```

### Task Handlers

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

### Input/Output Models

```csharp
// Workflow I/O
public class MyWorkflowInput : WorkflowInput<MyWorkflowOutput>
{
    public string CustomerId { get; set; }
}

public class MyWorkflowOutput : WorkflowOutput
{
    public string Result { get; set; }
}

// Task I/O
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

### Task Input Specification

In Conductor, task inputs in workflows are specified using Conductor expressions with the format: `${SOURCE.input/output.JSONPath}`. The `SOURCE` can be `workflow` or a task reference name in the workflow definition. `input/output` refers to the input of the workflow or output of the task. JSONPath is used to traverse the input/output object.

ConductorSharp generates these expressions automatically when writing workflows. Here's an example:

```csharp
_builder.AddTask(
    wf => wf.PrepareEmail,
    wf => new PrepareEmailRequest
    {
        CustomerName = $"{wf.GetCustomer.Output.FirstName} {wf.GetCustomer.Output.LastName}",
        Address = wf.WorkflowInput.Address
    }
);
```

This is converted to the following Conductor input parameters specification:

```json
"inputParameters": {
    "customer_name": "${get_customer.output.first_name} ${get_customer.output.last_name}",
    "address": "${workflow.input.address}"
}
```

#### Casting

When input/output parameters are of different types, casting can be used:

```csharp
wf => new PrepareEmailRequest
{
    CustomerName = ((FullName)wf.GetCustomer.Output.Name).FirstName,
    Address = (string)wf.GetCustomer.Output.Address
}
```

This translates to:

```json
"inputParameters": {
    "customer_name": "${get_customer.output.name.first_name}",
    "address": "${get_customer.output.address}"
}
```

#### Array Initialization

Array initialization is supported. Arrays can be typed or dynamic:

```csharp
wf => new()
{
    Integers = new[] { 1, 2, 3 },
    TestModelList = new List<ArrayTaskInput.TestModel>
    {
        new ArrayTaskInput.TestModel { String = wf.Input.TestValue },
        new ArrayTaskInput.TestModel { String = "List2" }
    },
    Models = new[]
    {
        new ArrayTaskInput.TestModel { String = "Test1" },
        new ArrayTaskInput.TestModel { String = "Test2" }
    },
    Objects = new dynamic[] { new { AnonymousObjProp = "Prop" }, new { Test = "Prop" } }
}
```

This translates to:

```json
"inputParameters": {
    "integers": [1, 2, 3],
    "test_model_list": [
        {
            "string": "${workflow.input.test_value}"
        },
        {
            "string": "List2"
        }
    ],
    "models": [
        {
            "string": "Test1"
        },
        {
            "string": "Test2"
        }
    ],
    "objects": [
        {
            "anonymous_obj_prop": "Prop"
        },
        {
            "test": "Prop"
        }
    ]
}
```

#### Object Initialization

Object initialization is supported, including anonymous objects when initializing sub-properties:

```csharp
wf => new()
{
    NestedObjects = new TestModel
    {
        Integer = 1,
        String = "test",
        Object = new TestModel
        {
            Integer = 1,
            String = "string",
            Object = new { NestedInput = "1" }
        }
    }
}
```

This translates to:

```json
"inputParameters": {
    "nested_objects": {
        "integer": 1,
        "string": "test",
        "object": {
            "integer": 1,
            "string": "string",
            "object": {
                "nested_input": "1"
            }
        }
    }
}
```

#### Indexing

Dictionary indexing is supported. Indexing using an indexer on arbitrary types is currently not supported:

```csharp
wf => new()
{
    CustomerName = wf.WorkflowInput.Dictionary["test"].CustomerName,
    Address = wf.WorkflowInput.DoubleDictionary["test"]["address"]
}
```

This translates to:

```json
"inputParameters": {
    "customer_name": "${workflow.input.dictionary['test'].customer_name}",
    "address": "${workflow.input.double_dictionary['test']['address']}"
}
```

#### Workflow Name

You can embed the name of any workflow in task input specification using `NamingUtil.NameOf<T>()`:

```csharp
wf => new()
{
    Name = $"Workflow name: {NamingUtil.NameOf<StringInterpolation>()}",
    WfName = NamingUtil.NameOf<StringInterpolation>()
}
```

This translates to:

```json
"inputParameters": {
    "name": "Workflow name: TEST_StringInterpolation",
    "wf_name": "TEST_StringInterpolation"
}
```

Note: `StringInterpolation` has an attribute `[OriginalName("TEST_StringInterpolation")]` applied.

#### String Concatenation

String concatenation is supported. You can concatenate strings with numbers, input/output parameters, and interpolation strings:

```csharp
wf => new()
{
    Input = 1
        + "Str_"
        + "2Str_"
        + wf.WorkflowInput.Input
        + $"My input: {wf.WorkflowInput.Input}"
        + NamingUtil.NameOf<StringAddition>()
        + 1
}
```

This translates to:

```json
"inputParameters": {
    "input": "1Str_2Str_${workflow.input.input}My input: ${workflow.input.input}string_addition1"
}
```

Note: `StringAddition` has an attribute `[OriginalName("string_addition")]` applied.

### Metadata Attributes

| Attribute | Target | Description |
|-----------|--------|-------------|
| `[OriginalName("NAME")]` | Class | Custom task/workflow name in Conductor |
| `[WorkflowMetadata(...)]` | Class | Workflow metadata (OwnerEmail, OwnerApp, Description, FailureWorkflow) |
| `[Version(n)]` | Class | Version number for sub-workflow references |
| `[TaskDomain("domain")]` | Class | Assign task to specific domain |

## Task Types

### Simple Task

```csharp
_builder.AddTask(wf => wf.MySimpleTask, wf => new() { Input = wf.WorkflowInput.Value });
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

### Dynamic Task

```csharp
public DynamicTaskModel<ExpectedInput, ExpectedOutput> DynamicHandler { get; set; }

_builder.AddTask(
    wf => wf.DynamicHandler,
    wf => new()
    {
        TaskInput = new() { CustomerId = wf.WorkflowInput.CustomerId },
        TaskToExecute = wf.WorkflowInput.TaskName  // Task name resolved at runtime
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
        DynamicTasks = /* list of tasks */,
        DynamicTasksInput = /* corresponding inputs */
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

### Wait Task

```csharp
public WaitTaskModel WaitTask { get; set; }

_builder.AddTask(
    wf => wf.WaitTask,
    wf => new WaitTaskInput { Duration = "1h" }  // or Until = "2024-01-01T00:00:00Z"
);
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
    wf => new() { QueryExpression = ".data | map(.name)", Data = wf.WorkflowInput.Items }
);
```

### PassThrough Task (Raw Definition)

For tasks not covered by the builder:

```csharp
_builder.AddTasks(new WorkflowTask
{
    Name = "CUSTOM_task",
    TaskReferenceName = "custom_ref",
    Type = "CUSTOM",
    InputParameters = new Dictionary<string, object> { ["key"] = "value" }
});
```

### Optional Tasks

Mark tasks as optional (workflow continues on failure):

```csharp
_builder.AddTask(wf => wf.OptionalTask, wf => new() { }).AsOptional();
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

### Worker Task Registration

Register standalone tasks without workflow:

```csharp
services.RegisterWorkerTask<MyTaskHandler>(options =>
{
    options.OwnerEmail = "team@example.com";
    options.Description = "My task description";
});
```

## Pipeline Behaviors

Behaviors form a middleware pipeline for task execution (powered by MediatR):

```csharp
.AddPipelines(pipelines =>
{
    // Add custom behavior (runs first)
    pipelines.AddCustomBehavior(typeof(MyCustomBehavior<,>));
    
    // Built-in behaviors
    pipelines.AddExecutionTaskTracking();  // Track task execution metrics
    pipelines.AddContextLogging();         // Add context to log scopes
    pipelines.AddRequestResponseLogging(); // Log requests/responses
    pipelines.AddValidation();             // Validate using DataAnnotations
})
```

### Custom Behavior Example

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

## Health Checks

### ASP.NET Core Integration

```csharp
// In Program.cs
builder.Services.AddHealthChecks()
    .AddCheck<ConductorSharpHealthCheck>("conductor-worker");

// Configure health service
.AddExecutionManager(...)
.SetHealthCheckService<FileHealthService>()  // or InMemoryHealthService
```

### Available Health Services

| Service | Description |
|---------|-------------|
| `InMemoryHealthService` | In-memory health state (default) |
| `FileHealthService` | Persists health to `CONDUCTORSHARP_HEALTH.json` file |

### Execution Context

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

## Patterns Package

Additional built-in tasks and utilities:

```csharp
.AddExecutionManager(...)
.AddConductorSharpPatterns()      // Adds WaitSeconds, ReadWorkflowTasks
.AddCSharpLambdaTasks()           // Adds C# lambda task support
```

### WaitSeconds Task

```csharp
public WaitSeconds WaitTask { get; set; }

_builder.AddTask(wf => wf.WaitTask, wf => new() { Seconds = 30 });
```

### ReadWorkflowTasks Task

Read task data from another workflow:

```csharp
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

### C# Lambda Tasks

Execute C# code inline in workflows:

```csharp
public CSharpLambdaTaskModel<LambdaInput, LambdaOutput> InlineLambda { get; set; }

_builder.AddTask(
    wf => wf.InlineLambda,
    wf => new() { Value = wf.WorkflowInput.Input },
    input => new LambdaOutput { Result = input.Value.ToUpperInvariant() }
);
```

## Kafka Cancellation Notifier

Handle task cancellation via Kafka events:

```csharp
.AddExecutionManager(...)
.AddKafkaCancellationNotifier(
    kafkaBootstrapServers: "localhost:9092",
    topicName: "conductor.status.task",
    groupId: "my-worker-group",
    createTopicOnStartup: true
)
```

**appsettings.json:**
```json
{
  "Conductor": {
    "BaseUrl": "http://localhost:8080",
    "MaxConcurrentWorkers": 10,
    "SleepInterval": 500,
    "LongPollInterval": 100,
    "KafkaCancellationNotifier": {
      "BootstrapServers": "localhost:9092",
      "GroupId": "my-worker",
      "TopicName": "conductor.status.task"
    }
  }
}
```

## Toolkit CLI

Generate C# models from existing Conductor task/workflow definitions.

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

### Command Options

| Option | Description |
|--------|-------------|
| `-f, --file` | Configuration file path (default: `conductorsharp.yaml`) |
| `-n, --name` | Filter by task/workflow name (can specify multiple) |
| `-a, --app` | Filter by owner app |
| `-e, --email` | Filter by owner email |
| `--no-tasks` | Skip task scaffolding |
| `--no-workflows` | Skip workflow scaffolding |
| `--dry-run` | Preview what would be generated |

## API Services

Inject these services to interact with Conductor programmatically:

| Service | Description |
|---------|-------------|
| `IWorkflowService` | Start, pause, resume, terminate workflows |
| `ITaskService` | Update tasks, get logs, poll for tasks |
| `IMetadataService` | Manage workflow/task definitions |
| `IAdminService` | Admin operations, queue management |
| `IEventService` | Event handlers |
| `IQueueAdminService` | Queue administration |
| `IWorkflowBulkService` | Bulk workflow operations |
| `IHealthService` | Conductor server health |
| `IExternalPayloadService` | External payload storage |

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

## Running the Examples

### Prerequisites

1. Clone and run Conductor:
   ```bash
   git clone https://github.com/conductor-oss/conductor.git
   cd conductor
   docker-compose up -d
   ```

2. Conductor UI available at: http://localhost:5000 (may vary by version)

### Starting the Examples

The solution includes three example projects:

| Project | Description |
|---------|-------------|
| `ConductorSharp.Definitions` | Console app with workflow definitions |
| `ConductorSharp.ApiEnabled` | Web API with workflow execution endpoints |
| `ConductorSharp.NoApi` | Console app with Kafka cancellation support |

```bash
# Run with Docker Compose
docker-compose up

# Or run individual projects
cd examples/ConductorSharp.Definitions
dotnet run
```

## License

MIT License - see [LICENSE](LICENSE) for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
