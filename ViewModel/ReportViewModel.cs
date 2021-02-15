using System;
using System.ComponentModel;
using System.Linq;
using AngelNode.Model;
using AngelNode.Model.Message;
using AngelNode.Model.Node;
using AngelNode.Model.Project;
using AngelNode.Service.Interface;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;

namespace AngelNode.ViewModel
{
    public class ReportViewModel : ViewModelBase
    {
        private readonly IProjectService _projectService;

        public ReportViewModel()
        {
            _projectService = SimpleIoc.Default.GetInstance<IProjectService>();
            Project.PropertyChanged += CountErrors;

            OpenLocationCommand = new RelayCommand<ProjectReportMessage>(OpenLocation);
        }

        private void OpenLocation(ProjectReportMessage message)
        {
            switch (message.Location)
            {
                case Character c:
                    MessengerInstance.Send(new TabOpenMessage
                    {
                        Data = c,
                        TabOpenType = TabOpenMessage.TabType.Character
                    });
                    break;
                case Scene s:
                    MessengerInstance.Send(new TabOpenMessage
                    {
                        Data = s,
                        TabOpenType = TabOpenMessage.TabType.Scene
                    });
                    break;
                case Variable v:
                    MessengerInstance.Send(new TabOpenMessage
                    {
                        Data = v,
                        TabOpenType = TabOpenMessage.TabType.Variable
                    });
                    break;
                case INode node:
                    MessengerInstance.Send(new TabOpenMessage
                    {
                        Data = message.Scene,
                        TabOpenType = TabOpenMessage.TabType.Node
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public Project Project => _projectService.CurrentProject;

        public int ErrorCount => Project.ProjectReport?.Messages.Count(m => m.Severity == ProjectReportMessage.MessageSeverity.Error) ?? 0;
        public int WarningCount => Project.ProjectReport?.Messages.Count(m => m.Severity == ProjectReportMessage.MessageSeverity.Warning) ?? 0;
        public int InfoCount => Project.ProjectReport?.Messages.Count(m => m.Severity == ProjectReportMessage.MessageSeverity.Info) ?? 0;

        public ProjectReportMessage.MessageSeverity ErrorSeverity { get; } = ProjectReportMessage.MessageSeverity.Error;
        public ProjectReportMessage.MessageSeverity WarningSeverity { get; } = ProjectReportMessage.MessageSeverity.Warning;
        public ProjectReportMessage.MessageSeverity InfoSeverity { get; } = ProjectReportMessage.MessageSeverity.Info;

        public RelayCommand<ProjectReportMessage> OpenLocationCommand { get; }

        private void CountErrors(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "ProjectReport")
            {
                RaisePropertyChanged(nameof(ErrorCount));
                RaisePropertyChanged(nameof(WarningCount));
                RaisePropertyChanged(nameof(InfoCount));
            }
        }
    }
}
