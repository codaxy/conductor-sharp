using ConductorSharp.Engine.Tests.Samples.Workflows;
using Newtonsoft.Json;

namespace ConductorSharp.Engine.Tests.Unit;

public class CSharpLambdaTaskBuilderTest
{
    [Fact]
    public void ShouldReturnCorrectDefinition()
    {
        var expectedDefinition = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/Samples/Workflows/CSharpLambda.json");
        var definition = JsonConvert.SerializeObject(new CSharpLambda().GetDefinition(), Formatting.Indented);

        Assert.Equal(expectedDefinition, definition);
    }
}
