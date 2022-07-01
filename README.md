# conductor-sharp

A Conductor client library with some quality of life additions and a builder for conductor workflows.
[More info on wiki](https://github.com/codaxy/conductor-sharp/wiki)

## Installing ConductorSharp

If you require Conductor API client use:
dotnet add package ConductorSharp.Client --version 1.0.3

If you also require workflow and task registration, worker scheduling and execution, workflow builder, use:
dotnet add package ConductorSharp.Engine --version 1.0.3

## Running the example

### Conductor setup

Clone Conductor repo and run docker-compose file according to https://github.com/Netflix/conductor.

The conductor UI can be accessed at http://localhost:5000

### Starting the example solution

Selecting and running the Docker Compose as startup for the .NET project will register the task and workflow definitions.

The example projects provide a simple API to test running the workflow and fetching workflow definitions.
