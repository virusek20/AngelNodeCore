using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using AngelNode.Extension;
using AngelNode.Model;
using AngelNode.Model.Message;
using AngelNode.Model.Project;
using AngelNode.Service.Interface;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GongSolutions.Wpf.DragDrop;

namespace AngelNode.ViewModel
{
    public class ProjectTreeViewModel : ViewModelBase, IDropTarget
    {
        private readonly IProjectService _projectService;

        public ObservableCollection<Character> Characters => _projectService.Characters;
        public ObservableCollection<Scene> Scenes => _projectService.Scenes;
        public ObservableCollection<Variable> Variables => _projectService.Variables;

        public Project CurrentProject => _projectService.CurrentProject;

        public RelayCommand AddCharacterCommand { get; }
        public RelayCommand AddSceneCommand { get; }
        public RelayCommand ReloadTagsCommand { get; }
        public RelayCommand AddVariableCommand { get; }
        public RelayCommand<object> OpenDetailCommand { get; }
        public RelayCommand<object> RemoveCharacterCommand { get; }
        public RelayCommand<object> RemoveSceneCommand { get; }
        public RelayCommand<object> RemoveVariableCommand { get; }
        public RelayCommand<object> DeleteCommand { get; }
        public RelayCommand<object> RunCommand { get; }

        public ProjectTreeViewModel()
        {
            _projectService = SimpleIoc.Default.GetInstance<IProjectService>();

            AddCharacterCommand = new RelayCommand(AddCharacter);
            RemoveCharacterCommand = new RelayCommand<object>(RemoveCharacter);

            AddSceneCommand = new RelayCommand(AddScene);
            ReloadTagsCommand = new RelayCommand(ReloadTags);
            RemoveSceneCommand = new RelayCommand<object>(RemoveScene);

            AddVariableCommand = new RelayCommand(AddVariable);
            RemoveVariableCommand = new RelayCommand<object>(RemoveVariable);

            OpenDetailCommand = new RelayCommand<object>(OpenDetail);

            DeleteCommand = new RelayCommand<object>(Delete);

            RunCommand = new RelayCommand<object>(Run);
        }

        private void ReloadTags()
        {
            RaisePropertyChanged(() => Scenes);
        }

        private void Run(object objScene)
        {
            if (!(objScene is Scene scene)) return;

            MessengerInstance.Send(new RunMessage
            {
                Data = scene,
                RunMessageType = RunMessage.RunType.Scene
            });
        }

        private void Delete(object obj)
        {
            switch (obj)
            {
                case Character c:
                    MessengerInstance.Send(new TabCloseMessage
                    {
                        Data = c,
                        TabCloseType = TabCloseMessage.TabType.Character
                    });
                    _projectService.Characters.Remove(c);
                    break;
                case Scene s:
                    MessengerInstance.Send(new TabCloseMessage
                    {
                        Data = s,
                        TabCloseType = TabCloseMessage.TabType.Scene
                    });
                    _projectService.Scenes.Remove(s);
                    break;
                case Variable v:
                    MessengerInstance.Send(new TabCloseMessage
                    {
                        Data = v,
                        TabCloseType = TabCloseMessage.TabType.Variable
                    });
                    _projectService.Variables.Remove(v);
                    break;
            }
        }

        private void AddCharacter()
        {
            var character = new Character
            {
                Name = "New character"
            };

            Characters.Add(character);
            OpenDetail(character);
        }

        private void RemoveCharacter(object objCharacter)
        {
            if (!(objCharacter is Character character)) return;

            MessengerInstance.Send(new TabCloseMessage
            {
                Data = character,
                TabCloseType = TabCloseMessage.TabType.Character
            });

            Characters.Remove(character);
        }

        private void AddScene()
        {
            var scene = new Scene
            {
                Name = "New scene"
            };

            Scenes.Add(scene);
            OpenDetail(scene);
            ReloadTags();
        }

        private void RemoveScene(object objScene)
        {
            if (!(objScene is Scene scene)) return;

            MessengerInstance.Send(new TabCloseMessage
            {
                Data = scene,
                TabCloseType = TabCloseMessage.TabType.Scene
            });

            Scenes.Remove(scene);
            ReloadTags();
        }

        private void AddVariable()
        {
            var variable = new Variable
            {
                Name = "New variable"
            };

            Variables.Add(variable);
            OpenDetail(variable);
        }

        private void RemoveVariable(object objVariable)
        {
            if (!(objVariable is Variable variable)) return;

            MessengerInstance.Send(new TabCloseMessage
            {
                Data = variable,
                TabCloseType = TabCloseMessage.TabType.Variable
            });

            Variables.Remove(variable);
        }

        private void OpenDetail(object obj)
        {
            switch (obj)
            {
                case Character character:
                    MessengerInstance.Send(new TabOpenMessage
                    {
                        Data = character,
                        TabOpenType = TabOpenMessage.TabType.Character
                    });
                    break;
                case Scene scene:
                    MessengerInstance.Send(new TabOpenMessage
                    {
                        Data = scene,
                        TabOpenType = TabOpenMessage.TabType.Scene
                    });
                    break;
                case Variable variable:
                    MessengerInstance.Send(new TabOpenMessage
                    {
                        Data = variable,
                        TabOpenType = TabOpenMessage.TabType.Variable
                    });
                    break;
            }
        }

        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is TreeViewItem) return;
            if (dropInfo.Data == dropInfo.TargetItem) return;
            var target = dropInfo.InsertIndex;

            switch (dropInfo.Data)
            {
                case Character character:
                    if (!(dropInfo.TargetItem is Character)) return;
                    if (!ObservableCollectionExtensions.IsSamePosition(Characters, character, target).shouldMove) return;
                    break;
                case Scene scene:
                    if (!(dropInfo.TargetItem is Scene s)) return;
                    if (s.Tag != scene.Tag) return; // This would just make life complicated
                    if (!ObservableCollectionExtensions.IsSamePosition(Scenes, scene, target).shouldMove) return;
                    break;
                case Variable variable:
                    if (!(dropInfo.TargetItem is Variable)) return;
                    if (!ObservableCollectionExtensions.IsSamePosition(Variables, variable, target).shouldMove) return;
                    break;
                default:
                    return;
            }

            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            dropInfo.Effects = DragDropEffects.Move;
        }

        public void Drop(IDropInfo dropInfo)
        {
            var target = dropInfo.InsertIndex;

            switch (dropInfo.Data)
            {
                case Character character:
                    if (!(dropInfo.TargetItem is Character)) return;
                    Characters.Move(character, target);
                    break;
                case Scene scene:
                    if (!(dropInfo.TargetItem is Scene s)) return;
                    var sceneIndex = Scenes.IndexOf(s);
                    Scenes.Move(scene, sceneIndex + 1);
                    (dropInfo.TargetCollection as ObservableCollection<Scene>)?.Move(scene, target);
                    break;
                case Variable variable:
                    if (!(dropInfo.TargetItem is Variable)) return;
                    Variables.Move(variable, target);
                    break;
            }
        }
    }
}
