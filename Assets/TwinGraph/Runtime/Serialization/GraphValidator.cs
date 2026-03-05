using System;
using System.Collections.Generic;
using TwinGraph.Runtime.Graph;
using TwinGraph.Runtime.Nodes;

namespace TwinGraph.Runtime.Serialization
{
    public static class GraphValidator
    {
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
            var hasStart = false;

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

                if (string.IsNullOrWhiteSpace(node.type))
                {
                    errors.Add($"Node '{id}' has an empty type.");
                }
                else
                {
                    if (string.Equals(node.type, "Start", StringComparison.OrdinalIgnoreCase))
                    {
                        hasStart = true;
                    }

                    if (!NodeRegistry.TryGet(node.type, out _))
                    {
                        errors.Add($"Node '{id}' has unknown type '{node.type}'.");
                    }
                }
            }

            if (!hasStart)
            {
                errors.Add("Graph is missing a Start node.");
            }

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
            }

            return errors.Count == 0;
        }
    }
}
