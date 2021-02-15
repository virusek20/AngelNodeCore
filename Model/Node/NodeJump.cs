using GalaSoft.MvvmLight;

namespace AngelNode.Model.Node
{
    public class NodeJump : ObservableObject, INode
    {
        private INode _target;

        public INode Target
        {
            get => _target;
            set { Set(() => Target, ref _target, value); }
        }

        public void Accept(INodeVisitor nodeVisitor)
        {
            nodeVisitor.VisitNodeJump(this);
        }
    }
}
