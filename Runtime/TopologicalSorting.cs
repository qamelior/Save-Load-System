using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SaveLoadSystem
{
    public class TopologicalSorting<T>
    {
        private readonly List<T> _unsorted = new();
        private readonly List<T> _result = new();
        private readonly Dictionary<T, bool> _visited = new();
        private readonly Dictionary<T, IEnumerable<T>> _dependencies = new();
        private readonly int[] _nodes;
        private readonly string _systemName;

        public TopologicalSorting(string nameOfTheInvokerSystem) => _systemName = nameOfTheInvokerSystem;

        public void AddNode(T element, IEnumerable<T> dependencies)
        {
            _unsorted.Add(element);
            _dependencies.Add(element, dependencies);
        }

        public IEnumerable<T> Execute() => _unsorted.All(Visit) ? _result : null;

        private bool Visit(T node)
        {
            var alreadyVisited = _visited.TryGetValue(node, out var inProcess);
            if (alreadyVisited)
            {
                if (inProcess)
                    Debug.LogError($"[Entity sorting] Circular dependency found ({node}) when trying to sort {_systemName}");
                return !inProcess;
            }

            _visited[node] = true;
            foreach (var dependee in  _dependencies[node])
            {
                if (Visit(dependee)) continue;
                Debug.LogError($"[Entity sorting] Circular dependency found ({dependee}) when trying to sort {_systemName}");
                return false;
            }

            _visited[node] = false;
            _result.Add(node);
            return true;
        }
    }
}