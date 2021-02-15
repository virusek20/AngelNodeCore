using System.Drawing;
using AngelNode.Model.Resource;
using GalaSoft.MvvmLight;

namespace AngelNode.Model
{
    public class Pose : ObservableObject
    {
        private string _name;
        private File _file;
        private bool _relative;

        public string Name
        {
            get => _name;
            set { Set(() => Name, ref _name, value); }
        }

        public File File
        {
            get => _file;
            set { Set(() => File, ref _file, value); }
        }

        public bool Relative
        {
            get => _relative;
            set => Set(() => Relative, ref _relative, value);
        }

        public (int width, int height) GetSpriteSize()
        {
            Image img = Image.FromFile(File.Path);
            return (img.Width, img.Height);
        }
    }
}
