using System;
using System.Globalization;
using TwinGraph.Runtime.Graph;
using TwinGraph.Runtime.Utils;
using UnityEngine;

namespace TwinGraph.Runtime.Nodes
{
    public sealed class CompareNodeExecutor : INodeExecutor
    {
        public string NodeType => "Compare";

        public NodeResult Execute(NodeData node, ExecutionContext context)
        {
            var leftRaw = node.GetParam("left", string.Empty);
            var op = node.GetParam("op", "==");
            var rightRaw = node.GetParam("right", string.Empty);

            var left = ResolveOperand(leftRaw, context);
            var right = ResolveOperand(rightRaw, context);
            var result = EvaluateComparison(left, op, right);

            return NodeResult.Next(result ? "True" : "False");
        }

        private static Variant ResolveOperand(string raw, ExecutionContext context)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return Variant.FromString(string.Empty);
            }

            if (raw[0] == '$')
            {
                var varName = raw.Substring(1);
                if (context.TryGetVar(varName, out var variant))
                {
                    return variant;
                }

                Debug.LogWarning($"[TwinGraph] Compare could not find variable '{varName}'.");
                return Variant.FromString(string.Empty);
            }

            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
            {
                return Variant.FromInt(i);
            }

            if (float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
            {
                return Variant.FromFloat(f);
            }

            if (bool.TryParse(raw, out var b))
            {
                return Variant.FromBool(b);
            }

            var vec = NodeValueParsers.ParseVector3(raw, Vector3.positiveInfinity);
            if (!float.IsPositiveInfinity(vec.x))
            {
                return Variant.FromVector3(vec);
            }

            return Variant.FromString(raw);
        }

        private static bool EvaluateComparison(Variant left, string op, Variant right)
        {
            var normalized = string.IsNullOrWhiteSpace(op) ? "==" : op.Trim();
            switch (normalized)
            {
                case "==":
                    return EqualsByType(left, right);
                case "!=":
                    return !EqualsByType(left, right);
                case ">":
                case ">=":
                case "<":
                case "<=":
                    if (TryNumeric(left, out var l) && TryNumeric(right, out var r))
                    {
                        switch (normalized)
                        {
                            case ">":
                                return l > r;
                            case ">=":
                                return l >= r;
                            case "<":
                                return l < r;
                            case "<=":
                                return l <= r;
                        }
                    }

                    Debug.LogWarning(
                        $"[TwinGraph] Compare operator '{normalized}' requires numeric operands."
                    );
                    return false;
                default:
                    Debug.LogWarning($"[TwinGraph] Unsupported Compare operator '{normalized}'.");
                    return false;
            }
        }

        private static bool EqualsByType(Variant left, Variant right)
        {
            if (TryNumeric(left, out var l) && TryNumeric(right, out var r))
            {
                return Math.Abs(l - r) <= 0.00001d;
            }

            if (left.Type == Variant.VariantType.Bool && right.Type == Variant.VariantType.Bool)
            {
                return left.AsBool() == right.AsBool();
            }

            if (
                left.Type == Variant.VariantType.Vector3
                && right.Type == Variant.VariantType.Vector3
            )
            {
                return left.AsVector3() == right.AsVector3();
            }

            return string.Equals(left.ToString(), right.ToString(), StringComparison.Ordinal);
        }

        private static bool TryNumeric(Variant value, out double numeric)
        {
            switch (value.Type)
            {
                case Variant.VariantType.Int:
                    numeric = value.AsInt();
                    return true;
                case Variant.VariantType.Float:
                    numeric = value.AsFloat();
                    return true;
                default:
                    numeric = 0d;
                    return false;
            }
        }
    }
}
