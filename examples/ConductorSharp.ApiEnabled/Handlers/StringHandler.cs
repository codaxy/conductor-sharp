using ConductorSharp.Engine.Interface;
using MediatR;

namespace ConductorSharp.ApiEnabled.Handlers;

public class StringHandlerRequest : IRequest<StringHandlerResponse>
{
    public string? Word { get; set; }
}

public class StringHandlerResponse
{
    public int WordCount { get; set; }
}
public class StringHandler : ITaskRequestHandler<StringHandlerRequest, StringHandlerResponse>
{
    public Task<StringHandlerResponse> Handle(StringHandlerRequest request, CancellationToken cancellationToken) 
        => Task.FromResult(new StringHandlerResponse { WordCount = request.Word == null ? 0 : request.Word.Length });
}
