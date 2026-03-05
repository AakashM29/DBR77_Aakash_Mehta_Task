using System.Globalization;
using TwinGraph.Runtime.Graph;
using UnityEngine;

namespace TwinGraph.Runtime.Nodes
{
    public sealed class AddNodeExecutor : INodeExecutor
    {
        public string NodeType => "Add";

        public NodeResult Execute(NodeData node, ExecutionContext context)
        {
            var outVar = node.GetParam("outVar", string.Empty);
            if (string.IsNullOrWhiteSpace(outVar))
            {
                Debug.LogWarning("[TwinGraph] Add requires an outVar parameter.");
                return NodeResult.Next("Next");
            }

            var aRaw = node.GetParam("a", "0");
            var bRaw = node.GetParam("b", "0");

            var a = ResolveNumber(aRaw, context);
            var b = ResolveNumber(bRaw, context);
            var sum = a + b;

            context.SetVar(outVar, Variant.FromFloat((float)sum));
            return NodeResult.Next("Next");
        }

        private static double ResolveNumber(string raw, ExecutionContext context)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return 0d;
            }

            if (raw[0] == '$')
            {
                var varName = raw.Substring(1);
                if (!context.TryGetVar(varName, out var variant))
                {
                    Debug.LogWarning(
                        $"[TwinGraph] Add could not find variable '{varName}'. Using 0."
                    );
                    return 0d;
                }

                switch (variant.Type)
                {
                    case Variant.VariantType.Int:
                        return variant.AsInt();
                    case Variant.VariantType.Float:
                        return variant.AsFloat();
                    default:
                        Debug.LogWarning(
                            $"[TwinGraph] Add variable '{varName}' is not numeric ({variant.Type}). Using 0."
                        );
                        return 0d;
                }
            }

            if (
                double.TryParse(
                    raw,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var literal
                )
            )
            {
                return literal;
            }

            Debug.LogWarning($"[TwinGraph] Add could not parse numeric literal '{raw}'. Using 0.");
            return 0d;
        }
    }
}
