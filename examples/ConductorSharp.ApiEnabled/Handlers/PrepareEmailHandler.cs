using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;
using MediatR;
using System.Text;

namespace ConductorSharp.ApiEnabled.Handlers;

public class PrepareEmailRequest : IRequest<PrepareEmailResponse>
{
    public string? CustomerName { get; set; }
    public string? Address { get; set; }
}

public class PrepareEmailResponse
{
    public string? EmailBody { get; set; }
}

[OriginalName("EMAIL_prepare")]
public class PrepareEmailHandler : ITaskRequestHandler<PrepareEmailRequest, PrepareEmailResponse>
{
    public Task<PrepareEmailResponse> Handle(PrepareEmailRequest request, CancellationToken cancellationToken)
    {
        var emailBodyBuilder = new StringBuilder();

        emailBodyBuilder.AppendLine("New order executed");
        emailBodyBuilder.AppendLine("------------------");
        emailBodyBuilder.AppendLine($"ProvisionDateTime: {DateTimeOffset.Now.ToString("r")}");
        emailBodyBuilder.AppendLine($"Customer: {request.CustomerName}");
        emailBodyBuilder.AppendLine($"Address: {request.Address}");

        return Task.FromResult(new PrepareEmailResponse { EmailBody = emailBodyBuilder.ToString() });
    }
}
