using System.IO;
using TwinGraph.Runtime.Graph;

namespace TwinGraph.Runtime.Serialization
{
    public static class GraphSerializer
    {
        public static string ToJson(GraphData graph, bool prettyPrint = true)
        {
            var safeGraph = graph ?? new GraphData();
            safeGraph.EnsureInitialized();
            return UnityEngine.JsonUtility.ToJson(safeGraph, prettyPrint);
        }

        public static GraphData FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            var graph = UnityEngine.JsonUtility.FromJson<GraphData>(json);
            graph?.EnsureInitialized();
            return graph;
        }

        public static bool Save(GraphData graph, string path, out string error)
        {
            error = string.Empty;
            if (string.IsNullOrWhiteSpace(path))
            {
                error = "Path is empty.";
                return false;
            }

            try
            {
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(path, ToJson(graph, true));
                return true;
            }
            catch (System.Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public static bool Load(string path, out GraphData graph, out string error)
        {
            graph = null;
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(path))
            {
                error = "Path is empty.";
                return false;
            }

            if (!File.Exists(path))
            {
                error = $"File does not exist: {path}";
                return false;
            }

            try
            {
                var json = File.ReadAllText(path);
                graph = FromJson(json);
                if (graph == null)
                {
                    error = "Failed to deserialize GraphData from JSON.";
                    return false;
                }

                return true;
            }
            catch (System.Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
