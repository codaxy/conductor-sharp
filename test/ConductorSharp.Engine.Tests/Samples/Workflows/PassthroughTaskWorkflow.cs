using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class PassthroughTaskWorkflowInput : WorkflowInput<PassthroughTaskWorkflowOutput> { }

    public class PassthroughTaskWorkflowOutput : WorkflowOutput { }

    public class PassthroughTaskWorkflow : Workflow<PassthroughTaskWorkflow, PassthroughTaskWorkflowInput, PassthroughTaskWorkflowOutput>
    {
        public PassthroughTaskWorkflow(
            WorkflowDefinitionBuilder<PassthroughTaskWorkflow, PassthroughTaskWorkflowInput, PassthroughTaskWorkflowOutput> builder
        ) : base(builder) { }

        public override void BuildDefinition()
        {
            _builder.AddTasks(
                new WorkflowDefinition.Task
                {
                    Name = "LAMBDA_return_data",
                    TaskReferenceName = "return_data",
                    Type = "LAMBDA",
                    Description = "Lambda task to return data",
                    InputParameters = new JObject
                    {
                        new JProperty("hostname", "${workflow.input.hostname}"),
                        new JProperty("additional_template", "${workflow.input.additional_template}"),
                        new JProperty("base_template", "${workflow.input.base_template}"),
                        new JProperty("licence", "${workflow.input.licence}"),
                        new JProperty("oam_domain", "${workflow.input.oam_domain}"),
                        new JProperty("platform_codename", "${workflow.input.platform_codename}"),
                        new JProperty("platform_name", "${workflow.input.platform_name}"),
                        new JProperty("software_version", "${workflow.input.software_version}"),
                        new JProperty("upstream_switch", "${workflow.input.upstream_switch}"),
                        new JProperty("upstream_switch_interface_name", "${workflow.input.upstream_switch_interface_name}"),
                        new JProperty(
                            "scriptExpression",
                            "return { "
                                + "hostname : $.hostname,"
                                + "additional_template : $.additional_template,"
                                + "base_template : $.base_template,"
                                + "licence : $.licence,"
                                + "oam_domain : $.oam_domain,"
                                + "platform_codename : $.platform_codename,"
                                + "platform_name : $.platform_name,"
                                + "software_version : $.software_version,"
                                + "upstream_switch : $.upstream_switch,"
                                + "upstream_switch_interface_name : $.upstream_switch_interface_name,"
                                + "}"
                        )
                    }
                }
            );
        }
    }
}
