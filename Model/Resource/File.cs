using System.IO;
using GalaSoft.MvvmLight;

namespace AngelNode.Model.Resource
{
    public class File : ObservableObject, IResource
    {
        private string _path;

        public string Path
        {
            get => _path;
            set
            {
                Set(() => Path, ref _path, value);
                RaisePropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// Gets the full name of this file (fileName + extension)
        /// Ex. myFile.png
        /// </summary>
        public string Name => System.IO.Path.GetFileName(_path);

        public ResourceType FileType => GuessType();

        public ResourceType GuessType()
        {
            string extension = System.IO.Path.GetExtension(Path);

            switch (extension)
            {
                case ".jpg":
                case ".png":
                    return ResourceType.Image;
                case ".wav":
                case ".mp3":
                case ".ogg":
                    return ResourceType.Sound;
                default:
                    return ResourceType.Unknown;
            }
        }

        public File(FileInfo file)
        {
            _path = file.FullName;
        }

        public File(string filePath)
        {
            if (!System.IO.File.Exists(filePath)) throw new FileNotFoundException("Referenced resource could not be found.", filePath);
            _path = filePath;
        }

        public override bool Equals(object obj)
        {
            if (obj is File f) return f.Path == Path;
            return false;
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }
    }
}
