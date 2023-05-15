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
        public BuildConfiguration BuildConfiguration { get; set; }
        private readonly ITaskNameBuilder _taskNameBuilder;

        public TaskDefinitionBuilder(BuildConfiguration buildConfiguration, ITaskNameBuilder taskNameBuilder)
        {
            BuildConfiguration = buildConfiguration;
            _taskNameBuilder = taskNameBuilder;
        }

        public TaskDefinition Build<T>(Action<TaskDefinitionOptions> updateOptions = null) => Build(typeof(T), updateOptions);

        public TaskDefinition Build(Type taskType, Action<TaskDefinitionOptions> updateOptions = null)
        {
            var options = new TaskDefinitionOptions();

            updateOptions?.Invoke(options);

            XmlDocumentationReader.LoadXmlDocumentation(taskType.Assembly);

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
                OwnerApp = BuildConfiguration.DefaultOwnerApp ?? options.OwnerApp,
                Name = originalName,
                Description = options.Description ?? DetermineDescription(taskType.GetDocSection("summary")),
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

        private static string DetermineDescription(string description)
        {
            var descriptionProperty = string.IsNullOrEmpty(description)
                ? new JProperty("description", "Missing description")
                : new JProperty("description", description);

            var descriptionObject = new JObject(descriptionProperty);
            return descriptionObject.ToString(Newtonsoft.Json.Formatting.None);
        }
    }
}
