using GalaSoft.MvvmLight;

namespace AngelNode.Model.Node
{
    public class DialogueResponse : ObservableObject
    {
        private string _text;
        private INode _target;

        public string Text
        {
            get => _text;
            set { Set(() => Text, ref _text, value); }
        }

        public INode Target
        {
            get => _target;
            set { Set(() => Target, ref _target, value); }
        }
    }
}
