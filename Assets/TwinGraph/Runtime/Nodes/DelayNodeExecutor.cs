using TwinGraph.Runtime.Graph;
using TwinGraph.Runtime.Utils;

namespace TwinGraph.Runtime.Nodes
{
    public sealed class DelayNodeExecutor : INodeExecutor
    {
        public string NodeType => "Delay";

        public NodeResult Execute(NodeData node, ExecutionContext context)
        {
            var seconds = NodeValueParsers.ParseFloat(node.GetParam("seconds", "1"), 1f);
            if (seconds < 0f)
            {
                seconds = 0f;
            }

            return NodeResult.Wait(seconds, "Next");
        }
    }
}
