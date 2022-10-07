using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Health
{
    public class DeploymentHealthCheck : IHealthCheck
    {
        private readonly IConductorSharpHealthService _healthService;

        public DeploymentHealthCheck(IConductorSharpHealthService healthService)
        {
            _healthService = healthService;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var healthData = await _healthService.GetHealthData();

            if (healthData.IsDeploymentCompleted)
            {
                return new HealthCheckResult(HealthStatus.Healthy, "Deployment completed");
            }
            else
            {
                return new HealthCheckResult(context.Registration.FailureStatus, "Deployment not completed");
            }
        }
    }
}
