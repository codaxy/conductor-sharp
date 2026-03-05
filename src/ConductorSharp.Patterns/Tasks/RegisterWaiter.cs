using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Client.Generated;
using ConductorSharp.Engine;
using ConductorSharp.Engine.Builders.Metadata;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util;
using ConductorSharp.Patterns.Services;
using ConductorSharp.Patterns.Workflows;
using MediatR;

namespace ConductorSharp.Patterns.Tasks;

public class RegisterWaiterRequest : IRequest<RegisterWaiterResponse>
{
    [Required]
    public string SignalKey { get; set; } = default!;
}

public class RegisterWaiterResponse
{
    public bool AlreadySignaled { get; set; }
    public TaskResultStatus? SignalStatus { get; set; }
    public Dictionary<string, object>? SignalOutputData { get; set; }
}

[OriginalName(Constants.RegisterWaiterTaskName)]
public class RegisterWaiter(ISignalStore signalStore, ConductorSharpExecutionContext context)
    : TaskRequestHandler<RegisterWaiterRequest, RegisterWaiterResponse>
{
    public override async Task<RegisterWaiterResponse> Handle(RegisterWaiterRequest request, CancellationToken cancellationToken)
    {
        var entry = await signalStore.RegisterWaiterAsync(request.SignalKey, context.WorkflowId, SignalWait.WaitTaskRefName, cancellationToken);

        if (entry.SignalStatus is not null)
        {
            await signalStore.DeleteAsync(request.SignalKey, cancellationToken);

            return new RegisterWaiterResponse
            {
                AlreadySignaled = true,
                SignalStatus = entry.SignalStatus,
                SignalOutputData = entry.SignalOutputData
            };
        }

        return new RegisterWaiterResponse { AlreadySignaled = false };
    }
}
