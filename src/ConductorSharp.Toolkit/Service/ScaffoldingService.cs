﻿using ConductorSharp.Client.Model.Common;
using ConductorSharp.Client.Service;
using ConductorSharp.Engine.Util;
using ConductorSharp.Toolkit.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ConductorSharp.Toolkit.Service
{
    public class ScaffoldingService : IScaffoldingService
    {
        private readonly IMetadataService _metadataService;
        private readonly ILogger<ScaffoldingService> _logger;
        private readonly ScaffoldingConfig _config;

        public ScaffoldingService(IMetadataService metadataService, IOptions<ScaffoldingConfig> options, ILogger<ScaffoldingService> logger)
        {
            _metadataService = metadataService;
            _logger = logger;
            _config = options.Value;
        }

        public async Task Scaffold()
        {
            var workflowDefinitions = await _metadataService.GetAllWorkflowDefinitions();
            var workflowCollectionBuilder = new StringBuilder();
            foreach (var workflowDefinition in workflowDefinitions)
            {
                var workflowClass = await CreateWorkflowClass(workflowDefinition);

                if (workflowClass != null)
                    workflowCollectionBuilder.Append(workflowClass);
            }

            var taskDefinitions = await _metadataService.GetAllTaskDefinitions();

            var taskCollectionBuilder = new StringBuilder();
            foreach (var taskDefinition in taskDefinitions)
            {
                var taskClass = await CreateTaskClass(taskDefinition);

                if (taskClass != null)
                    taskCollectionBuilder.Append(taskClass);
            }

            var taskTemplate = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Templates/TaskCollectionTemplate.default");
            var taskCollection = taskCollectionBuilder.ToString();
            taskTemplate = taskTemplate.Replace("{{namespace}}", _config.BaseNamespace + ".Tasks");
            taskTemplate = taskTemplate.Replace("{{taskCollection}}", taskCollection);
            var dir = _config.Destination + Path.DirectorySeparatorChar + "Tasks";

            Directory.CreateDirectory(dir);

            File.WriteAllText(dir + Path.DirectorySeparatorChar + "Tasks.scaff.cs", taskTemplate);

            var workflowTemplate = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Templates/WorkflowCollectionTemplate.default");
            var workflowCollection = workflowCollectionBuilder.ToString();
            workflowTemplate = workflowTemplate.Replace("{{namespace}}", _config.BaseNamespace + ".Workflows");
            workflowTemplate = workflowTemplate.Replace("{{workflowCollection}}", workflowCollection);
            var workflowDir = _config.Destination + Path.DirectorySeparatorChar + "Workflows";

            Directory.CreateDirectory(workflowDir);

            File.WriteAllText(workflowDir + Path.DirectorySeparatorChar + "Workflows.scaff.cs", workflowTemplate);
        }

        public async Task<string> CreateWorkflowClass(WorkflowDefinition workflowDefinition)
        {
            var lines = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Templates/WorkflowTemplate.default");

            string name = SnakeCaseUtil.ToPascalCase($"{workflowDefinition.Name}_V{workflowDefinition.Version}").Trim();
            string note = null;
            if (LangUtils.MakeValidMemberName(name, "A", out name))
            {
                note = "The autogenerated name has been changed because it is invalid C# member name";
            }

            string workflowDescription = workflowDefinition.Description;
            string[] labels;

            try
            {
                var descriptionObject = JObject.Parse(workflowDescription);
                workflowDescription = descriptionObject.SelectToken("description")?.Value<string>();
                var labelsObject = descriptionObject.SelectToken("labels");

                if (labelsObject is JArray labelsArray)
                {
                    labels = labelsArray.ToObject<string[]>();
                }
            }
            catch (Exception) { }

            var inputLinesBuilder = new StringBuilder();
            try
            {
                var inputParameters = JObject.Parse(workflowDefinition.InputParametersJSON[0]);

                foreach (var param in inputParameters)
                {
                    var value = param.Value.SelectToken("value")?.ToString(Newtonsoft.Json.Formatting.None);
                    var type = param.Value.SelectToken("type")?.Value<string>();
                    var description = param.Value.SelectToken("description")?.Value<string>();

                    if (type == null)
                        type = "string";

                    inputLinesBuilder.AppendLine($"        /// <originalName>");
                    inputLinesBuilder.AppendLine($"        /// {param.Key}");
                    inputLinesBuilder.AppendLine($"        /// </originalName>");

                    if (!string.IsNullOrEmpty(description))
                    {
                        inputLinesBuilder.AppendLine($"        /// <summary>");
                        inputLinesBuilder.AppendLine($"        /// {description}");
                        inputLinesBuilder.AppendLine($"        /// </summary>");
                    }

                    string propertyName;

                    switch (type)
                    {
                        case "string":
                        case "integer":
                            if (!string.IsNullOrEmpty(value))
                            {
                                inputLinesBuilder.AppendLine($"        /// <remark>");
                                inputLinesBuilder.AppendLine($"        /// Example: {value}");
                                inputLinesBuilder.AppendLine($"        /// </remark>");
                            }
                            inputLinesBuilder.AppendLine($"        [JsonProperty(\"{param.Key}\")]");
                            LangUtils.MakeValidMemberName(SnakeCaseUtil.ToPascalCase(param.Key), "A", out propertyName);
                            inputLinesBuilder.AppendLine($"        public dynamic {propertyName} {{ get; set; }}");
                            break;
                        case "toggle":
                            inputLinesBuilder.AppendLine($"        [JsonProperty(\"{param.Key}\")]");
                            LangUtils.MakeValidMemberName(SnakeCaseUtil.ToPascalCase(param.Key), "A", out propertyName);
                            inputLinesBuilder.AppendLine($"        public dynamic {propertyName} {{ get; set; }}");
                            break;
                        case "select":
                            var optionsToken = param.Value.SelectToken("options");

                            string[] options = null;

                            if (optionsToken is JArray optionsArray)
                            {
                                options = optionsArray.ToObject<string[]>();
                            }
                            if (options != null && options.Length > 0)
                            {
                                inputLinesBuilder.AppendLine($"        /// <remark>");
                                inputLinesBuilder.AppendLine($"        /// Options: {string.Join(',', options)}");
                                inputLinesBuilder.AppendLine($"        /// </remark>");
                            }
                            inputLinesBuilder.AppendLine($"        [JsonProperty(\"{param.Key}\")]");
                            LangUtils.MakeValidMemberName(SnakeCaseUtil.ToPascalCase(param.Key), "A", out propertyName);
                            inputLinesBuilder.AppendLine($"        public dynamic {propertyName} {{ get; set; }}");
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception)
            {
                if (workflowDefinition.InputParametersJSON != null)
                    Console.WriteLine(
                        $"Unable to parse input parameters {string.Join(',', workflowDefinition.InputParametersJSON)} for {workflowDefinition.Name})"
                    );
                else
                    Console.WriteLine($"No input parameters defined for {workflowDefinition.Name}");
            }

            if (string.IsNullOrEmpty(workflowDefinition.OwnerEmail))
                _logger.LogWarning($"No owner email defined for task {workflowDefinition.Name}");

            if (string.IsNullOrEmpty(workflowDefinition.OwnerApp))
                _logger.LogWarning($"No owner app defined for task {workflowDefinition.Name}");

            lines = lines.Replace("{{inputProperties}}", inputLinesBuilder.ToString());
            lines = lines.Replace("{{workflowName}}", name);
            lines = lines.Replace("{{note}}", note);
            lines = lines.Replace("{{ownerEmail}}", $"{workflowDefinition.OwnerEmail}");
            lines = lines.Replace("{{ownerApp}}", $"{workflowDefinition.OwnerApp}");
            lines = lines.Replace("{{originalName}}", $"{workflowDefinition.Name}");
            lines = lines.Replace("{{description}}", workflowDescription);
            lines = lines.Replace("{{commentDescription}}", workflowDescription?.Replace('\n', ','));

            if (_config.Dryrun)
                return null;

            return lines;
        }
        public async Task<string> CreateTaskClass(TaskDefinition taskDefinition)
        {
            var lines = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Templates/WorkerTemplate.default");

            var namespaceRegex = new Regex("(^[^_]+)_.+");

            var matches = namespaceRegex.Match(taskDefinition.Name).Groups;

            string naspace = "";
            string name = "";

            if (matches.Count > 1)
            {
                naspace = matches[1].Value.ToLowerInvariant();
                naspace = char.ToUpperInvariant(naspace[0]) + naspace[1..];
            }

            name = SnakeCaseUtil.ToPascalCase(taskDefinition.Name).Trim();
            string note = null;
            if (LangUtils.MakeValidMemberName(name, "A", out name))
            {
                note = "The autogenerated name has been prepended because it starts with a digit character.";
            }

            //name = char.ToUpperInvariant(name[0]) + name[1..];

            var inputLinesBuilder = new StringBuilder();
            string propertyName;
            foreach (var property in taskDefinition.InputKeys)
            {
                inputLinesBuilder.AppendLine($"        /// <originalName>");
                inputLinesBuilder.AppendLine($"        /// {property}");
                inputLinesBuilder.AppendLine($"        /// </originalName>");
                inputLinesBuilder.AppendLine($"        [JsonProperty(\"{property}\")]");
                LangUtils.MakeValidMemberName(SnakeCaseUtil.ToPascalCase(property), "A", out propertyName);
                inputLinesBuilder.AppendLine($"        public dynamic {propertyName} {{ get; set; }}");
            }

            var outputLinesBuilder = new StringBuilder();
            foreach (var property in taskDefinition.OutputKeys)
            {
                outputLinesBuilder.AppendLine($"        /// <originalName>");
                outputLinesBuilder.AppendLine($"        /// {property}");
                outputLinesBuilder.AppendLine($"        /// </originalName>");
                outputLinesBuilder.AppendLine($"        [JsonProperty(\"{property}\")]");
                LangUtils.MakeValidMemberName(SnakeCaseUtil.ToPascalCase(property), "A", out propertyName);
                outputLinesBuilder.AppendLine($"        public dynamic {propertyName} {{ get; set; }}");
            }

            var generatedNamespace = _config.BaseNamespace + ".Tasks" + (!string.IsNullOrEmpty(naspace) ? $".{naspace}" : "");

            lines = lines.Replace("{{workerName}}", name);
            lines = lines.Replace("{{note}}", note);
            lines = lines.Replace("{{inputProperties}}", inputLinesBuilder.ToString());
            lines = lines.Replace("{{outputProperties}}", outputLinesBuilder.ToString());
            lines = lines.Replace("{{namespace}}", $"{generatedNamespace}");
            lines = lines.Replace("{{ownerEmail}}", $"{taskDefinition.OwnerEmail}");
            lines = lines.Replace("{{ownerApp}}", $"{taskDefinition.OwnerApp}");
            lines = lines.Replace("{{originalName}}", $"{taskDefinition.Name}");

            var description = "";
            try
            {
                description = JObject.Parse(taskDefinition.Description).SelectToken("description").Value<string>();
            }
            catch (Exception)
            {
                _logger.LogWarning($"Invalid description '{taskDefinition.Description}' in task {taskDefinition.Name}");
            }

            var ownerEmail = taskDefinition.OwnerEmail;
            var ownerApp = taskDefinition.OwnerApp;

            if (string.IsNullOrEmpty(ownerEmail))
                _logger.LogWarning($"No owner email defined for task {taskDefinition.Name}");

            if (string.IsNullOrEmpty(ownerApp))
                _logger.LogWarning($"No owner app defined for task {taskDefinition.Name}");

            lines = lines.Replace("{{description}}", description);
            lines = lines.Replace("{{commentDescription}}", description.Replace('\n', ','));

            if (_config.Dryrun)
                return null;

            return lines;
        }
    }
}
