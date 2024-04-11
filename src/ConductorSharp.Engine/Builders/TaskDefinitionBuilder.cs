﻿using System;
using System.Collections.Generic;
using System.Linq;
using ConductorSharp.Client.Generated;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util;
using ConductorSharp.Engine.Util.Builders;

namespace ConductorSharp.Engine.Builders
{
    public class TaskDefinitionBuilder(BuildConfiguration buildConfiguration, ITaskNameBuilder taskNameBuilder)
    {
        public BuildConfiguration BuildConfiguration { get; set; } = buildConfiguration;
        private readonly ITaskNameBuilder _taskNameBuilder = taskNameBuilder;

        public TaskDef Build<T>(Action<TaskDefinitionOptions> updateOptions = null) => Build(typeof(T), updateOptions);

        public TaskDef Build(Type taskType, Action<TaskDefinitionOptions> updateOptions = null)
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

            return new TaskDef
            {
                OwnerApp = BuildConfiguration.DefaultOwnerApp ?? options.OwnerApp,
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
                InputTemplate = (options.InputTemplate ?? []).ToObject<IDictionary<string, object>>(),
                ExecutionNameSpace = options.ExecutionNameSpace,
                BackoffScaleFactor = 1
            };
        }
    }
}
