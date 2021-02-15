using AngelNode.Model.Node;
using System.Collections.Generic;

namespace AngelNode.Service.Implementations.DOT
{
    public class GraphNode
    {
        public string Text;
        public bool Critical;
        public string Style;
        public INode Node;

        public Dictionary<GraphNode, string> NextNodes = new Dictionary<GraphNode, string>();
        public HashSet<GraphNode> PreviousNodes = new HashSet<GraphNode>();
    }
}
