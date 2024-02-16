using System;
using System.Collections.Generic;
using ConductorSharp.Engine.Interface;

namespace ConductorSharp.Engine.Builders
{
    public class DecisionCases<TWorkflow>
        where TWorkflow : ITypedWorkflow
    {
        internal Dictionary<string, Action<ITaskSequenceBuilder<TWorkflow>>> Cases { get; } = [];

        public Action<ITaskSequenceBuilder<TWorkflow>> DefaultCase { get; set; }
        public Action<ITaskSequenceBuilder<TWorkflow>> this[string @case]
        {
            get => Cases[@case];
            set
            {
                if (Cases.ContainsKey(@case))
                    throw new InvalidOperationException($"Case \"{@case}\" already defined");
                Cases[@case] = value;
            }
        }
    }
}
