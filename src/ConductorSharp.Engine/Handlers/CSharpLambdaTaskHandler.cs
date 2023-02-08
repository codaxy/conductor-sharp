using ConductorSharp.Engine.Exceptions;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Handlers
{
    internal class CSharpLambdaTaskInput : IRequest<object>
    {
        public const string LambdaIdenfitierParamName = "lambda_identifier";
        public const string TaskInputParamName = "task_input";

        [JsonProperty(LambdaIdenfitierParamName)]
        [Required]
        public string LambdaIdentifier { get; set; }

        [JsonProperty(TaskInputParamName)]
        [Required]
        public JObject TaskInput { get; set; }
    }

    [OriginalName(TaskName)]
    internal class CSharpLambdaTaskHandler : ITaskRequestHandler<CSharpLambdaTaskInput, object>
    {
        public const string TaskName = "CONDUCTOR_SHARP_inline_lambda_task";
        public const string LambdaTaskNameConfigurationProperty = nameof(LambdaTaskNameConfigurationProperty);

        private readonly IEnumerable<CSharpLambdaHandler> _handlers;

        public CSharpLambdaTaskHandler(IEnumerable<CSharpLambdaHandler> handlers) => _handlers = handlers;

        public Task<object> Handle(CSharpLambdaTaskInput request, CancellationToken cancellationToken)
        {
            var lambda = _handlers.FirstOrDefault(lambda => lambda.LambdaIdentifier == request.LambdaIdentifier);
            if (lambda == null)
                throw new NoLambdaException(request.LambdaIdentifier);

            return Task.FromResult(lambda.Handler.DynamicInvoke(request.TaskInput.ToObject(lambda.TaskInputType)));
        }
    }
}
