using AngelNode.Model.Resource;
using GalaSoft.MvvmLight;

namespace AngelNode.Model
{
    public class Outfit : ObservableObject
    {
        private Directory _directory;
        private string _name;

        public Directory Directory
        {
            get => _directory;
            set => Set(() => Directory, ref _directory, value);
        }

        public string Name
        {
            get => _name;
            set => Set(() => Name, ref _name, value);
        }
    }
}
