using ConductorSharp.Engine.Builders.Metadata;

namespace ConductorSharp.Engine.Tests.Samples.Workers;

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
    public override Task<PrepareEmailResponse> Handle(PrepareEmailRequest request, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
