using System;
using System.Collections.Generic;

namespace TwinGraph.Runtime.Nodes
{
    public static class NodeRegistry
    {
        private static readonly Dictionary<string, INodeExecutor> Executors = new Dictionary<
            string,
            INodeExecutor
        >(StringComparer.OrdinalIgnoreCase);

        private static bool defaultsRegistered;

        public static void EnsureDefaultsRegistered()
        {
            if (defaultsRegistered)
            {
                return;
            }

            Register(new StartNodeExecutor());
            Register(new CreatePrimitiveNodeExecutor());
            Register(new DelayNodeExecutor());
            Register(new SetTransformNodeExecutor());
            Register(new LogNodeExecutor());
            defaultsRegistered = true;
        }

        public static void Register(INodeExecutor executor)
        {
            if (executor == null || string.IsNullOrWhiteSpace(executor.NodeType))
            {
                return;
            }

            Executors[executor.NodeType] = executor;
        }

        public static bool TryGet(string nodeType, out INodeExecutor executor)
        {
            if (string.IsNullOrWhiteSpace(nodeType))
            {
                executor = null;
                return false;
            }

            return Executors.TryGetValue(nodeType, out executor);
        }
    }
}
