using MediatR;

namespace ConductorSharp.Engine.Model
{

    public class DynamicForkJoinInput : IRequest<NoOutput>
    {
        public dynamic          DynamicTasks { get; set; }

        public dynamic DynamicTasksI { get; set; }
    }
    public class DynamicForkJoinTaskModel : TaskModel<DynamicForkJoinInput, NoOutput>
    {
    }
}