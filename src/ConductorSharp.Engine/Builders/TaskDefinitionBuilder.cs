using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util;
using ConductorSharp.Engine.Util.Builders;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ConductorSharp.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ConductorSharp.Engine.Builders
{
    public class TaskDefinitionBuilder
    {
        private readonly ITaskNameBuilder _taskNameBuilder;

        public TaskDefinitionBuilder(ITaskNameBuilder taskNameBuilder)
        {
            _taskNameBuilder = taskNameBuilder;
        }

        public TaskDefinition Build<T>(Action<TaskDefinitionOptions> updateOptions = null) => Build(typeof(T), updateOptions);

        public TaskDefinition Build(Type taskType, Action<TaskDefinitionOptions> updateOptions = null)
        {
            var options = new TaskDefinitionOptions();

            updateOptions?.Invoke(options);

            var interfaces = taskType
                .GetInterfaces()
                .Where(a => a.IsGenericType && a.GetGenericTypeDefinition() == typeof(ITaskRequestHandler<,>))
                .First();

            var genericArguments = interfaces.GetGenericArguments();

            var inputType = genericArguments[0];
            var outputType = genericArguments[1];

            var originalName = _taskNameBuilder.Build(taskType);

            return new TaskDefinition
            {
                OwnerApp = options.OwnerApp,
                Name = originalName,
                Description = options.Description,
                RetryCount = options.RetryCount,
                TimeoutSeconds = options.TimeoutSeconds,
                InputKeys = inputType.GetProperties().Select(NamingUtil.GetParameterName).ToList(),
                OutputKeys = outputType.GetProperties().Select(NamingUtil.GetParameterName).ToList(),
                TimeoutPolicy = options.TimeoutPolicy,
                RetryLogic = options.RetryLogic,
                RetryDelaySeconds = options.RetryDelaySeconds,
                ResponseTimeoutSeconds = options.ResponseTimeoutSeconds,
                ConcurrentExecLimit = options.ConcurrentExecLimit,
                RateLimitPerFrequency = options.RateLimitPerFrequency,
                RateLimitFrequencyInSeconds = options.RateLimitFrequencyInSeconds,
                OwnerEmail = options.OwnerEmail,
                PollTimeoutSeconds = options.PollTimeoutSeconds,
                CreatedBy = options.CreatedBy,
                UpdatedBy = options.UpdatedBy,
                InputTemplate = options.InputTemplate,
                ExecutionNameSpace = options.ExecutionNameSpace,
            };
        }
    }
}
