namespace ConductorSharp.Engine.Tests.Samples.Tasks;

public partial class CustomerGetV1Input : IRequest<CustomerGetV1Output>
{
    /// <originalName>
    /// customer_id
    /// </originalName>
    public object CustomerId { get; set; }
}

public partial class CustomerGetV1Output
{
    /// <originalName>
    /// name
    /// </originalName>
    public object Name { get; set; }

    /// <originalName>
    /// address
    /// </originalName>
    public object Address { get; set; }
}

/// <originalName>
/// CUSTOMER_get
/// </originalName>
///
[OriginalName("CUSTOMER_get")]
public partial class CustomerGetV1 : SimpleTaskModel<CustomerGetV1Input, CustomerGetV1Output> { }
