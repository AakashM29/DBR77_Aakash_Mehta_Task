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
    }
}
