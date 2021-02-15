using System.Collections.ObjectModel;
using AngelNode.Model;
using AngelNode.Model.Project;

namespace AngelNode.Service.Interface
{
    public interface IProjectService
    {
        Project CurrentProject { get; }
        ObservableCollection<Variable> Variables { get; }
        ObservableCollection<Character> Characters { get; }
        ObservableCollection<Scene> Scenes { get; }

        void Load(string path);
        void Save(string path);
        ProjectReport Analyze();
        void CreateNew(string path);
        string GetRelativePath(string fullPath);
    }
}
