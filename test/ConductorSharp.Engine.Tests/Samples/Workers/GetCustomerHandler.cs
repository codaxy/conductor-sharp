using System.ComponentModel.DataAnnotations;
using ConductorSharp.Engine.Builders.Metadata;

namespace ConductorSharp.Engine.Tests.Samples.Workers;

public class GetCustomerRequest : ITaskInput<GetCustomerResponse>
{
    [Required]
    [JsonProperty("id")]
    public int CustomerId { get; set; }
}

public class GetCustomerResponse
{
    public string Name { get; set; }
    public string Address { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
}

[OriginalName("CUSTOMER_get")]
public class GetCustomerHandler : NgWorker<GetCustomerRequest, GetCustomerResponse>
{
    public override Task<GetCustomerResponse> Handle(GetCustomerRequest request, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
