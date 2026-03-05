using System;
using System.Collections.Generic;

namespace TwinGraph.Runtime.Graph
{
    [Serializable]
    public sealed class GraphData
    {
        public string version = "0.1.0";
        public List<NodeData> nodes = new List<NodeData>();
        public List<LinkData> links = new List<LinkData>();

        public void EnsureInitialized()
        {
            if (nodes == null)
            {
                nodes = new List<NodeData>();
            }

            if (links == null)
            {
                links = new List<LinkData>();
            }

            for (var i = 0; i < nodes.Count; i++)
            {
                nodes[i]?.EnsureInitialized();
            }
        }

        public static GraphData CreateSample()
        {
            const string startNodeId = "start";
            const string logNodeId = "log";

            return new GraphData
            {
                nodes = new List<NodeData>
                {
                    new NodeData { id = startNodeId, type = "Start" },
                    new NodeData
                    {
                        id = logNodeId,
                        type = "Log",
                        parameters = new List<NodeParamData>
                        {
                            new NodeParamData
                            {
                                key = "message",
                                value = "[TwinGraph] Sample graph data initialized.",
                            },
                        },
                    },
                },
                links = new List<LinkData>
                {
                    new LinkData
                    {
                        fromNodeId = startNodeId,
                        fromPort = "Next",
                        toNodeId = logNodeId,
                    },
                },
            };
        }
    }

    [Serializable]
    public sealed class NodeData
    {
        public string id = string.Empty;
        public string type = string.Empty;
        public List<NodeParamData> parameters = new List<NodeParamData>();

        public void EnsureInitialized()
        {
            if (parameters == null)
            {
                parameters = new List<NodeParamData>();
            }
        }

        public string GetParam(string key, string defaultValue = "")
        {
            if (parameters == null)
            {
                return defaultValue;
            }

            for (var i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                if (
                    parameter != null
                    && string.Equals(parameter.key, key, StringComparison.OrdinalIgnoreCase)
                )
                {
                    return parameter.value ?? defaultValue;
                }
            }

            return defaultValue;
        }
    }

    [Serializable]
    public sealed class NodeParamData
    {
        public string key = string.Empty;
        public string value = string.Empty;
    }

    [Serializable]
    public sealed class LinkData
    {
        public string fromNodeId = string.Empty;
        public string fromPort = "Next";
        public string toNodeId = string.Empty;
    }
}
