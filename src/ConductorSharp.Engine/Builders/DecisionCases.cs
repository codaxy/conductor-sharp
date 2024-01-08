using ConductorSharp.Engine.Interface;
using System;
using System.Collections.Generic;

namespace ConductorSharp.Engine.Builders
{
    public class DecisionCases<TWorkflow> where TWorkflow : ITypedWorkflow
    {
        internal Dictionary<string, Action<ITaskSequenceBuilder<TWorkflow>>> Cases { get; } = new();

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
