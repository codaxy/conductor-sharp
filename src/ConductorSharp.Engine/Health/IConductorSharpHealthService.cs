using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Health
{
    public interface IConductorSharpHealthService
    {
        Task<HealthData> GetHealthData();
    }
}
