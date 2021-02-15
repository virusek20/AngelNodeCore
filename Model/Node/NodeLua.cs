using GalaSoft.MvvmLight;

namespace AngelNode.Model.Node
{
    public class NodeLua : ObservableObject, INode
    {
        private string _script;

        public string Script
        {
            get => _script;
            set { Set(() => Script, ref _script, value); }
        }

        public void Accept(INodeVisitor nodeVisitor)
        {
            nodeVisitor.VisitNodeLua(this);
        }
    }
}
