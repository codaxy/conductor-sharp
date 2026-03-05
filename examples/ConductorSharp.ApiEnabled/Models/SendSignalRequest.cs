using System.ComponentModel.DataAnnotations;

namespace ConductorSharp.ApiEnabled.Models;

public record SendSignalRequest(string SignalKey, Dictionary<string, object> OutputData);
