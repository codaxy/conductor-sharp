using ConductorSharp.Engine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConductorSharp.Engine.Builders
{
    public class WorkflowBuildItemRegistry
    {
        private readonly Dictionary<Type, Dictionary<string, object>> _items = new();

        public void Register<TWorkflow>(string key, object value) where TWorkflow : IConfigurableWorkflow
        {
            if (_items.TryGetValue(typeof(TWorkflow), out var item))
            {
                item[key] = value;
            }
            else
            {
                _items.Add(typeof(TWorkflow), new() { { key, value } });
            }
        }

        public bool TryGet<TWorkflow>(out Dictionary<string, object> items) where TWorkflow : IConfigurableWorkflow =>
            _items.TryGetValue(typeof(TWorkflow), out items);

        public List<T> GetAll<T>()
        {
            return _items.Values.SelectMany(a => a.Values).Where(a => a is T).Select(b => (T)b).ToList();
        }
    }
}
