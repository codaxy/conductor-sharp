namespace ConductorSharp.Engine.Extensions
{
    public interface IPipelineBuilder
    {
        void AddRequestResponseLogging();
        void AddValidation();
        void AddContextLogging();
        void AddExecutionTaskTracking();
    }
}
