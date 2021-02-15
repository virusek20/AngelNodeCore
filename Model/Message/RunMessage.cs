namespace AngelNode.Model.Message
{
    public struct RunMessage
    {
        public enum RunType
        {
            Scene,
            Node
        }

        public RunType RunMessageType;
        public object Data;
    }
}
