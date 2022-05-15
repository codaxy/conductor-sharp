# conductor-sharp

A Conductor client library with some quality of life additions and a builder for conductor workflows.

## Motivation behind the library

Workflows in conductor are JSON objects with properties specifying the workflow name, input parameters, etc. These properties are referenced across workflows and it can become hard to maintain the correctness of these definitions when updating workflow properties. Any issues with workflow definitions will be reported at workflow registration timey, but not while building the workflow definition itself.

This library implements a workflow builder that attempts to solve this problem by representing workflow tasks as C# classes and using a builder to bind their inputs and outputs.

Without builder:

```
{
  "createTime": 0,
  "updateTime": 1652641828884,
  "name": "NOTIFICATION_send_to_customer",
  "description": "{\"description\":null,\"labels\":null}",
  "version": 1,
  "tasks": [
    {
      "name": "CUSTOMER_get",
      "taskReferenceName": "get_customer",
      "description": "{\"description\":null}",
      "inputParameters": {
        "customer_id": "${workflow.input.customer_id}"
      },
      "type": "SIMPLE",
      "decisionCases": {},
      "defaultCase": [],
      "forkTasks": [],
      "startDelay": 0,
      "joinOn": [],
      "optional": false,
      "rateLimited": false,
      "defaultExclusiveJoinTask": [],
      "asyncComplete": false,
      "loopOver": []
    },
    {
      "name": "EMAIL_prepare",
      "taskReferenceName": "prepare_email",
      "description": "{\"description\":null}",
      "inputParameters": {
        "address": "${get_customer.output.address}",
        "name": "${get_customer.output.name}"
      },
      "type": "SIMPLE",
      "decisionCases": {},
      "defaultCase": [],
      "forkTasks": [],
      "startDelay": 0,
      "joinOn": [],
      "optional": false,
      "rateLimited": false,
      "defaultExclusiveJoinTask": [],
      "asyncComplete": false,
      "loopOver": []
    }
  ],
  "inputParameters": [
    "{\"customer_id\":{\"value\":\"\",\"description\":\" (optional)\"}}"
  ],
  "outputParameters": {},
  "schemaVersion": 2,
  "restartable": true,
  "workflowStatusListenerEnabled": true,
  "ownerEmail": "example@example.local",
  "timeoutPolicy": "ALERT_ONLY",
  "timeoutSeconds": 0,
  "variables": {},
  "inputTemplate": {}
}
```

With builder:

```
var builder = new WorkflowDefinitionBuilder<SendCustomerNotification>();

builder.AddTask(
    a => a.GetCustomer,
    b => new() { CustomerId = b.WorkflowInput.CustomerId }
);

builder.AddTask(
    a => a.PrepareEmail,
    b =>
        new()
        {
            Address = b.GetCustomer!.Output.Address,
            Name = b.GetCustomer!.Output.Name
        }
);

return builder.Build(
    options =>
    {
        options.Version = 1;
        options.OwnerEmail = "example@example.local";
    }
);
```

## Installing ConductorSharp

[ToDo]

## Running the example

### Conductor setup

Clone Conductor repo and run docker-compose file according to https://github.com/Netflix/conductor.

The conductor UI can be accessed at http://localhost:5000

### Starting the example solution

Selecting and running the Docker Compose as startup for the .NET project will register the task and workflow definitions.

The example projects provide a simple API to test running the workflow and fetching workflow definitions.
