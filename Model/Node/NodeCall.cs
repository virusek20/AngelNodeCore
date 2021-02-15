using GalaSoft.MvvmLight;

namespace AngelNode.Model.Node
{
    public class NodeCall : ObservableObject, INode
    {
        private Scene _target;

        public Scene Target
        {
            get => _target;
            set => Set(() => Target, ref _target, value);
        }

        public void Accept(INodeVisitor nodeVisitor)
        {
            nodeVisitor.VisitNodeCall(this);
        }
    }
}
