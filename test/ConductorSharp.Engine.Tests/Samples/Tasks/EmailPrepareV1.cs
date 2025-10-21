using ConductorSharp.Engine.Builders.Metadata;

namespace ConductorSharp.Engine.Tests.Samples.Tasks;

public partial class EmailPrepareV1Input : ITaskInput<EmailPrepareV1Output>
{
    /// <originalName>
    /// address
    /// </originalName>
    public object Address { get; set; }

    /// <originalName>
    /// name
    /// </originalName>
    public object Name { get; set; }
}

public partial class EmailPrepareV1Output
{
    /// <originalName>
    /// email_body
    /// </originalName>
    public object EmailBody { get; set; }
}

/// <originalName>
/// EMAIL_prepare
/// </originalName>
[OriginalName("EMAIL_prepare")]
public partial class EmailPrepareV1 : SimpleTaskModel<EmailPrepareV1Input, EmailPrepareV1Output> { }
