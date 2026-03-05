using System;
using TwinGraph.Runtime.Graph;
using TwinGraph.Runtime.Utils;
using UnityEngine;

namespace TwinGraph.Runtime.Nodes
{
    public sealed class CreatePrimitiveNodeExecutor : INodeExecutor
    {
        public string NodeType => "CreatePrimitive";

        public NodeResult Execute(NodeData node, ExecutionContext context)
        {
            var shapeName = node.GetParam("shape", node.GetParam("primitiveType", "Cube"));
            var primitiveType = ParsePrimitiveType(shapeName);
            var objectKey = node.GetParam("objectKey", "CreatedPrimitive");
            var position = NodeValueParsers.ParseVector3(
                node.GetParam("position", "0,0,0"),
                Vector3.zero
            );
            var rotation = NodeValueParsers.ParseVector3(
                node.GetParam("rotation", "0,0,0"),
                Vector3.zero
            );
            var scale = NodeValueParsers.ParseVector3(node.GetParam("scale", "1,1,1"), Vector3.one);

            var instance = GameObject.CreatePrimitive(primitiveType);
            instance.name = objectKey;

            if (context.Root != null)
            {
                instance.transform.SetParent(context.Root, false);
                instance.transform.localPosition = position;
                instance.transform.localEulerAngles = rotation;
            }
            else
            {
                instance.transform.position = position;
                instance.transform.eulerAngles = rotation;
            }

            instance.transform.localScale = scale;
            context.Objects[objectKey] = instance;

            return NodeResult.Next("Next");
        }

        private static PrimitiveType ParsePrimitiveType(string value)
        {
            if (Enum.TryParse(value, true, out PrimitiveType parsedType))
            {
                if (
                    parsedType == PrimitiveType.Cube
                    || parsedType == PrimitiveType.Sphere
                    || parsedType == PrimitiveType.Capsule
                    || parsedType == PrimitiveType.Cylinder
                    || parsedType == PrimitiveType.Plane
                    || parsedType == PrimitiveType.Quad
                )
                {
                    return parsedType;
                }
            }

            Debug.LogWarning($"[TwinGraph] Unknown primitive shape '{value}', defaulting to Cube.");
            return PrimitiveType.Cube;
        }
    }
}
