using AutoMapper;
using MediatR;
using XgsPon.Workflows.Engine.Interface;
using XgsPon.Workflows.Engine.Util;
using XgsPon.Models.HandlerRequests;
using XgsPon.Models.HandlerResponses;

namespace XgsPn.Handlers
{
    [OriginalName("TEST_worker")]
    public class TestHandler : ITaskRequestHandler<TestRequest, TestResponse>
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public TestHandler(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<TestResponse> Handle(
            TestRequest request,
            CancellationToken cancellationToken
        )
        {
            return await Task.FromResult(new TestResponse());
        }
    }
}
