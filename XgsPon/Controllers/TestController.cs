using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using XgsPon.Models.HandlerRequests;
using XgsPon.Models.HandlerResponses;

namespace Vodafone.Frinx.Infoblox.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public TestController(IMediator mediator, IMapper mapper) => (_mediator, _mapper) = (mediator, mapper);

        [HttpGet]
        public async Task<ActionResult<TestResponse>> Get()
        {
            var response = await _mediator.Send(new TestRequest());
            return Ok(response);
        }
    }
}
