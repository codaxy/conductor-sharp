using ConductorSharp.Client;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Handlers
{
    internal class CSharpLambdaTaskInput : IRequest<object>
    {
        public const string LambdaIdenfitierPropName = "lambda_identifier";
        public const string TaskInputPropName = "task_input";

        [JsonProperty(LambdaIdenfitierPropName)]
        [Required]
        public string LambdaIdentifier { get; set; }

        [JsonProperty(TaskInputPropName)]
        [Required]
        public JObject TaskInput { get; set; }
    }

    [OriginalName(TaskName)]
    internal class CSharpLambdaTaskHandler : ITaskRequestHandler<CSharpLambdaTaskInput, object>
    {
        public const string TaskName = "_CONDUCTOR_SHARP_inline_lambda_task";
        private readonly Dictionary<string, CSharpLambda> _lambdas;

        public CSharpLambdaTaskHandler(IEnumerable<CSharpLambda> lambdas) => _lambdas = lambdas.ToDictionary(lambda => lambda.LambdaIdentifier);

        public Task<object> Handle(CSharpLambdaTaskInput request, CancellationToken cancellationToken)
        {
            var lambda = _lambdas.GetValueOrDefault(request.LambdaIdentifier);
            if (lambda == null)
                throw new NotImplementedException($"No corresponding lambda for identifier: {request.LambdaIdentifier}");

            return Task.FromResult(lambda.Handler.DynamicInvoke(request.TaskInput.ToObject(lambda.InputType, ConductorConstants.IoJsonSerializer)));
        }
    }
}
