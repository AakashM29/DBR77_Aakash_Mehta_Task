using UnityEngine;

namespace TwinGraph.Runtime.Graph
{
    [CreateAssetMenu(fileName = "GraphAsset", menuName = "TwinGraph/Graph Asset")]
    public sealed class GraphAsset : ScriptableObject
    {
        public GraphData graph = GraphData.CreateSample();
    }
}
