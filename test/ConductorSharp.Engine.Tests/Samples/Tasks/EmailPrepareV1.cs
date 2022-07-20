namespace ConductorSharp.Engine.Tests.Samples.Tasks;

public partial class EmailPrepareV1Input : IRequest<EmailPrepareV1Output>
{
    /// <originalName>
    /// address
    /// </originalName>
    public dynamic Address { get; set; }

    /// <originalName>
    /// name
    /// </originalName>
    public dynamic Name { get; set; }
}

public partial class EmailPrepareV1Output
{
    /// <originalName>
    /// email_body
    /// </originalName>
    public dynamic EmailBody { get; set; }
}

/// <originalName>
/// EMAIL_prepare
/// </originalName>
[OriginalName("EMAIL_prepare")]
public partial class EmailPrepareV1 : SimpleTaskModel<EmailPrepareV1Input, EmailPrepareV1Output> { }
