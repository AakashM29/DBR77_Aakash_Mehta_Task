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
                if (!HasNonEmptyParam(node, "position"))
                {
                    Debug.LogWarning(
                        $"[TwinGraph] SetTransform node '{node.id}' has applyPosition=true but no 'position' parameter."
                    );
                }

                target.transform.localPosition = NodeValueParsers.ParseVector3(
                    node.GetParam("position", string.Empty),
                    target.transform.localPosition
                );
            }

            if (applyRotation)
            {
                if (!HasNonEmptyParam(node, "rotation"))
                {
                    Debug.LogWarning(
                        $"[TwinGraph] SetTransform node '{node.id}' has applyRotation=true but no 'rotation' parameter."
                    );
                }

                target.transform.localEulerAngles = NodeValueParsers.ParseVector3(
                    node.GetParam("rotation", string.Empty),
                    target.transform.localEulerAngles
                );
            }

            if (applyScale)
            {
                if (!HasNonEmptyParam(node, "scale"))
                {
                    Debug.LogWarning(
                        $"[TwinGraph] SetTransform node '{node.id}' has applyScale=true but no 'scale' parameter."
                    );
                }

                target.transform.localScale = NodeValueParsers.ParseVector3(
                    node.GetParam("scale", string.Empty),
                    target.transform.localScale
                );
            }

            return NodeResult.Next("Next");
        }

        private static bool HasNonEmptyParam(NodeData node, string key)
        {
            if (node.parameters == null)
            {
                return false;
            }

            for (var i = 0; i < node.parameters.Count; i++)
            {
                var parameter = node.parameters[i];
                if (
                    parameter != null
                    && string.Equals(parameter.key, key, System.StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(parameter.value)
                )
                {
                    return true;
                }
            }

            return false;
        }
    }
}
