using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;

namespace AngelNode.Model
{
    public class SceneFolder : ObservableObject, IScene
    {
        private string _name;

        /// <summary>
        /// Gets or sets the folder name
        /// </summary>
        public string Name
        {
            get => _name;
            set { Set(() => Name, ref _name, value); }
        }

        /// <summary>
        /// Gets the observable collection containing scenes within this folder
        /// </summary>
        public ObservableCollection<Scene> Scenes { get; } = new ObservableCollection<Scene>();
    }
}
