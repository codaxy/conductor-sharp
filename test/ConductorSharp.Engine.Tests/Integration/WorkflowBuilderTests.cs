﻿using ConductorSharp.Engine.Tests.Samples.Workflows;
using ConductorSharp.Engine.Tests.Util;

namespace ConductorSharp.Engine.Tests.Integration
{
    public class WorkflowBuilderTests
    {
        [Fact]
        public void BuilderReturnsCorrectDefinition()
        {
            var definition = SerializationUtil.SerializeObject(new SendCustomerNotification().GetDefinition());
            var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/SendCustomerNotification.json");

            Assert.Equal(expectedDefinition, definition);
        }
    }
}
