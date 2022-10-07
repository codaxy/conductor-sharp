using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Health
{
    internal interface IConductorSharpHealthUpdater
    {
        Task ResetHealthData();
        Task RemoveHealthData();
        Task SetDeploymentCompleted();

        Task SetDeploymentStarted();

        Task SetExecutionManagerStarted();
    }
}
