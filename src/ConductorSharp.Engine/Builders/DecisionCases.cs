using ConductorSharp.Engine.Interface;
using System;
using System.Collections.Generic;

namespace ConductorSharp.Engine.Builders
{
    public class DecisionCases<TWorkflow> where TWorkflow : ITypedWorkflow
    {
        private readonly Dictionary<string, Action<DecisionTaskBuilder<TWorkflow>>> _cases = new();
        public Action<DecisionTaskBuilder<TWorkflow>> DefaultCase { get; set; }

        public Action<DecisionTaskBuilder<TWorkflow>> this[string @case]
        {
            set => _cases[@case] = value;
        }

        internal IReadOnlyDictionary<string, Action<DecisionTaskBuilder<TWorkflow>>> Cases => _cases;
    }
}
