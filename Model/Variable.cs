using GalaSoft.MvvmLight;

namespace AngelNode.Model
{
    public class Variable : ObservableObject
    {
        private string _name;
        private bool _highlightChanges;
        private bool _binary;

        public string Name
        {
            get => _name;
            set { Set(() => Name, ref _name, value); }
        }

        public bool HighlightChanges
        {
            get => _highlightChanges;
            set { Set(() => HighlightChanges, ref _highlightChanges, value); }
        }

        public bool Binary
        {
            get => _binary;
            set { Set(() => Binary, ref _binary, value); }
        }
    }
}
