namespace TwinGraph.Runtime.Graph
{
    public readonly struct NodeResult
    {
        public NodeResult(string nextPort, bool shouldStop = false)
        {
            NextPort = nextPort;
            ShouldStop = shouldStop;
        }

        public string NextPort { get; }
        public bool ShouldStop { get; }

        public static NodeResult Next(string port = "Next")
        {
            return new NodeResult(port, false);
        }

        public static NodeResult Stop()
        {
            return new NodeResult(string.Empty, true);
        }
    }
}
