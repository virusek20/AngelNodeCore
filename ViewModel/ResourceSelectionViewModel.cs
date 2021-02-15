using System;
using System.Windows;
using AngelNode.Model.Resource;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;

namespace AngelNode.ViewModel
{
    public class ResourceSelectionViewModel : ViewModelBase
    {
        private IResource _selectedNode;

        public Directory RootDirectory => SimpleIoc.Default.GetInstance<ResourceTreeViewModel>().RootDirectory;

        public IResource SelectedNode
        {
            get => _selectedNode;
            set
            {
                Set(() => SelectedNode, ref _selectedNode, value);
                RaisePropertyChanged(nameof(CanConfirm));
            }
        }

        public bool IsValid { get; private set; }

        public bool CanConfirm
        {
            get
            {
                if (SelectedNode == null) return false;
                switch (SelectedNode)
                {
                    case File f:
                        if (!AllowFiles) return false;

                        if (ResourceType != ResourceType.Unknown) return ResourceType == f.GuessType();
                        else return true;
                    case Directory d:
                        return AllowDirectories;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public ResourceType ResourceType { get; set; } = ResourceType.Unknown;
        public bool AllowDirectories { get; set; } = false;
        public bool AllowFiles { get; set; } = true;

        public RelayCommand<Window> OkCommand { get; }
        public RelayCommand<Window> CancelCommand { get; }

        public RelayCommand<object> TreeSelectCommand { get; }
        public RelayCommand<object> SelectOkCommand { get; }

        public ResourceSelectionViewModel()
        {
            OkCommand = new RelayCommand<Window>(Ok);
            CancelCommand = new RelayCommand<Window>(Cancel);
            TreeSelectCommand = new RelayCommand<object>(TreeSelect);
            SelectOkCommand = new RelayCommand<object>(SelectOk);
        }

        private void SelectOk(object selectedItemView)
        {
            var args = (Tuple<object, object>)selectedItemView;
            SelectedNode = args.Item1 as File;
            if (!CanConfirm) return;

            IsValid = true;
            ((Window)args.Item2).Close();
        }

        private void Ok(Window view)
        {
            IsValid = true;
            view.Close();
        }

        private void Cancel(Window view)
        {
            view.Close();
        }

        private void TreeSelect(object selectedItem)
        {
            SelectedNode = selectedItem as File;
        }
    }
}
