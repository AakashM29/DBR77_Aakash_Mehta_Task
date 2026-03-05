using System.Globalization;
using TwinGraph.Runtime.Graph;
using TwinGraph.Runtime.Utils;
using UnityEngine;

namespace TwinGraph.Runtime.Nodes
{
    public sealed class SetVarNodeExecutor : INodeExecutor
    {
        public string NodeType => "SetVar";

        public NodeResult Execute(NodeData node, ExecutionContext context)
        {
            var varName = node.GetParam("varName", string.Empty);
            if (string.IsNullOrWhiteSpace(varName))
            {
                Debug.LogWarning("[TwinGraph] SetVar requires a non-empty varName.");
                return NodeResult.Next("Next");
            }

            var valueType = node.GetParam("valueType", "string");
            var rawValue = node.GetParam("value", string.Empty);
            var parsed = ParseVariant(valueType, rawValue);
            context.SetVar(varName, parsed);

            return NodeResult.Next("Next");
        }

        private static Variant ParseVariant(string valueType, string rawValue)
        {
            switch (valueType.Trim().ToLowerInvariant())
            {
                case "int":
                case "integer":
                    if (
                        int.TryParse(
                            rawValue,
                            NumberStyles.Integer,
                            CultureInfo.InvariantCulture,
                            out var i
                        )
                    )
                    {
                        return Variant.FromInt(i);
                    }

                    Debug.LogWarning(
                        $"[TwinGraph] SetVar could not parse int '{rawValue}'. Using 0."
                    );
                    return Variant.FromInt(0);

                case "float":
                case "single":
                case "number":
                    if (
                        float.TryParse(
                            rawValue,
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture,
                            out var f
                        )
                    )
                    {
                        return Variant.FromFloat(f);
                    }

                    Debug.LogWarning(
                        $"[TwinGraph] SetVar could not parse float '{rawValue}'. Using 0."
                    );
                    return Variant.FromFloat(0f);

                case "bool":
                case "boolean":
                    return Variant.FromBool(NodeValueParsers.ParseBool(rawValue, false));

                case "vector3":
                case "vec3":
                    return Variant.FromVector3(
                        NodeValueParsers.ParseVector3(rawValue, Vector3.zero)
                    );

                case "string":
                    return Variant.FromString(rawValue);

                default:
                    Debug.LogWarning(
                        $"[TwinGraph] Unknown SetVar valueType '{valueType}'. Storing as string."
                    );
                    return Variant.FromString(rawValue);
            }
        }
    }
}
