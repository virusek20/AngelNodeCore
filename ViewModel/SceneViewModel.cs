using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using AngelNode.Model;
using AngelNode.Model.Message;
using AngelNode.Model.Node;
using AngelNode.Model.Resource;
using AngelNode.Service.Interface;
using AngelNode.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using File = AngelNode.Model.Resource.File;

namespace AngelNode.ViewModel
{
    public class SceneViewModel : ViewModelBase
    {
        private readonly IProjectService _projectService;

        private Scene _scene;
        private INode _selectedNode;

        public Scene Scene
        {
            get => _scene;
            set { Set(() => Scene, ref _scene, value); }
        }

        public INode SelectedNode
        {
            get => _selectedNode;
            set
            {
                Set(() => SelectedNode, ref _selectedNode, value);

                switch (value)
                {
                    // Questionable, produces visual glitch | Gray box in empty space
                    case NodeChangeBackground _:
                        RaisePropertyChanged(nameof(BackgroundSource));
                        break;
                    case NodePlaySound _:
                        RaisePropertyChanged(nameof(SoundSource));
                        break;
                    case NodeJump _:
                        RaisePropertyChanged(nameof(TargetSource));
                        break;
                    case NodeVariableJump _:
                        RaisePropertyChanged(nameof(CTargetSource));
                        break;
                }
            }
        }

        public RelayCommand<object> SelectNodeCommand { get; }
        public RelayCommand<IList> RemoveNodesCommand { get; }
        public RelayCommand<ListViewItem> AddLibraryNodeCommand { get; }
        public RelayCommand<Type> AddContextNodeCommand { get; }
        public RelayCommand ResourcePickerCommand { get; }
        public RelayCommand AddOptionCommand { get; }
        public RelayCommand AddMessageCommand { get; }
        public RelayCommand DuplicatePhoneCommand { get; }
        public RelayCommand ParseCommand { get; }
        public RelayCommand RunCommand { get; }
        public RelayCommand ConvertResponseCommand { get; }
        public RelayCommand<Scene> JumpSceneCommand { get; }

        public ObservableCollection<Character> Characters => _projectService.Characters;
        public ObservableCollection<Variable> Variables => _projectService.Variables;
        public ObservableCollection<Scene> Scenes => _projectService.Scenes;

        public ObservableCollection<File> BackgroundSource
        {
            get
            {
                if (SelectedNode is NodeChangeBackground ncb)
                {
                    return new ObservableCollection<File>
                    {
                        ncb.Background
                    };
                }

                return new ObservableCollection<File>();
            }
        }

        public ObservableCollection<INode> TargetSource
        {
            get
            {
                return SelectedNode switch
                {
                    NodeJump nj => new ObservableCollection<INode> { nj.Target },
                    _ => new ObservableCollection<INode>(),
                };
            }
        }

        public ObservableCollection<File> SoundSource
        {
            get
            {
                return SelectedNode switch
                {
                    NodePlaySound nps => new ObservableCollection<File> { nps.Sound },
                    _ => new ObservableCollection<File>(),
                };
            }
        }

        public ObservableCollection<INode> CTargetSource
        {
            get
            {
                return SelectedNode switch
                {
                    NodeVariableJump nvj => new ObservableCollection<INode> { nvj.Target },
                    _ => new ObservableCollection<INode>(),
                };
            }
        }

        public SceneViewModel()
        {
            _projectService = SimpleIoc.Default.GetInstance<IProjectService>();

            SelectNodeCommand = new RelayCommand<object>(SelectNode);
            RemoveNodesCommand = new RelayCommand<IList>(RemoveNodes);
            AddLibraryNodeCommand = new RelayCommand<ListViewItem>(AddLibraryNode);
            AddContextNodeCommand = new RelayCommand<Type>(AddContextNode);
            ResourcePickerCommand = new RelayCommand(OpenResourcePicker);
            AddOptionCommand = new RelayCommand(AddOption);
            AddMessageCommand = new RelayCommand(AddMessage);
            DuplicatePhoneCommand = new RelayCommand(DuplicatePhone);
            ParseCommand = new RelayCommand(Parse);
            RunCommand = new RelayCommand(Run);
            ConvertResponseCommand = new RelayCommand(ConvertResponse);
            JumpSceneCommand = new RelayCommand<Scene>(JumpScene);
        }

