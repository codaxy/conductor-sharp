using ConductorSharp.Engine.Builders.Metadata;

namespace ConductorSharp.Engine.Tests.Samples.Workers;

public class PrepareEmailRequest : ITaskInput<PrepareEmailResponse>
{
    public string CustomerName { get; set; }
    public string Address { get; set; }
}

public class PrepareEmailResponse
{
    public string EmailBody { get; set; }
}

[OriginalName("EMAIL_prepare")]
public class PrepareEmailHandler : Worker<PrepareEmailRequest, PrepareEmailResponse>
{
    public override Task<PrepareEmailResponse> Handle(
        PrepareEmailRequest request,
        WorkerExecutionContext context,
        CancellationToken cancellationToken
    ) => throw new NotImplementedException();
}
