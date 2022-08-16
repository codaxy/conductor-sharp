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
            var workflowDirectory = Path.Combine(_config.Destination, "Workflows");
            Directory.CreateDirectory(workflowDirectory);
            foreach (var workflowDefinition in workflowDefinitions)
            {
                (var contents, var modelClassName) = CreateWorkflowClass(workflowDefinition);

                if (contents != null)
                {
                    var filePath = Path.Combine(workflowDirectory, $"{modelClassName}.scaff.cs");
                    File.WriteAllText(filePath, contents);
                }
            }

            var taskDefinitions = await _metadataService.GetAllTaskDefinitions();
            var tasksDirectory = Path.Combine(_config.Destination, "Tasks");
            Directory.CreateDirectory(tasksDirectory);
            foreach (var taskDefinition in taskDefinitions)
            {
                (var contents, var modelClassName) = CreateTaskClass(taskDefinition);

                if (contents != null)
                {
                    var filePath = Path.Combine(tasksDirectory, $"{modelClassName}.scaff.cs");
                    File.WriteAllText(filePath, contents);
                }
            }
        }

        public (string contents, string modelClassName) CreateWorkflowClass(WorkflowDefinition workflowDefinition)
        {
            string name = SnakeCaseUtil.ToPascalCase($"{workflowDefinition.Name}_V{workflowDefinition.Version}").Trim();
            string note = null;
            if (LangUtils.MakeValidMemberName(name, "A", out name))
            {
                note = "The autogenerated name has been changed because it is invalid C# member name";
            }

            var modelGenerator = new TaskModelGenerator(_config.BaseNamespace + ".Workflows", name, TaskModelGenerator.ModelType.Workflow);
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
            catch (Exception)
            {
                _logger.LogWarning("Unable to parse description for workflow {0}", workflowDefinition.Name);
            }

            try
            {
                var inputParameters = JObject.Parse(workflowDefinition.InputParametersJSON[0]);

                foreach (var param in inputParameters)
                {
                    var inputPropData = new TaskModelGenerator.PropertyData();

                    var value = param.Value.SelectToken("value")?.ToString(Newtonsoft.Json.Formatting.None);
                    var type = param.Value.SelectToken("type")?.Value<string>();
                    var description = param.Value.SelectToken("description")?.Value<string>();

                    if (type == null)
                        type = "string";

                    inputPropData.XmlComments["originalName"] = param.Key;

                    if (!string.IsNullOrEmpty(description))
                        inputPropData.XmlComments["summary"] = description;

                    switch (type)
                    {
                        case "string":
                        case "integer":
                            if (!string.IsNullOrEmpty(value))
                            {
                                inputPropData.XmlComments["remark"] = $"Example: {value}";
                            }
                            DefinePropertyParams(inputPropData, "dynamic", param.Key);
                            modelGenerator.AddInputProperty(inputPropData);

                            break;
                        case "toggle":
                            DefinePropertyParams(inputPropData, "dynamic", param.Key);
                            modelGenerator.AddInputProperty(inputPropData);
                            break;
                        case "select":
                            var optionsToken = param.Value.SelectToken("options");

                            string[] options = null;

                            if (optionsToken is JArray optionsArray)
                            {
                                options = optionsArray.ToObject<string[]>();
                            }
                            if (options != null && options.Length > 0)
                                inputPropData.XmlComments["remark"] = $"Options: {string.Join(',', options)}";

                            DefinePropertyParams(inputPropData, "dynamic", param.Key);
                            modelGenerator.AddInputProperty(inputPropData);
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

            modelGenerator.AddXmlComment("summary", workflowDescription?.Replace('\n', ','));
            modelGenerator.AddXmlComment("originalName", workflowDefinition.Name);
            modelGenerator.AddXmlComment("ownerApp", workflowDefinition.OwnerApp);
            modelGenerator.AddXmlComment("ownerEmail", workflowDefinition.OwnerEmail);
            modelGenerator.AddXmlComment("note", note);

            if (_config.Dryrun)
                return (null, null);

            return (modelGenerator.Build(), name);
        }

        private void AppendClassProperty(StringBuilder linesBuilder, string type, string name)
        {
            linesBuilder.AppendLine($"        [JsonProperty(\"{name}\")]");
            LangUtils.MakeValidMemberName(SnakeCaseUtil.ToPascalCase(name), "A", out var propertyName);
            linesBuilder.AppendLine($"        public {type} {propertyName} {{ get; set; }}");
        }

        private void DefinePropertyParams(TaskModelGenerator.PropertyData propData, string type, string originalName)
        {
            LangUtils.MakeValidMemberName(SnakeCaseUtil.ToPascalCase(originalName), "A", out var propertyName);
            propData.OriginalName = originalName;
            propData.Name = propertyName;
            propData.Type = type;
        }

        private void AppendXmlComment(StringBuilder linesBuilder, string tag, string value)
        {
            linesBuilder.AppendLine($"        /// <{tag}>");
            linesBuilder.AppendLine($"        /// {value}");
            linesBuilder.AppendLine($"        /// </{tag}>");
        }

        public (string contents, string modelClassName) CreateTaskClass(TaskDefinition taskDefinition)
        {
            string name = SnakeCaseUtil.ToPascalCase(taskDefinition.Name).Trim();
            string note = null;
            if (LangUtils.MakeValidMemberName(name, "A", out name))
            {
                note = "The autogenerated name has been prepended because it starts with a digit character.";
            }

            var modelGenerator = new TaskModelGenerator(_config.BaseNamespace + ".Tasks", name, TaskModelGenerator.ModelType.Task);
            foreach (var property in taskDefinition.InputKeys)
            {
                var inputPropData = new TaskModelGenerator.PropertyData();
                DefinePropertyParams(inputPropData, "dynamic", property);
                inputPropData.XmlComments["originalName"] = property;
                modelGenerator.AddInputProperty(inputPropData);
            }

            foreach (var property in taskDefinition.OutputKeys)
            {
                var inputPropData = new TaskModelGenerator.PropertyData();
                DefinePropertyParams(inputPropData, "dynamic", property);
                inputPropData.XmlComments["originalName"] = property;
                modelGenerator.AddOutputProperty(inputPropData);
            }

            modelGenerator.AddXmlComment("originalName", taskDefinition.Name);
            modelGenerator.AddXmlComment("ownerEmail", taskDefinition.OwnerEmail);
            modelGenerator.AddXmlComment("node", note);

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

            modelGenerator.AddXmlComment("summary", description.Replace('\n', ','));
            if (_config.Dryrun)
                return (null, null);

            return (modelGenerator.Build(), name);
        }
    }
}
