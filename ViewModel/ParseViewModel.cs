using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using AngelNode.Model.Node;
using AngelNode.Service.Interface;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;

namespace AngelNode.ViewModel
{
    public class ParseViewModel : ViewModelBase
    {
        private readonly IProjectService _projectService;

        private string _text;

        public RelayCommand RefreshCommand { get; }
        public RelayCommand<Window> AcceptCommand { get; }
        public RelayCommand<Window> CancelCommand { get; }

        public bool Accepted { get; private set; }

        public string Text
        {
            get => _text;
            set
            {
                Set(() => Text, ref _text, value);
                ParseText();
            }
        }


        public ObservableCollection<INode> Nodes { get; } = new ObservableCollection<INode>();

        public ParseViewModel()
        {
            _projectService = SimpleIoc.Default.GetInstance<IProjectService>();

            AcceptCommand = new RelayCommand<Window>(Accept);
            CancelCommand = new RelayCommand<Window>(Cancel);

            RefreshCommand = new RelayCommand(ParseText);
        }

        private void Cancel(Window window)
        {
            window.Close();
        }

        private void Accept(Window window)
        {
            Accepted = true;
            window.Close();
        }

        private void ParseText()
        {
            Nodes.Clear();

            foreach (var line in Regex.Split(Text, @"\r?\n|\r"))
            {
                if (Regex.IsMatch(line, @"^\*.*\*$"))
                {
                    Nodes.Add(new NodePlaySound());
                    continue;
                }

                if (Regex.IsMatch(line, @"^\(.*\)$"))
                {
                    Nodes.Add(new NodeChangePose());
                    continue;
                }

                if (Regex.IsMatch(line, @"^\[.*\]$"))
                {
                    Nodes.Add(new NodeChangeBackground());
                    continue;
                }

                if (Regex.IsMatch(line, @"^\<.*\>$"))
                {
                    var musicNode = new NodePlaySound
                    {
                        SoundType = NodePlaySound.SoundTypeEnum.Music
                    };
                    Nodes.Add(musicNode);
                    continue;
                }

                var match = Regex.Match(line, @"^(.*): (.*)$");
                if (match.Success)
                {
                    var foundName = match.Groups[1].Value.Trim();
                    if (foundName == "MC") foundName = "Player";
                    var dialogueNode = new NodeDialogue
                    {
                        Text = match.Groups[2].Value.Trim().Replace("“", "").Replace("”", "").Replace("’", "'").Replace("…", "..."),
                        Character = _projectService.Characters.FirstOrDefault(character => character.Name == foundName)
                    };
                    Nodes.Add(dialogueNode);
                }
                else
                {
                    var subMatch = Regex.Match(line, @"^-(.*)$");
                    if (!subMatch.Success) continue;

                    var dialogueNode = new NodeDialogue
                    {
                        Text = subMatch.Groups[1].Value.Trim().Replace("“", "").Replace("”", "").Replace("’", "'").Replace("…", "..."),
                        // TODO: Nono
                        Character = _projectService.Characters.First(c => c.Name == "Player - Internal")
                    };
                    Nodes.Add(dialogueNode);
                }
            }
        }
    }
}
