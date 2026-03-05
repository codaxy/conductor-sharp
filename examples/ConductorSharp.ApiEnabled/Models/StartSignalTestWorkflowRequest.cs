using System.ComponentModel.DataAnnotations;

namespace ConductorSharp.ApiEnabled.Models;

public record StartSignalTestWorkflowRequest(string SignalKey, string Message);
