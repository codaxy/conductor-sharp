using MediatR;

namespace ConductorSharp.Engine.Model
{
    public class DynamicForkJoinInput : IRequest<NoOutput>
    {
        public object DynamicTasks { get; set; }

        public object DynamicTasksI { get; set; }
    }

    public class DynamicForkJoinTaskModel : TaskModel<DynamicForkJoinInput, NoOutput> { }
}
