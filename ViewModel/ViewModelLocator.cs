using System;
using AngelNode.Service.Implementations.DOT;
using AngelNode.Service.Implementations.ExportXML;
using AngelNode.Service.Implementations.ProjectXML;
using AngelNode.Service.Implementations.RenPy;
using AngelNode.Service.Implementations.ResourceUsage;
using AngelNode.Service.Interface;
using GalaSoft.MvvmLight.Ioc;

namespace AngelNode.ViewModel
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<ProjectTreeViewModel>();
            SimpleIoc.Default.Register<ResourceTreeViewModel>();
            SimpleIoc.Default.Register<CharacterViewModel>();
            SimpleIoc.Default.Register<VariableViewModel>();
            SimpleIoc.Default.Register<SceneViewModel>();
            SimpleIoc.Default.Register<NodeSelectionViewModel>();
            SimpleIoc.Default.Register<LaunchViewModel>();
            SimpleIoc.Default.Register<AboutViewModel>();
            SimpleIoc.Default.Register<ReportViewModel>();
            SimpleIoc.Default.Register<ResourceSelectionViewModel>();
            SimpleIoc.Default.Register<ImagePreviewViewModel>();
            SimpleIoc.Default.Register<ParseViewModel>();
            SimpleIoc.Default.Register<StartViewModel>();

            if (!SimpleIoc.Default.ContainsCreated<IProjectService>())
            {
                SimpleIoc.Default.Register<IProjectService>(() => new LocalProjectService());
                //SimpleIoc.Default.Register<IExportService>(() => new CppExportService(), "cpp");
                SimpleIoc.Default.Register<IExportService>(() => new RenPyExportService(), "renpy");
                SimpleIoc.Default.Register<IExportService>(() => new XmlExportService(), "xml");
                SimpleIoc.Default.Register<IExportService>(() => new DotExportService(), "dot");
                SimpleIoc.Default.Register<IExportService>(() => new ResourceUsageExportService(), "rsc");
            }
        }

        public MainViewModel MainViewModel => SimpleIoc.Default.GetInstance<MainViewModel>(Guid.NewGuid().ToString());
        public ProjectTreeViewModel ProjectTreeViewModel => SimpleIoc.Default.GetInstance<ProjectTreeViewModel>();
        public ResourceTreeViewModel ResourceTreeViewModel => SimpleIoc.Default.GetInstance<ResourceTreeViewModel>();
        public CharacterViewModel CharacterViewModel => SimpleIoc.Default.GetInstance<CharacterViewModel>(Guid.NewGuid().ToString());
        public VariableViewModel VariableViewModel => SimpleIoc.Default.GetInstance<VariableViewModel>(Guid.NewGuid().ToString());
        public SceneViewModel SceneViewModel => SimpleIoc.Default.GetInstance<SceneViewModel>(Guid.NewGuid().ToString());
        public NodeSelectionViewModel NodeSelectionViewModel => SimpleIoc.Default.GetInstance<NodeSelectionViewModel>(Guid.NewGuid().ToString());
        public LaunchViewModel LaunchViewModel => SimpleIoc.Default.GetInstance<LaunchViewModel>();
        public AboutViewModel AboutViewModel => SimpleIoc.Default.GetInstance<AboutViewModel>();
        public ReportViewModel ReportViewModel => SimpleIoc.Default.GetInstance<ReportViewModel>(Guid.NewGuid().ToString());
        public ResourceSelectionViewModel ResourceSelectionViewModel => SimpleIoc.Default.GetInstance<ResourceSelectionViewModel>(Guid.NewGuid().ToString());
        public ImagePreviewViewModel ImagePreviewViewModel => SimpleIoc.Default.GetInstance<ImagePreviewViewModel>(Guid.NewGuid().ToString());
        public ParseViewModel ParseViewModel => SimpleIoc.Default.GetInstance<ParseViewModel>(Guid.NewGuid().ToString());
        public StartViewModel StartViewModel => SimpleIoc.Default.GetInstance<StartViewModel>();
    }
}