using System.ComponentModel.DataAnnotations;

namespace ConductorSharp.Engine.Tests.Samples.Workers;

public class GetCustomerRequest : IRequest<GetCustomerResponse>
{
    [Required]
    public int CustomerId { get; set; }
}

public class GetCustomerResponse
{
    public string? Name { get; set; }
    public string? Address { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
}

[OriginalName("CUSTOMER_get")]
public class GetCustomerHandler : ITaskRequestHandler<GetCustomerRequest, GetCustomerResponse>
{
    public Task<GetCustomerResponse> Handle(GetCustomerRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
}
