namespace AngelNode.Model.Message
{
    public struct TabOpenMessage
    {
        public enum TabType
        {
            Scene,
            Character,
            Variable,
            Node
        }

        public TabType TabOpenType;
        public object Data;
    }
}
