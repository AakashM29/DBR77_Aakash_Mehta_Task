using TwinGraph.Runtime.Graph;
using TwinGraph.Runtime.Utils;
using UnityEngine;

namespace TwinGraph.Runtime.Nodes
{
    public sealed class SetTransformNodeExecutor : INodeExecutor
    {
        public string NodeType => "SetTransform";

        public NodeResult Execute(NodeData node, ExecutionContext context)
        {
            var objectKey = node.GetParam("objectKey", string.Empty);
            if (string.IsNullOrWhiteSpace(objectKey))
            {
                Debug.LogWarning("[TwinGraph] SetTransform requires a non-empty objectKey.");
                return NodeResult.Next("Next");
            }

            if (!context.Objects.TryGetValue(objectKey, out var target) || target == null)
            {
                Debug.LogWarning(
                    $"[TwinGraph] SetTransform could not find object with key '{objectKey}'."
                );
                return NodeResult.Next("Next");
            }

            var applyPosition = NodeValueParsers.ParseBool(
                node.GetParam("applyPosition", "true"),
                true
            );
            var applyRotation = NodeValueParsers.ParseBool(
                node.GetParam("applyRotation", "true"),
                true
            );
            var applyScale = NodeValueParsers.ParseBool(node.GetParam("applyScale", "true"), true);

            if (applyPosition)
            {
                target.transform.localPosition = NodeValueParsers.ParseVector3(
                    node.GetParam("position", "0,0,0"),
                    target.transform.localPosition
                );
            }

            if (applyRotation)
            {
                target.transform.localEulerAngles = NodeValueParsers.ParseVector3(
                    node.GetParam("rotation", "0,0,0"),
                    target.transform.localEulerAngles
                );
            }

            if (applyScale)
            {
                target.transform.localScale = NodeValueParsers.ParseVector3(
                    node.GetParam("scale", "1,1,1"),
                    target.transform.localScale
                );
            }

            return NodeResult.Next("Next");
        }
    }
}
