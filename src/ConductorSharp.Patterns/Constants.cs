namespace ConductorSharp.Patterns
{
    public static class Constants
    {
        public const string TaskNamePrefix = "CSH_PATTERNS";

        public const string SignalWaitWorkflowName = "signal_wait";
        public const string RegisterWaiterTaskName = "register_signal_waiter";
        public const string SignalPrefixConfigurationProperty = nameof(SignalPrefixConfigurationProperty);
    }
}
