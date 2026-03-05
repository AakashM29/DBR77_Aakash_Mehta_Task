namespace TwinGraph.Runtime.Graph
{
    public readonly struct NodeResult
    {
        public NodeResult(string nextPort, bool shouldStop = false, float waitSeconds = 0f)
        {
            NextPort = nextPort;
            ShouldStop = shouldStop;
            WaitSeconds = waitSeconds;
        }

        public string NextPort { get; }
        public bool ShouldStop { get; }
        public float WaitSeconds { get; }

        public static NodeResult Next(string port = "Next")
        {
            return new NodeResult(port, false, 0f);
        }

        public static NodeResult Wait(float seconds, string port = "Next")
        {
            return new NodeResult(port, false, seconds < 0f ? 0f : seconds);
        }

        public static NodeResult Stop()
        {
            return new NodeResult(string.Empty, true, 0f);
        }
    }
}
