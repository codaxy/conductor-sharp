using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Health
{
    public class ConductorSharpHealthCheck : IHealthCheck
    {
        private readonly IConductorSharpHealthService _healthService;

        public ConductorSharpHealthCheck(IConductorSharpHealthService healthService)
        {
            _healthService = healthService;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var healthData = await _healthService.GetHealthData(cancellationToken);

            if (healthData.IsExecutionManagerRunning)
            {
                return new HealthCheckResult(HealthStatus.Healthy, "Deployment has been completed and Execution Manager is running");
            }
            else
            {
                return new HealthCheckResult(context.Registration.FailureStatus, "Execution Manager is not running");
            }
        }
    }
}
