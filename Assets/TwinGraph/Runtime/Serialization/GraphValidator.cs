using System;
using System.Collections.Generic;
using System.Globalization;
using TwinGraph.Runtime.Graph;
using TwinGraph.Runtime.Nodes;
using TwinGraph.Runtime.Utils;
using UnityEngine;

namespace TwinGraph.Runtime.Serialization
{
    public static class GraphValidator
    {
        private static readonly HashSet<string> AllowedPorts = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase
        )
        {
            "Next",
            "True",
            "False",
        };

        private static readonly HashSet<string> AllowedCompareOperators = new HashSet<string>(
            StringComparer.Ordinal
        )
        {
            "==",
            "!=",
            ">",
            ">=",
            "<",
            "<=",
        };

        private static readonly HashSet<string> AllowedPrimitiveShapes = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase
        )
        {
            "Cube",
            "Sphere",
            "Capsule",
            "Cylinder",
            "Plane",
            "Quad",
        };

        private static readonly HashSet<string> AllowedSetVarTypes = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase
        )
        {
            "int",
            "integer",
            "float",
            "single",
            "number",
            "bool",
            "boolean",
            "vector3",
            "vec3",
            "string",
        };

        public static bool Validate(GraphData graph, out List<string> errors)
        {
            errors = new List<string>();

            if (graph == null)
            {
                errors.Add("GraphData is null.");
                return false;
            }

            graph.EnsureInitialized();
            NodeRegistry.EnsureDefaultsRegistered();

            var nodeIds = new HashSet<string>(StringComparer.Ordinal);
            var nodesById = new Dictionary<string, NodeData>(StringComparer.Ordinal);
            var startCount = 0;

            for (var i = 0; i < graph.nodes.Count; i++)
            {
                var node = graph.nodes[i];
                if (node == null)
                {
                    errors.Add($"Node at index {i} is null.");
                    continue;
                }

                var id = node.id?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(id))
                {
                    errors.Add($"Node at index {i} has an empty id.");
                }
                else if (!nodeIds.Add(id))
                {
                    errors.Add($"Duplicate node id '{id}'.");
                }
                else
                {
                    nodesById[id] = node;
                }

                if (string.IsNullOrWhiteSpace(node.type))
                {
                    errors.Add($"Node '{id}' has an empty type.");
                }
                else
                {
                    if (string.Equals(node.type, "Start", StringComparison.OrdinalIgnoreCase))
                    {
                        startCount++;
                    }

                    if (!NodeRegistry.TryGet(node.type, out _))
                    {
                        errors.Add($"Node '{id}' has unknown type '{node.type}'.");
                    }
                }

                ValidateRequiredParameters(node, id, errors);
            }

            if (startCount == 0)
            {
                errors.Add("Graph is missing a Start node.");
            }
            else if (startCount > 1)
            {
                errors.Add($"Graph has {startCount} Start nodes. Exactly one Start node is required.");
            }

            var usedRoutes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var outgoingPortsByNode = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

            for (var i = 0; i < graph.links.Count; i++)
            {
                var link = graph.links[i];
                if (link == null)
                {
                    errors.Add($"Link at index {i} is null.");
                    continue;
                }

                var fromNodeId = link.fromNodeId?.Trim() ?? string.Empty;
                var toNodeId = link.toNodeId?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(fromNodeId))
                {
                    errors.Add($"Link at index {i} has an empty fromNodeId.");
                }
                else if (!nodeIds.Contains(fromNodeId))
                {
                    errors.Add($"Link at index {i} references missing fromNodeId '{fromNodeId}'.");
                }

                if (string.IsNullOrWhiteSpace(toNodeId))
                {
                    errors.Add($"Link at index {i} has an empty toNodeId.");
                }
                else if (!nodeIds.Contains(toNodeId))
                {
                    errors.Add($"Link at index {i} references missing toNodeId '{toNodeId}'.");
                }

                var normalizedPort = NormalizePort(link.fromPort);
                if (!AllowedPorts.Contains(normalizedPort))
                {
                    errors.Add(
                        $"Link at index {i} uses unsupported fromPort '{link.fromPort}'. Allowed: Next, True, False."
                    );
                }

                if (!string.IsNullOrWhiteSpace(fromNodeId))
                {
                    if (!outgoingPortsByNode.TryGetValue(fromNodeId, out var ports))
                    {
                        ports = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        outgoingPortsByNode[fromNodeId] = ports;
                    }

                    ports.Add(normalizedPort);

                    var routeKey = BuildRouteKey(fromNodeId, normalizedPort);
                    if (!usedRoutes.Add(routeKey))
                    {
                        errors.Add(
                            $"Duplicate route '{fromNodeId}:{normalizedPort}' detected. Each node/port must route to exactly one target."
                        );
                    }
                }

                if (nodesById.TryGetValue(fromNodeId, out var fromNode))
                {
                    ValidatePortCompatibility(fromNode, fromNodeId, normalizedPort, i, errors);
                }
            }

            ValidateNodeOutgoingRequirements(nodesById, outgoingPortsByNode, errors);

            return errors.Count == 0;
        }

        private static void ValidateRequiredParameters(NodeData node, string nodeId, List<string> errors)
        {
            var safeNodeId = string.IsNullOrWhiteSpace(nodeId) ? "<missing-id>" : nodeId;
            var type = node.type?.Trim() ?? string.Empty;

            switch (type.ToLowerInvariant())
            {
                case "createprimitive":
                    RequireNonEmptyParam(node, safeNodeId, "objectKey", errors);
                    ValidateCreatePrimitiveParameters(node, safeNodeId, errors);
                    break;
                case "settransform":
                    RequireNonEmptyParam(node, safeNodeId, "objectKey", errors);
                    ValidateSetTransformParameters(node, safeNodeId, errors);
                    break;
                case "setvar":
                    RequireNonEmptyParam(node, safeNodeId, "varName", errors);
                    ValidateSetVarParameters(node, safeNodeId, errors);
                    break;
                case "add":
                    RequireNonEmptyParam(node, safeNodeId, "outVar", errors);
                    ValidateAddParameters(node, safeNodeId, errors);
                    break;
                case "compare":
                    RequireNonEmptyParam(node, safeNodeId, "left", errors);
                    RequireNonEmptyParam(node, safeNodeId, "right", errors);
                    ValidateCompareParameters(node, safeNodeId, errors);
                    break;
                case "delay":
                    ValidateDelayParameters(node, safeNodeId, errors);
                    break;
                case "log":
                    RequireNonEmptyParam(node, safeNodeId, "message", errors);
                    break;
            }
        }

        private static void ValidateCompareParameters(NodeData node, string nodeId, List<string> errors)
        {
            var op = node.GetParam("op", "==").Trim();
            if (!AllowedCompareOperators.Contains(op))
            {
                errors.Add(
                    $"Node '{nodeId}' has unsupported Compare op '{op}'. Allowed: ==, !=, >, >=, <, <=."
                );
            }

            var left = node.GetParam("left", string.Empty);
            var right = node.GetParam("right", string.Empty);

            if (!IsValidOperandReference(left))
            {
                errors.Add(
                    $"Node '{nodeId}' has invalid Compare left operand '{left}'. Use a literal or '$variableName'."
                );
            }

            if (!IsValidOperandReference(right))
            {
                errors.Add(
                    $"Node '{nodeId}' has invalid Compare right operand '{right}'. Use a literal or '$variableName'."
                );
            }

            if (IsRelationalOperator(op))
            {
                if (!IsValidNumericOperand(left))
                {
                    errors.Add(
                        $"Node '{nodeId}' uses relational op '{op}' but left operand '{left}' is not numeric or variable reference."
                    );
                }

                if (!IsValidNumericOperand(right))
                {
                    errors.Add(
                        $"Node '{nodeId}' uses relational op '{op}' but right operand '{right}' is not numeric or variable reference."
                    );
                }
            }
        }

        private static void ValidateCreatePrimitiveParameters(
            NodeData node,
            string nodeId,
            List<string> errors
        )
        {
            RequireNonEmptyParam(node, nodeId, "position", errors);
            RequireNonEmptyParam(node, nodeId, "rotation", errors);
            RequireNonEmptyParam(node, nodeId, "scale", errors);

            var shape = node.GetParam("shape", node.GetParam("primitiveType", string.Empty)).Trim();
            if (string.IsNullOrWhiteSpace(shape))
            {
                errors.Add(
                    $"Node '{nodeId}' (CreatePrimitive) requires non-empty parameter 'shape' or 'primitiveType'."
                );
                return;
            }

            if (!AllowedPrimitiveShapes.Contains(shape))
            {
                errors.Add(
                    $"Node '{nodeId}' has unsupported shape '{shape}'. Allowed: Cube, Sphere, Capsule, Cylinder, Plane, Quad."
                );
            }

            ValidateVector3Param(node, nodeId, "position", errors);
            ValidateVector3Param(node, nodeId, "rotation", errors);
            ValidateVector3Param(node, nodeId, "scale", errors);
        }

        private static void ValidateSetTransformParameters(
            NodeData node,
            string nodeId,
            List<string> errors
        )
        {
            RequireNonEmptyParam(node, nodeId, "applyPosition", errors);
            RequireNonEmptyParam(node, nodeId, "applyRotation", errors);
            RequireNonEmptyParam(node, nodeId, "applyScale", errors);

            ValidateStrictBoolParam(node, nodeId, "applyPosition", errors);
            ValidateStrictBoolParam(node, nodeId, "applyRotation", errors);
            ValidateStrictBoolParam(node, nodeId, "applyScale", errors);

            var applyPosition = NodeValueParsers.ParseBool(node.GetParam("applyPosition", "true"), true);
            var applyRotation = NodeValueParsers.ParseBool(node.GetParam("applyRotation", "true"), true);
            var applyScale = NodeValueParsers.ParseBool(node.GetParam("applyScale", "true"), true);

            if (applyPosition)
            {
                RequireNonEmptyParam(node, nodeId, "position", errors);
                ValidateVector3Param(node, nodeId, "position", errors);
            }

            if (applyRotation)
            {
                RequireNonEmptyParam(node, nodeId, "rotation", errors);
                ValidateVector3Param(node, nodeId, "rotation", errors);
            }

            if (applyScale)
            {
                RequireNonEmptyParam(node, nodeId, "scale", errors);
                ValidateVector3Param(node, nodeId, "scale", errors);
            }
        }

        private static void ValidateSetVarParameters(NodeData node, string nodeId, List<string> errors)
        {
            RequireNonEmptyParam(node, nodeId, "valueType", errors);
            RequireNonEmptyParam(node, nodeId, "value", errors);

            var valueType = node.GetParam("valueType", string.Empty).Trim();
            if (!AllowedSetVarTypes.Contains(valueType))
            {
                errors.Add(
                    $"Node '{nodeId}' has unsupported SetVar valueType '{valueType}'."
                );
                return;
            }

            var rawValue = node.GetParam("value", string.Empty);
            switch (valueType.ToLowerInvariant())
            {
                case "int":
                case "integer":
                    if (!int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                    {
                        errors.Add($"Node '{nodeId}' SetVar int value '{rawValue}' is invalid.");
                    }

                    break;
                case "float":
                case "single":
                case "number":
                    if (!double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                    {
                        errors.Add($"Node '{nodeId}' SetVar float value '{rawValue}' is invalid.");
                    }

                    break;
                case "bool":
                case "boolean":
                    if (!TryParseStrictBool(rawValue))
                    {
                        errors.Add($"Node '{nodeId}' SetVar bool value '{rawValue}' is invalid.");
                    }

                    break;
                case "vector3":
                case "vec3":
                    if (!TryParseStrictVector3(rawValue))
                    {
                        errors.Add($"Node '{nodeId}' SetVar vector3 value '{rawValue}' is invalid.");
                    }

                    break;
            }
        }

        private static void ValidateAddParameters(NodeData node, string nodeId, List<string> errors)
        {
            RequireNonEmptyParam(node, nodeId, "a", errors);
            RequireNonEmptyParam(node, nodeId, "b", errors);

            var a = node.GetParam("a", string.Empty);
            var b = node.GetParam("b", string.Empty);

            if (!IsValidNumericOperand(a))
            {
                errors.Add(
                    $"Node '{nodeId}' Add operand 'a' has invalid value '{a}'. Use numeric literal or '$variableName'."
                );
            }

            if (!IsValidNumericOperand(b))
            {
                errors.Add(
                    $"Node '{nodeId}' Add operand 'b' has invalid value '{b}'. Use numeric literal or '$variableName'."
                );
            }
        }

        private static void ValidateDelayParameters(NodeData node, string nodeId, List<string> errors)
        {
            RequireNonEmptyParam(node, nodeId, "seconds", errors);
            var raw = node.GetParam("seconds", string.Empty);

            if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds))
            {
                errors.Add($"Node '{nodeId}' Delay seconds '{raw}' is invalid.");
                return;
            }

            if (seconds < 0d)
            {
                errors.Add($"Node '{nodeId}' Delay seconds must be >= 0, found '{raw}'.");
            }
        }

        private static void RequireNonEmptyParam(
            NodeData node,
            string nodeId,
            string key,
            List<string> errors
        )
        {
            if (HasNonEmptyParam(node, key))
            {
                return;
            }

            errors.Add($"Node '{nodeId}' ({node.type}) requires non-empty parameter '{key}'.");
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
                    && string.Equals(parameter.key, key, StringComparison.OrdinalIgnoreCase)
                )
                {
                    return !string.IsNullOrWhiteSpace(parameter.value);
                }
            }

            return false;
        }

        private static void ValidateVector3Param(
            NodeData node,
            string nodeId,
            string key,
            List<string> errors
        )
        {
            var raw = node.GetParam(key, string.Empty);
            if (!TryParseStrictVector3(raw))
            {
                errors.Add($"Node '{nodeId}' parameter '{key}' must be a valid Vector3 (x,y,z).");
            }
        }

        private static void ValidateStrictBoolParam(
            NodeData node,
            string nodeId,
            string key,
            List<string> errors
        )
        {
            var raw = node.GetParam(key, string.Empty);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return;
            }

            if (!TryParseStrictBool(raw))
            {
                errors.Add(
                    $"Node '{nodeId}' parameter '{key}' must be a valid bool token (true/false/1/0/yes/no/on/off)."
                );
            }
        }

        private static string NormalizePort(string rawPort)
        {
            return string.IsNullOrWhiteSpace(rawPort) ? "Next" : rawPort.Trim();
        }

        private static void ValidatePortCompatibility(
            NodeData fromNode,
            string fromNodeId,
            string fromPort,
            int linkIndex,
            List<string> errors
        )
        {
            var isCompareNode = string.Equals(
                fromNode.type,
                "Compare",
                StringComparison.OrdinalIgnoreCase
            );

            if (isCompareNode)
            {
                if (
                    !string.Equals(fromPort, "True", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(fromPort, "False", StringComparison.OrdinalIgnoreCase)
                )
                {
                    errors.Add(
                        $"Link at index {linkIndex} from Compare node '{fromNodeId}' must use 'True' or 'False' port, found '{fromPort}'."
                    );
                }

                return;
            }

            if (!string.Equals(fromPort, "Next", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(
                    $"Link at index {linkIndex} from node '{fromNodeId}' ({fromNode.type}) must use 'Next' port, found '{fromPort}'."
                );
            }
        }

        private static void ValidateNodeOutgoingRequirements(
            Dictionary<string, NodeData> nodesById,
            Dictionary<string, HashSet<string>> outgoingPortsByNode,
            List<string> errors
        )
        {
            foreach (var entry in nodesById)
            {
                var nodeId = entry.Key;
                var node = entry.Value;
                if (node == null)
                {
                    continue;
                }

                if (
                    string.Equals(node.type, "Compare", StringComparison.OrdinalIgnoreCase)
                )
                {
                    if (!outgoingPortsByNode.TryGetValue(nodeId, out var ports))
                    {
                        errors.Add($"Compare node '{nodeId}' is missing outgoing 'True' link.");
                        errors.Add($"Compare node '{nodeId}' is missing outgoing 'False' link.");
                        continue;
                    }

                    if (!ports.Contains("True"))
                    {
                        errors.Add($"Compare node '{nodeId}' is missing outgoing 'True' link.");
                    }

                    if (!ports.Contains("False"))
                    {
                        errors.Add($"Compare node '{nodeId}' is missing outgoing 'False' link.");
                    }
                }
            }
        }

        private static string BuildRouteKey(string fromNodeId, string fromPort)
        {
            return fromNodeId + "::" + fromPort;
        }

        private static bool IsRelationalOperator(string op)
        {
            return string.Equals(op, ">", StringComparison.Ordinal)
                || string.Equals(op, ">=", StringComparison.Ordinal)
                || string.Equals(op, "<", StringComparison.Ordinal)
                || string.Equals(op, "<=", StringComparison.Ordinal);
        }

        private static bool IsValidNumericOperand(string raw)
        {
            return IsVariableReference(raw)
                || double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out _);
        }

        private static bool IsValidOperandReference(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            if (raw[0] != '$')
            {
                return true;
            }

            return raw.Length > 1 && !string.IsNullOrWhiteSpace(raw.Substring(1));
        }

        private static bool IsVariableReference(string raw)
        {
            return raw != null && raw.StartsWith("$", StringComparison.Ordinal) && raw.Length > 1;
        }

        private static bool TryParseStrictVector3(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            var parsed = NodeValueParsers.ParseVector3(raw, Vector3.positiveInfinity);
            return !float.IsPositiveInfinity(parsed.x);
        }

        private static bool TryParseStrictBool(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            if (bool.TryParse(raw, out _))
            {
                return true;
            }

            switch (raw.Trim().ToLowerInvariant())
            {
                case "1":
                case "0":
                case "y":
                case "n":
                case "yes":
                case "no":
                case "on":
                case "off":
                    return true;
                default:
                    return false;
            }
        }
    }
}
