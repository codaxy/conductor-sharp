using MediatR;
using XgsPon.Models.HandlerResponses;

namespace XgsPon.Models.HandlerRequests
{
    public class TestRequest : IRequest<TestResponse>
    {
        public string? Request { get; set; }
    }
}
