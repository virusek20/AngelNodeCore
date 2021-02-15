using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;

namespace AngelNode.Model.Project
{
    public class ProjectReport : ObservableObject
    {
        private bool _wasSuccessful = true;

        public bool WasSuccessful
        {
            get => _wasSuccessful;
            set { Set(() => WasSuccessful, ref _wasSuccessful, value); }
        }

        public ObservableCollection<ProjectReportMessage> Messages { get; } = new ObservableCollection<ProjectReportMessage>();

        public ProjectReport()
        {
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(Messages, this);
        }

        public void AddMessage(ProjectReportMessage.MessageSeverity severity, string text, object location = null)
        {
            if (severity == ProjectReportMessage.MessageSeverity.Error) _wasSuccessful = false;
            Messages.Add(new ProjectReportMessage(severity, text, location));
        }

        public IEnumerable<ProjectReportMessage> Unique()
        {
            return Messages.Distinct();
        }

        public static ProjectReport operator +(ProjectReport a, ProjectReport b)
        {
            var result = new ProjectReport
            {
                _wasSuccessful = a._wasSuccessful && b._wasSuccessful,
            };

            foreach (var message in a.Messages) result.Messages.Add(message);
            foreach (var message in b.Messages) result.Messages.Add(message);

            return result;
        }
    }
}
