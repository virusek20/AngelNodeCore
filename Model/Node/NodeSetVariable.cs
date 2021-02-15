using GalaSoft.MvvmLight;

namespace AngelNode.Model.Node
{
    public class NodeSetVariable : ObservableObject, INode
    {
        private Variable _variable;
        private int _value;
        private SetType _type;

        public enum SetType
        {
            Add,
            Set
        }

        public Variable Variable
        {
            get => _variable;
            set { Set(() => Variable, ref _variable, value); }
        }

        public int Value
        {
            get => _value;
            set { Set(() => Value, ref _value, value); }
        }

        public SetType Type
        {
            get => _type;
            set { Set(() => Type, ref _type, value); }
        }

        public void Accept(INodeVisitor nodeVisitor)
        {
            nodeVisitor.VisitNodeSetVariable(this);
        }
    }
}
