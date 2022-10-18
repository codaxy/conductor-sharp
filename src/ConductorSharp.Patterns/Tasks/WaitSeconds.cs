using ConductorSharp.Engine;
using ConductorSharp.Engine.Util;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Patterns.Tasks
{
    public class WaitSecondsRequest : IRequest<WaitSecondsResponse>
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int Seconds { get; set; }
    }

    public class WaitSecondsResponse { }

    [OriginalName(Constants.TaskNamePrefix + "_wait_seconds")]
    public class WaitSeconds : TaskRequestHandler<WaitSecondsRequest, WaitSecondsResponse>
    {
        private readonly ILogger<WaitSeconds> _logger;

        public WaitSeconds(ILogger<WaitSeconds> logger) => _logger = logger;

        public async override Task<WaitSecondsResponse> Handle(WaitSecondsRequest input, CancellationToken cancellationToken)
        {
            await Task.Delay(input.Seconds * 1000);
            return new WaitSecondsResponse();
        }
    }
}
