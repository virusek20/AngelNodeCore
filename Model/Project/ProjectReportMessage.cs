using System;
using AngelNode.Model.Node;
using AngelNode.Service.Interface;
using GalaSoft.MvvmLight.Ioc;

namespace AngelNode.Model.Project
{
    public class ProjectReportMessage : IEquatable<ProjectReportMessage>
    {
        public enum MessageSeverity
        {
            Debug,
            Info,
            Warning,
            Error
        }

        public MessageSeverity Severity { get; }
        public string Text { get; }
        public object Location { get; }

        public NodeLocation Scene
        {
            get
            {
                if (!(Location is INode searchNode)) return new NodeLocation();

                // Eh, it works
                var projectService = SimpleIoc.Default.GetInstance<IProjectService>();
                foreach (var scene in projectService.Scenes)
                {
                    var id = scene.Nodes.IndexOf(searchNode);
                    if (id != -1)
                        return new NodeLocation
                        {
                            Name = scene.Name,
                            Scene = scene,
                            Node = searchNode,
                            NodeId = (id + 1).ToString()
                        };
                }

                return new NodeLocation();
            }
        }

        public ProjectReportMessage(MessageSeverity severity, string text, object location)
        {
            Severity = severity;
            Text = text;
            Location = location;
        }

        public class NodeLocation
        {
            public string Name { get; set; }
            public string NodeId { get; set; }
            public Scene Scene { get; set; }
            public INode Node { get; set; }
        }

        public bool Equals(ProjectReportMessage other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Severity == other.Severity && string.Equals(Text, other.Text) && Equals(Location, other.Location);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ProjectReportMessage) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Severity;
                hashCode = (hashCode * 397) ^ (Text != null ? Text.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Location != null ? Location.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
