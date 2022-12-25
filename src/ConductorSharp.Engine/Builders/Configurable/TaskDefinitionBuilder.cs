using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util;
using ConductorSharp.Engine.Util.Builders;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace ConductorSharp.Engine.Builders.Configurable
{
    public class TaskDefinitionBuilder
    {
        public BuildConfiguration BuildConfiguration { get; set; }

        public TaskDefinitionBuilder(BuildConfiguration buildConfiguration)
        {
            BuildConfiguration = buildConfiguration;
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

            var originalName = NamingUtil.DetermineRegistrationName(taskType);

            return new TaskDefinition
            {
                OwnerApp = BuildConfiguration.DefaultOwnerApp ?? options.OwnerApp,
                Name = originalName,
                Description = options.Description ?? DetermineDescription(taskType.GetDocSection("summary")),
                RetryCount = options.RetryCount,
                TimeoutSeconds = options.TimeoutSeconds,
                InputKeys = inputType
                    .GetProperties()
                    .Select(a => a.GetDocSection("originalName") ?? SnakeCaseUtil.ToCapitalizedPrefixSnakeCase(a.Name))
                    .ToList(),
                OutputKeys = outputType
                    .GetProperties()
                    .Select(a => a.GetDocSection("originalName") ?? SnakeCaseUtil.ToCapitalizedPrefixSnakeCase(a.Name))
                    .ToList(),
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

        private string DetermineDescription(string description, params string[] labels)
        {
            var descriptionProperty = string.IsNullOrEmpty(description)
                ? new JProperty("description", "Missing description")
                : new JProperty("description", description);

            var descriptionObject = new JObject(descriptionProperty);
            return descriptionObject.ToString(Newtonsoft.Json.Formatting.None);
        }
    }
}
