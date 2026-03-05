using TwinGraph.Runtime.Graph;
using UnityEngine;

namespace TwinGraph.Runtime.Nodes
{
    public sealed class LogNodeExecutor : INodeExecutor
    {
        public string NodeType => "Log";

        public NodeResult Execute(NodeData node, ExecutionContext context)
        {
            var message = node.GetParam("message", "[TwinGraph] Log node executed.");
            Debug.Log(message);
            return NodeResult.Next("Next");
        }
    }
}
