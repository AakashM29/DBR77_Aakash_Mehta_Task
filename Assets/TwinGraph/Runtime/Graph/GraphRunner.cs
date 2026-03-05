using System;
using System.Collections.Generic;
using TwinGraph.Runtime.Nodes;
using UnityEngine;

namespace TwinGraph.Runtime.Graph
{
    public sealed class GraphRunner : MonoBehaviour
    {
        [SerializeField]
        private GraphAsset graphAsset;

        [SerializeField]
        private bool runOnStart = true;

        [SerializeField, Min(1)]
        private int maxSteps = 64;

        [SerializeField]
        private bool verboseLogging = true;

        private readonly Dictionary<string, NodeData> nodesById = new Dictionary<string, NodeData>(
            StringComparer.Ordinal
        );

        private readonly Dictionary<string, string> routingByPort = new Dictionary<string, string>(
            StringComparer.Ordinal
        );

        private void Awake()
        {
            NodeRegistry.EnsureDefaultsRegistered();
        }

        private void Start()
        {
            if (runOnStart)
            {
                RunGraph();
            }
        }

        [ContextMenu("Run Graph")]
        public void RunGraph()
        {
            var graphData = graphAsset != null ? graphAsset.graph : null;
            if (graphData == null)
            {
                Debug.LogWarning("[TwinGraph] GraphRunner has no GraphAsset assigned.", this);
                return;
            }

            graphData.EnsureInitialized();
            BuildRouting(graphData);
            var startNode = FindStartNode();

            if (startNode == null)
            {
                Debug.LogWarning("[TwinGraph] GraphRunner found no Start node.", this);
                return;
            }

            var context = new ExecutionContext(transform);
            var currentNode = startNode;

            for (var step = 0; step < maxSteps; step++)
            {
                var executedNode = currentNode;
                if (!NodeRegistry.TryGet(executedNode.type, out var executor))
                {
                    Debug.LogWarning(
                        $"[TwinGraph] No executor registered for node type '{executedNode.type}'.",
                        this
                    );
                    return;
                }

                if (verboseLogging)
                {
                    Debug.Log(
                        $"[TwinGraph] Step {step + 1}: {executedNode.type} ({executedNode.id})",
                        this
                    );
                }

                var result = executor.Execute(executedNode, context);
                if (result.ShouldStop)
                {
                    return;
                }

                var nextPort = string.IsNullOrWhiteSpace(result.NextPort)
                    ? "Next"
                    : result.NextPort;
                if (!TryGetNextNode(executedNode.id, nextPort, out currentNode))
                {
                    if (verboseLogging)
                    {
                        Debug.Log(
                            $"[TwinGraph] Execution finished at node '{executedNode.id}' on port '{nextPort}'.",
                            this
                        );
                    }

                    return;
                }
            }

            Debug.LogWarning(
                $"[TwinGraph] Reached maxSteps ({maxSteps}). Execution stopped safely.",
                this
            );
        }

        private void BuildRouting(GraphData graphData)
        {
            nodesById.Clear();
            routingByPort.Clear();

            if (graphData.nodes != null)
            {
                for (var i = 0; i < graphData.nodes.Count; i++)
                {
                    var node = graphData.nodes[i];
                    if (node == null || string.IsNullOrWhiteSpace(node.id))
                    {
                        continue;
                    }

                    nodesById[node.id] = node;
                }
            }

            if (graphData.links != null)
            {
                for (var i = 0; i < graphData.links.Count; i++)
                {
                    var link = graphData.links[i];
                    if (
                        link == null
                        || string.IsNullOrWhiteSpace(link.fromNodeId)
                        || string.IsNullOrWhiteSpace(link.toNodeId)
                    )
                    {
                        continue;
                    }

                    var fromPort = string.IsNullOrWhiteSpace(link.fromPort)
                        ? "Next"
                        : link.fromPort;
                    routingByPort[BuildRouteKey(link.fromNodeId, fromPort)] = link.toNodeId;
                }
            }

            if (verboseLogging)
            {
                Debug.Log(
                    $"[TwinGraph] Graph loaded with {nodesById.Count} node(s) and {routingByPort.Count} link(s).",
                    this
                );
            }
        }

        private NodeData FindStartNode()
        {
            foreach (var entry in nodesById)
            {
                var node = entry.Value;
                if (
                    node != null
                    && string.Equals(node.type, "Start", StringComparison.OrdinalIgnoreCase)
                )
                {
                    return node;
                }
            }

            return null;
        }

        private bool TryGetNextNode(string fromNodeId, string fromPort, out NodeData nextNode)
        {
            nextNode = null;
            if (!routingByPort.TryGetValue(BuildRouteKey(fromNodeId, fromPort), out var toNodeId))
            {
                return false;
            }

            return nodesById.TryGetValue(toNodeId, out nextNode);
        }

        private static string BuildRouteKey(string nodeId, string fromPort)
        {
            return nodeId + "::" + fromPort;
        }
    }
}
