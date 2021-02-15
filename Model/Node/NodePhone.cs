using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;

namespace AngelNode.Model.Node
{
    public class NodePhone : ObservableObject, INode
    {
        public class PhoneMessage : ObservableObject
        {
            private bool _playerMade;
            private bool _skip;
            private string _text;

            public bool PlayerMade
            {
                get => _playerMade;
                set { Set(() => PlayerMade, ref _playerMade, value); }
            }

            public bool Skip
            {
                get => _skip;
                set { Set(() => Skip, ref _skip, value); }
            }
            public string Text
            {
                get => _text;
                set { Set(() => Text, ref _text, value); }
            }
        }

        private Character _character;
        private string _time;

        public Character Character
        {
            get => _character;
            set { Set(() => Character, ref _character, value); }
        }

        public string Time
        {
            get => _time;
            set { Set(() => Time, ref _time, value); }
        }

        public ObservableCollection<PhoneMessage> PhoneMessages { get; } = new ObservableCollection<PhoneMessage>();

        public void Accept(INodeVisitor nodeVisitor)
        {
            nodeVisitor.VisitNodePhone(this);
        }
    }
}
