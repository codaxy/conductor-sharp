using ConductorSharp.Engine.Interface;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ConductorSharp.ApiEnabled.Handlers;

public class NumberHandlerRequest : IRequest<NumberHandlerResponse>
{
    [Required]
    public int Number { get; set; }
}

public class NumberHandlerResponse
{
    public bool IsNegative { get; set; }
    public bool IsOdd { get; set; }
}

public class NumberHandler : ITaskRequestHandler<NumberHandlerRequest, NumberHandlerResponse>
{
    public Task<NumberHandlerResponse> Handle(NumberHandlerRequest request, CancellationToken cancellationToken)
        => Task.FromResult(new NumberHandlerResponse
        {
            IsOdd = request.Number % 2 != 0,
            IsNegative = request.Number < 0
        });
}
