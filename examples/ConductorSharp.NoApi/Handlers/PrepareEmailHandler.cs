using System.Text;
using ConductorSharp.Engine.Builders.Metadata;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;
using Microsoft.Extensions.Logging;

namespace ConductorSharp.NoApi.Handlers
{
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
    public class PrepareEmailHandler : INgWorker<PrepareEmailRequest, PrepareEmailResponse>
    {
        private readonly WorkerExecutionContext _context;
        private readonly ILogger<PrepareEmailHandler> _logger;

        public PrepareEmailHandler(WorkerExecutionContext context, ILogger<PrepareEmailHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PrepareEmailResponse> Handle(
            PrepareEmailRequest request,
            WorkerExecutionContext context,
            CancellationToken cancellationToken
        )
        {
            var emailBodyBuilder = new StringBuilder();

            emailBodyBuilder.AppendLine("New order executed");
            emailBodyBuilder.AppendLine("------------------");
            emailBodyBuilder.AppendLine($"ProvisionDateTime: {DateTimeOffset.Now.ToString("r")}");
            emailBodyBuilder.AppendLine($"Customer: {request.CustomerName}");
            emailBodyBuilder.AppendLine($"Address: {request.Address}");
            emailBodyBuilder.AppendLine("------------------");
            emailBodyBuilder.AppendLine($"WorkflowId : {_context.WorkflowId}");
            emailBodyBuilder.AppendLine($"WorkflowName: {_context.WorkflowName}");

            _logger.LogInformation("Prepared email");

            await Task.Delay(10000, cancellationToken);
            return new PrepareEmailResponse { EmailBody = emailBodyBuilder.ToString() };
        }
    }
}
