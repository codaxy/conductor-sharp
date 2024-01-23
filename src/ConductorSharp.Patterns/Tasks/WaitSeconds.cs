using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Engine;
using ConductorSharp.Engine.Builders.Metadata;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util;
using MediatR;

namespace ConductorSharp.Patterns.Tasks
{
    public class WaitSecondsRequest : IRequest<NoOutput>
    {
        /// <summary>
        /// Time to wait in seconds
        /// </summary>
        [Required]
        [Range(1, int.MaxValue)]
        public int Seconds { get; set; }
    }

    /// <summary>
    /// Executes `await Task.Delay(input.Seconds * 1000)` to wait for a given amount of seconds
    /// </summary>
    [OriginalName(Constants.TaskNamePrefix + "_wait_seconds")]
    public class WaitSeconds : TaskRequestHandler<WaitSecondsRequest, NoOutput>
    {
        public override async Task<NoOutput> Handle(WaitSecondsRequest input, CancellationToken cancellationToken)
        {
            await Task.Delay(input.Seconds * 1000, cancellationToken);
            return new NoOutput();
        }
    }
}
