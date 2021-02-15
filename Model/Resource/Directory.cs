using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using GalaSoft.MvvmLight;

namespace AngelNode.Model.Resource
{
    public class Directory : ObservableObject, IResource
    {
        private ObservableCollection<IResource> _files;
        private string _path;

        public ObservableCollection<IResource> Files
        {
            get => _files;
            set { Set(() => Files, ref _files, value); }
        }

        public string Path
        {
            get => _path;
            set
            {
                Set(() => Path, ref _path, value);
                RaisePropertyChanged(nameof(Name));
            }
        }

        public string Name => System.IO.Path.GetFileName(Path);

        public Directory(DirectoryInfo dir)
        {
            _path = dir.FullName;
            var files = dir.EnumerateFileSystemInfos().Select<FileSystemInfo, IResource>(info =>
            {
                if ((info.Attributes & FileAttributes.Directory) != 0) return new Directory(new DirectoryInfo(info.FullName));
                return new File(new FileInfo(info.FullName));
            });

            _files = new ObservableCollection<IResource>(files);
        }

        public bool RecursiveRename(string oldPath, string newPath)
        {
            Directory removedDirectory = null;

            foreach (var file in Files)
            {
                if (file.Path == oldPath)
                {
                    if (file is Directory directory)
                    {
                        removedDirectory = directory;
                        break;
                    }

                    file.Path = newPath;
                    return true;
                }

                if (file is Directory dir)
                {
                    bool subFolderFound = dir.RecursiveRename(oldPath, newPath);
                    if (subFolderFound) return true;
                }
            }

            if (removedDirectory != null)
            {
                Files.Remove(removedDirectory);
                Files.Add(new Directory(new DirectoryInfo(newPath)));
                return true;
            }

            return false;
        }

        public bool RecursiveAdd(string path)
        {
            string parentDir = System.IO.Path.GetDirectoryName(path);

            if (parentDir == _path)
            {
                if (System.IO.Directory.Exists(path)) Files.Add(new Directory(new DirectoryInfo(path)));
                else Files.Add(new File(new FileInfo(path)));
                RecursiveSort(false);
                return true;
            }

            foreach (var file in Files)
            {
                if (file.Path == parentDir)
                {
                    var dire = (Directory) file;
                    if (System.IO.Directory.Exists(path)) dire.Files.Add(new Directory(new DirectoryInfo(path)));
                    else dire.Files.Add(new File(new FileInfo(path)));
                    dire.RecursiveSort(false);
                    return true;
                }

                if (file is Directory dir)
                {
                    bool subFolderFound = dir.RecursiveAdd(path);
                    if (subFolderFound) return true;
                }
            }

            return false;
        }

        public void RecursiveSort(bool recurse)
        {
            var sortableList = new List<IResource>(Files);
            sortableList.OrderBy(r => System.IO.Path.GetFileName(r.Path));

            for (int i = 0; i < sortableList.Count; i++)
            {
                Files.Move(Files.IndexOf(sortableList[i]), i);
            }

            if (!recurse) return;

            foreach (var file in Files)
            {
                if (file is Directory dir) dir.RecursiveSort(true);
            }
        }

        public bool RecursiveRemove(string path)
        {
            IResource removedFile = null;

            foreach (var file in Files)
            {
                if (file.Path == path)
                {
                    removedFile = file;
                    break;
                }

                if (file is Directory dir)
                {
                    bool subFolderFound = dir.RecursiveRemove(path);
                    if (subFolderFound) return true;
                }
            }

            if (removedFile == null) return false;

            Files.Remove(removedFile);
            return true;
        }
    }
}
