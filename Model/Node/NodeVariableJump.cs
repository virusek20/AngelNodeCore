using GalaSoft.MvvmLight;

namespace AngelNode.Model.Node
{
    public class NodeVariableJump : ObservableObject, INode
    {
        private Variable _variable;
        private int _value;
        private ComparisonType _type;
        private INode _target;

        public enum ComparisonType
        {
            Equal,
            GreaterThan,
            LessThan
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

        public INode Target
        {
            get => _target;
            set { Set(() => Target, ref _target, value); }
        }

        public ComparisonType Type
        {
            get => _type;
            set { Set(() => Type, ref _type, value); }
        }

        public void Accept(INodeVisitor nodeVisitor)
        {
            nodeVisitor.VisitNodeVariableJump(this);
        }
    }
}
