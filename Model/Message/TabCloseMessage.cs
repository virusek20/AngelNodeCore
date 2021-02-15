namespace AngelNode.Model.Message
{
    public struct TabCloseMessage
    {
        public enum TabType
        {
            Scene,
            Character,
            Variable,
            All
        }

        public TabType TabCloseType;
        public object Data;
    }
}
