using GalaSoft.MvvmLight;
using NuGet.Versioning;

namespace AngelNode.Model.Project
{
    public class Project : ObservableObject
    {
        private string _name;
        private string _path;
        private ProjectReport _projectReport;
        private SemanticVersion _version;

        public string Name
        {
            get => _name;
            set { Set(() => Name, ref _name, value); }
        }

        public string Path
        {
            get => _path;
            set { Set(() => Path, ref _path, value); }
        }

        public ProjectReport ProjectReport
        {
            get => _projectReport;
            set { Set(() => ProjectReport, ref _projectReport, value); }
        }

        public SemanticVersion Version
        {
            get => _version;
            set { Set(() => Version, ref _version, value); }
        }

        public string ResourcesPath => System.IO.Path.Combine(Path, "resources");

        /// <summary>
        /// Gets the absolute path to a resource.
        /// </summary>
        /// <param name="relativeResource">Path relative to the resources folder</param>
        /// <returns>Absolute path of resource</returns>
        public string GetResourceFullPath(string relativeResource)
        {
            return System.IO.Path.Combine(ResourcesPath, relativeResource);
        }
    }
}
