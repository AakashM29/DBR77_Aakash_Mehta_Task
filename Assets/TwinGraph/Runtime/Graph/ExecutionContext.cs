using System.Collections.Generic;
using UnityEngine;

namespace TwinGraph.Runtime.Graph
{
    public sealed class ExecutionContext
    {
        public ExecutionContext(Transform root)
        {
            Root = root;
        }

        public Transform Root { get; }

        public Dictionary<string, GameObject> Objects { get; } =
            new Dictionary<string, GameObject>();

        public Dictionary<string, Variant> Vars { get; } = new Dictionary<string, Variant>();

        public void SetVar(string key, Variant value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            Vars[key] = value;
        }

        public bool TryGetVar(string key, out Variant value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                value = default;
                return false;
            }

            return Vars.TryGetValue(key, out value);
        }
    }
}
