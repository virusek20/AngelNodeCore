using GalaSoft.MvvmLight;

namespace AngelNode.Model.Node
{
    public class NodeDialogue : ObservableObject, INode
    {
        private Character _character;
        private string _text;

        public Character Character
        {
            get => _character;
            set { Set(() => Character, ref _character, value); }
        }

        public string Text
        {
            get => _text;
            set { Set(() => Text, ref _text, value); }
        }

        public void Accept(INodeVisitor nodeVisitor)
        {
            nodeVisitor.VisitNodeDialogue(this);
        }
    }
}
