using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using AngelNode.Model.Resource;
using AngelNode.Service.Interface;
using AngelNode.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using Directory = AngelNode.Model.Resource.Directory;

namespace AngelNode.ViewModel
{
    public class ResourceTreeViewModel : ViewModelBase
    {
        private FileSystemWatcher _fileSystemWatcher;

        public Directory RootDirectory { get; }
        public RelayCommand ReimportCommand { get; }
        public RelayCommand<object> PreviewImageCommand { get; }

        public ResourceTreeViewModel()
        {
            var projectService = SimpleIoc.Default.GetInstance<IProjectService>();
            projectService.CurrentProject.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == "Path") CreateWatcher(projectService.CurrentProject.ResourcesPath);
                };

            CreateWatcher(projectService.CurrentProject.ResourcesPath);
            RootDirectory = new Directory(new DirectoryInfo(projectService.CurrentProject.ResourcesPath));
            ReimportCommand = new RelayCommand(() => Reimport(null));
            PreviewImageCommand = new RelayCommand<object>(PreviewImage);
        }

        private void PreviewImage(object file)
        {
            if (!(file is Model.Resource.File f) || f.GuessType() != ResourceType.Image) return;

            var preview = new ImagePreviewView();
            ((ImagePreviewViewModel) preview.DataContext).File = f;

            preview.Show();
        }

        private void CreateWatcher(string path)
        {
            if (_fileSystemWatcher != null)
            {
                _fileSystemWatcher.Path = path;
            }
            else
            {
                _fileSystemWatcher = new FileSystemWatcher(path)
                {
                    IncludeSubdirectories = true
                };

                _fileSystemWatcher.Created += (sender, args) => Reimport(args);
                _fileSystemWatcher.Deleted += (sender, args) => Reimport(args);
                _fileSystemWatcher.Renamed += (sender, args) => Reimport(args);
                _fileSystemWatcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName;
                _fileSystemWatcher.EnableRaisingEvents = true;
            }
        }

        private void Reimport(FileSystemEventArgs args)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    switch (args.ChangeType)
                    {
                        case WatcherChangeTypes.Created:
                            RootDirectory.RecursiveAdd(args.FullPath);
                            break;
                        case WatcherChangeTypes.Deleted:
                            RootDirectory.RecursiveRemove(args.FullPath);
                            break;
                        case WatcherChangeTypes.Renamed:
                            var e = (RenamedEventArgs) args;
                            RootDirectory.RecursiveRename(e.OldFullPath, e.FullPath);
                            break;
                    }
                }));
        }
    }
}
