using GalaSoft.MvvmLight;

namespace AngelNode.Model.Node
{
    public class NodeTodo : ObservableObject, INode
    {
        private string _note;

        public string Note
        {
            get => _note;
            set { Set(() => Note, ref _note, value); }
        }

        public void Accept(INodeVisitor nodeVisitor)
        {
            nodeVisitor.VisitNodeTodo(this);
        }
    }
}
