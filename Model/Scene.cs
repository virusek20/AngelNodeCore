using System.Collections.Generic;
using System.Collections.ObjectModel;
using AngelNode.Model.Node;
using GalaSoft.MvvmLight;

namespace AngelNode.Model
{
    public class Scene : ObservableObject, IScene
    {
        private string _name;
        private string _tag = string.Empty;
        private string _startpointName;
        private bool _isStartpoint;

        public string Name
        {
            get => _name;
            set { Set(() => Name, ref _name, value); }
        }

        public string Tag
        {
            get => _tag;
            set { Set(() => Tag, ref _tag, value); }
        }

        public bool IsStartpoint 
        {
            get => _isStartpoint;
            set => Set(() => IsStartpoint, ref _isStartpoint, value);
        }

        public string StartpointName 
        {
            get => _startpointName;
            set => Set(() => StartpointName, ref _startpointName, value);
        }

        public ObservableCollection<INode> Nodes { get; } = new ObservableCollection<INode>();

        public Scene() { }

        public Scene(string name, string tag, IEnumerable<INode> nodes)
        {
            Name = name;
            Tag = tag;
            Nodes = new ObservableCollection<INode>(nodes);
        }
    }
}
