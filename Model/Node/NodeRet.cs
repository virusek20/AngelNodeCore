using GalaSoft.MvvmLight;

namespace AngelNode.Model.Node
{
    public class NodeRet : ObservableObject, INode
    {
        public void Accept(INodeVisitor nodeVisitor)
        {
            nodeVisitor.VisitNodeRet(this);
        }
    }
}
