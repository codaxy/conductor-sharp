using AutoMapper;
using ConductorSharp.ApiEnabled.Handlers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ConductorSharp.ApiEnabled.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ExampleController : ControllerBase
{
    private readonly IMediator mediator;

    public ExampleController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpPost("number")]
    public async Task<NumberHandlerResponse> InvokeNumberHandler([FromBody] NumberHandlerRequest request) => await mediator.Send(request);

    [HttpPost("string")]
    public async Task<StringHandlerResponse> InvokeStringHandler([FromBody] StringHandlerRequest request) => await mediator.Send(request);
}
