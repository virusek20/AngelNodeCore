using GalaSoft.MvvmLight;

namespace AngelNode.Model.Node
{
    public class NodeAchievement : ObservableObject, INode
    {
        private string _name;

        public string Name
        {
            get => _name;
            set { Set(() => Name, ref _name, value); }
        }

        public void Accept(INodeVisitor nodeVisitor)
        {
            nodeVisitor.VisitNodeAchievement(this);
        }
    }
}
