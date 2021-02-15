using GalaSoft.MvvmLight;

namespace AngelNode.Model.Node
{
    public class NodeWait : ObservableObject, INode
    {
        private float _duration;

        public float Duration
        {
            get => _duration;
            set { Set(() => Duration, ref _duration, value); }
        }

        public void Accept(INodeVisitor nodeVisitor)
        {
            nodeVisitor.VisitNodeWait(this);
        }
    }
}
