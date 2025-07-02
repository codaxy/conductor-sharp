using System.ComponentModel.DataAnnotations;
using ConductorSharp.Engine.Builders.Metadata;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;

namespace ConductorSharp.NoApi.Handlers;

public class GetCustomerRequest : ITaskInput<GetCustomerResponse>
{
    [Required]
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
public class GetCustomerHandler : INgWorker<GetCustomerRequest, GetCustomerResponse>
{
    private static Customer[] customers = new Customer[]
    {
        new Customer
        {
            Id = 1,
            Address = "Baker Street 221b",
            Name = "Sherlock Holmes"
        }
    };

    public Task<GetCustomerResponse> Handle(GetCustomerRequest request, WorkerExecutionContext context, CancellationToken cancellationToken)
    {
        var customer = customers.First(a => a.Id == request.CustomerId);

        return Task.FromResult(new GetCustomerResponse { Name = customer.Name, Address = customer.Address });
    }
}
