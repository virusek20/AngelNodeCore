using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;

namespace AngelNode.Model.Node
{
    public class NodeResponseDialogue : ObservableObject, INode
    {
        private Character _character;
        private string _dialogue;

        public Character Character
        {
            get => _character;
            set { Set(() => Character, ref _character, value); }
        }

        public string Dialogue
        {
            get => _dialogue;
            set { Set(() => Dialogue, ref _dialogue, value); }
        }

        public ObservableCollection<DialogueResponse> ResponseMap { get; } = new ObservableCollection<DialogueResponse>();
        public void Accept(INodeVisitor nodeVisitor)
        {
            nodeVisitor.VisitNodeResponseDialogue(this);
        }
    }
}
