using System.Collections.ObjectModel;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AngelNode.Model;
using AngelNode.Model.Resource;
using AngelNode.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GongSolutions.Wpf.DragDrop;

namespace AngelNode.ViewModel
{
    public class CharacterViewModel : ViewModelBase, IDropTarget
    {
        private Character _character;
        private ObservableCollection<Pose> _poses;

        public Character Character
        {
            get => _character;
            set
            {
                Set(() => Character, ref _character, value);
                RaisePropertyChanged(nameof(PhonePictureSource));
                RaisePropertyChanged(nameof(Poses));
            }
        }

        public ObservableCollection<File> PhonePictureSource => new ObservableCollection<File>
        {
            Character?.PhonePicture
        };

        public ObservableCollection<Pose> Poses
        {
            get
            {
                if (_poses == null) _poses = Character?.Poses;
                return _poses;
            }
        }

        public RelayCommand<Pose> DeletePoseCommand { get; }
        public RelayCommand<Pose> SetDefaultPoseCommand { get; }
        public RelayCommand<Pose> SetShowcasePoseCommand { get; }
        public RelayCommand<Outfit> DeleteOutfitCommand { get; }
        public RelayCommand ResourcePickerCommand { get; }

        public CharacterViewModel()
        {
            DeletePoseCommand = new RelayCommand<Pose>(DeletePose);
            SetDefaultPoseCommand = new RelayCommand<Pose>(SetDefaultPose);
            SetShowcasePoseCommand = new RelayCommand<Pose>(SetShowcasePose);
            DeleteOutfitCommand = new RelayCommand<Outfit>(DeleteOutfit);
            ResourcePickerCommand = new RelayCommand(OpenResourcePicker);
        }

        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is IResource resource && dropInfo.VisualTarget is ListView)
            {
                if (resource is File f && f.GuessType() != ResourceType.Image) return;

                dropInfo.Effects = DragDropEffects.All;
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.VisualTarget is ListView) AddPoseFromResource(dropInfo.Data as IResource);
            else if (dropInfo.VisualTarget is ListBox) TrySetPhonePicture(dropInfo.Data as File);
        }

        private void AddPoseFromResource(IResource resource)
        {
            switch (resource)
            {
                case File file:
                    if (Character.Poses.FirstOrDefault((pose => pose.File.Path == file.Path)) != null) break;

                    if (file.GuessType() == ResourceType.Image)
                    {
                        var pose = new Pose
                        {
                            File = file,
                            Name = System.IO.Path.GetFileNameWithoutExtension(file.Path),
                            Relative = false
                        };

                        Character.SharedPoses.Add(pose);
                        Poses.Add(pose);
                    }
                    
                    break;
                case Directory directory:
                    foreach (var subFile in directory.Files) AddPoseFromResource(subFile);
                    break;
            }
        }

        private void TrySetPhonePicture(File file)
        {
            if (file == null) return;
            if (file.GuessType() == ResourceType.Image) 
            {
                Character.PhonePicture = file;
                RaisePropertyChanged(nameof(PhonePictureSource));
            }
        }

        private void SetDefaultPose(Pose pose)
        {
            Character.DefaultPose = pose;
        }

        private void SetShowcasePose(Pose pose)
        {
            if (!Character.OutfitPoses.Contains(pose)) throw new ArgumentException("Provided pose has to be an outfit pose.", nameof(pose));

            Character.ShowcasePose = pose;
        }

        private void DeletePose(Pose pose)
        {
            if (pose == null) return;

            if (Character.DefaultPose == pose) Character.DefaultPose = Character.Poses.FirstOrDefault(p => p != pose);
            if (Character.ShowcasePose == pose) Character.ShowcasePose = Character.OutfitPoses.FirstOrDefault(p => p != pose);

            Character.SharedPoses.Remove(pose);
            Character.OutfitPoses.Remove(pose);
            Poses.Remove(pose);
        }

        private void DeleteOutfit(Outfit outfit)
        {
            if (outfit == null) return;

            if (Character.Outfits.Count == 1)
            {
                var result = MessageBox.Show("This is the last outfit for this character. Removing this outfit will break all pose change nodes associated with this character.\nDo you wish to proceed?", "Pose removal", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No) return;
            }

            Character.Outfits.Remove(outfit);
            if (Character.Outfits.Count == 0) Character.OutfitPoses.Clear();

            _poses.Clear();
            foreach (var pose in Character.Poses) _poses.Add(pose);
        }

        private void OpenResourcePicker()
        {
            var window = new ResourceSelectionView { Owner = Application.Current.MainWindow };
            var dataContext = (ResourceSelectionViewModel)window.DataContext;
            dataContext.ResourceType = ResourceType.Image;

            window.ShowDialog();

            if (dataContext.IsValid)
            {
                Character.PhonePicture = dataContext.SelectedNode as File;
                RaisePropertyChanged(nameof(PhonePictureSource));
            }
        }
    }
}
