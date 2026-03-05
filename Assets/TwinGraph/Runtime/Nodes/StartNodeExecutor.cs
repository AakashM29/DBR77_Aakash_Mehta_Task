using TwinGraph.Runtime.Graph;

namespace TwinGraph.Runtime.Nodes
{
    public sealed class StartNodeExecutor : INodeExecutor
    {
        public string NodeType => "Start";

        public NodeResult Execute(NodeData node, ExecutionContext context)
        {
            return NodeResult.Next("Next");
        }
    }
}
