using TwinGraph.Runtime.Graph;

namespace TwinGraph.Runtime.Nodes
{
    public interface INodeExecutor
    {
        string NodeType { get; }
        NodeResult Execute(NodeData node, ExecutionContext context);
    }
}