        private void JumpScene(Scene scene)
        {
            MessengerInstance.Send(new TabOpenMessage
            {
                Data = scene,
                TabOpenType = TabOpenMessage.TabType.Scene
            });
        }

        private void ConvertResponse()
        {
            if (!(SelectedNode is NodeDialogue dialogue)) throw new InvalidCastException("Cannot cast node to NodeDialogue");

            var index = Scene.Nodes.IndexOf(SelectedNode);
            Scene.Nodes.Remove(SelectedNode);

            var responseNode = new NodeResponseDialogue
            {
                Character = dialogue.Character,
                Dialogue = dialogue.Text
            };

            Scene.Nodes.Insert(index, responseNode);
            SelectedNode = responseNode;
        }

        private void Run()
        {
            MessengerInstance.Send(new RunMessage
            {
                Data = SelectedNode,
                RunMessageType = RunMessage.RunType.Node
            });
        }

        private void Parse()
        {
            var window = new ParseView
            {
                Owner = Application.Current.MainWindow
            };

            window.ShowDialog();
            var dataContext = (ParseViewModel)window.DataContext;

            if (!dataContext.Accepted) return;
            foreach (var node in dataContext.Nodes) Scene.Nodes.Add(node);
        }

        private void AddOption()
        {
            ((NodeResponseDialogue)SelectedNode).ResponseMap.Add(new DialogueResponse());
        }

        private void OpenResourcePicker()
        {
            var window = new ResourceSelectionView { Owner = Application.Current.MainWindow };
            var dataContext = (ResourceSelectionViewModel)window.DataContext;

            dataContext.ResourceType = SelectedNode switch
            {
                NodeChangeBackground _ => ResourceType.Image,
                NodePlaySound _ => ResourceType.Sound,
                _ => throw new InvalidOperationException(),
            };
            window.ShowDialog();

            if (dataContext.IsValid)
            {
                switch (SelectedNode)
                {
                    case NodeChangeBackground ncb:
                        ncb.Background = dataContext.SelectedNode as File;
                        RaisePropertyChanged(nameof(BackgroundSource));
                        break;
                    case NodePlaySound nps:
                        nps.Sound = dataContext.SelectedNode as File;
                        RaisePropertyChanged(nameof(SoundSource));
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        private void AddContextNode(Type nodeType)
        {
            if (!(Activator.CreateInstance(nodeType) is INode newNode))
            {
                throw new InvalidCastException($"Instantiated class '{nodeType}' does not implement INode");
            }

            Scene.Nodes.Add(newNode);
            SelectedNode = newNode;
        }

        private void AddLibraryNode(ListViewItem nodeType)
        {
            var type = nodeType.Resources?["NodeType"] as Type;
            if (type == null) throw new InvalidDataException($"Supplied item does not contain a valid 'NodeType' definition");
            AddContextNode(type);
        }

        private void SelectNode(object node)
        {
            var window = new NodeSelectionView { Owner = Application.Current.MainWindow };
            var dataContext = (NodeSelectionViewModel)window.DataContext;
            dataContext.Scene = Scene;

            window.ShowDialog();
            if (!dataContext.IsValid) return;

            switch (node)
            {
                case NodeJump nj:
                    nj.Target = dataContext.SelectedNode;
                    RaisePropertyChanged(nameof(TargetSource));
                    break;
                case NodeVariableJump nvj:
                    nvj.Target = dataContext.SelectedNode;
                    RaisePropertyChanged(nameof(CTargetSource));
                    break;
                case DialogueResponse dr:
                    dr.Target = dataContext.SelectedNode;
                    break;
            }
        }

        private void RemoveNodes(IList selectedItems)
        {
            while (selectedItems.Count != 0)
            {
                Scene.Nodes.Remove(selectedItems[0] as INode);
            }
        }

        private void AddMessage()
        {
            ((NodePhone)SelectedNode).PhoneMessages.Add(new NodePhone.PhoneMessage());
        }

        private void DuplicatePhone()
        {
            if (!(SelectedNode is NodePhone phone)) return;
            var duplicate = new NodePhone
            {
                Character = phone.Character,
                Time = phone.Time
            };

            foreach (var message in phone.PhoneMessages)
            {
                duplicate.PhoneMessages.Add(new NodePhone.PhoneMessage
                {
                    PlayerMade = message.PlayerMade,
                    Skip = message.Skip,
                    Text = message.Text
                });
            }

            Scene.Nodes.Add(duplicate);
            SelectedNode = duplicate;
        }
    }
}
