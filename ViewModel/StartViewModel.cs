using AngelNode.Model.Project;
using AngelNode.Service.Interface;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;

namespace AngelNode.ViewModel
{
    public class StartViewModel : ViewModelBase
    {
        private Project _project;

        public Project Project
        {
            get => _project;
            set { Set(() => Project, ref _project, value); }
        }

        public int Major
        {
            get => _project.Version.Major;
            set
            {
                _project.Version = new NuGet.Versioning.SemanticVersion(value, _project.Version.Minor, _project.Version.Patch);
                RaisePropertyChanged(() => Major);
            }
        }

        public int Minor
        {
            get => _project.Version.Minor;
            set
            {
                _project.Version = new NuGet.Versioning.SemanticVersion(_project.Version.Major, value, _project.Version.Patch);
                RaisePropertyChanged(() => Minor);
            }
        }

        public int Patch
        {
            get => _project.Version.Patch;
            set
            {
                _project.Version = new NuGet.Versioning.SemanticVersion(_project.Version.Major, _project.Version.Minor, value);
                RaisePropertyChanged(() => Patch);
            }
        }

        public StartViewModel()
        {
            _project = SimpleIoc.Default.GetInstance<IProjectService>().CurrentProject;
        }
    }
}
