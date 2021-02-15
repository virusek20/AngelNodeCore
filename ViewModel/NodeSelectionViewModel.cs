using System;
using System.Collections.ObjectModel;
using System.Windows;
using AngelNode.Model;
using AngelNode.Model.Node;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace AngelNode.ViewModel
{
    public class NodeSelectionViewModel : ViewModelBase
    {
        private INode _selectedNode;
        private Scene _scene;

        public ObservableCollection<INode> Nodes => _scene?.Nodes;

        public INode SelectedNode
        {
            get => _selectedNode;
            set
            {
                Set(() => SelectedNode, ref _selectedNode, value);
                RaisePropertyChanged(nameof(CanConfirm));
            }
        }

        public Scene Scene
        {
            get => _scene;
            set
            {
                Set(() => Scene, ref _scene, value);

                _selectedNode = null;
                RaisePropertyChanged(nameof(SelectedNode));
                RaisePropertyChanged(nameof(Nodes));
            }
        }

        public bool IsValid { get; private set; }

        public bool CanConfirm => SelectedNode != null;

        public RelayCommand<Window> OkCommand { get; }
        public RelayCommand<Window> CancelCommand { get; }

        public RelayCommand<object> TreeSelectCommand { get; }
        public RelayCommand<object> SelectOkCommand { get; }

        public NodeSelectionViewModel()
        {
            OkCommand = new RelayCommand<Window>(Ok);
            CancelCommand = new RelayCommand<Window>(Cancel);
            TreeSelectCommand = new RelayCommand<object>(TreeSelect);
            SelectOkCommand = new RelayCommand<object>(SelectOk);
        }

        private void SelectOk(object selectedItemView)
        {
            var args = (Tuple<object, object>)selectedItemView;
            SelectedNode = args.Item1 as INode;
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
            SelectedNode = selectedItem as INode;
        }
    }
}
