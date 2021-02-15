using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using AngelNode.Model;
using AngelNode.Model.Node;
using AngelNode.Service.Interface;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Win32;

namespace AngelNode.Service.Implementations.ExportXML
{
    public class XmlExportService : IExportService
    {
        private readonly IProjectService _projectService;

        private string _executablePath;

        public XmlExportService()
        {
            _projectService = SimpleIoc.Default.GetInstance<IProjectService>();
        }

        public void Export(string path)
        {
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8,
                NewLineChars = "\n"
            };

            using (XmlWriter writer = XmlWriter.Create(path, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Project");
                writer.WriteAttributeString("Name", _projectService.CurrentProject.Name ?? string.Empty);
                writer.WriteAttributeString("Version", _projectService.CurrentProject.Version.ToString());

                writer.WriteStartElement("Characters");
                foreach (var character in _projectService.Characters) SaveCharacter(writer, character);
                writer.WriteEndElement();

                writer.WriteStartElement("Scenes");
                var nodeMapping = MapNodes();
                var nodeVisitor = new XmlExportNodeVisitor(_projectService, writer, nodeMapping);
                foreach (var scene in _projectService.Scenes) SaveScene(writer, scene, nodeVisitor);
                writer.WriteEndElement();

                writer.WriteStartElement("Backgrounds");
                foreach (var background in nodeVisitor.Backgrounds)
                {
                    writer.WriteStartElement("Background");
                    writer.WriteAttributeString("Path", background);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                writer.WriteStartElement("Sounds");
                foreach (var sound in nodeVisitor.Sounds)
                {
                    writer.WriteStartElement("Sound");
                    writer.WriteAttributeString("Path", sound);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                writer.WriteStartElement("Startpoints");
                foreach (var scene in _projectService.Scenes.Where(s => s.IsStartpoint)) SaveStartpoint(writer, scene, nodeMapping);
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        public int GetNodeNumber(INode node)
        {
            var mapping = MapNodes();
            return mapping[node];
        }

        public string DefaultExtension { get; } = ".xml";
        public string FileFilter { get; } = "XML Files|*.xml";

        private void SaveCharacter(XmlWriter writer, Character character)
        {
            writer.WriteStartElement("Character");
            writer.WriteAttributeString("Name", character.Name ?? string.Empty);
            writer.WriteAttributeString("Height", character.Height.ToString());
            writer.WriteAttributeString("Pitch", character.Pitch.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("Color", $"{character.Color.R};{character.Color.G};{character.Color.B}");
            writer.WriteAttributeString("PhoneNumber", character.PhoneNumber ?? string.Empty);
            if (character.DefaultPose != null) writer.WriteAttributeString("DefaultPose", character.Poses.IndexOf(character.DefaultPose).ToString());
            if (character.ShowcasePose != null) writer.WriteAttributeString("ShowcasePose", character.Poses.IndexOf(character.ShowcasePose).ToString());
            if (character.PhonePicture != null) writer.WriteAttributeString("PhonePicture", _projectService.GetRelativePath(character.PhonePicture.Path).Replace('\\', '/').Replace(".png", ".DDS"));
            else writer.WriteAttributeString("PhonePicture", string.Empty);

            writer.WriteStartElement("Poses");
            foreach (var pose in character.Poses) SavePose(writer, pose);
            writer.WriteEndElement();

            writer.WriteStartElement("Outfits");
            foreach (var outfit in character.Outfits) SaveOutfit(writer, outfit);
            writer.WriteEndElement();

            writer.WriteEndElement();
        }

        private void SavePose(XmlWriter writer, Pose pose)
        {
            writer.WriteStartElement("Pose");
            if (pose.Relative) writer.WriteAttributeString("RelativePath", pose.File.Name.Replace(".png", ".DDS"));
            else writer.WriteAttributeString("Path", "resource/" + _projectService.GetRelativePath(pose.File.Path).Replace('\\', '/').Replace(".png", ".DDS"));

            writer.WriteEndElement();
        }

        private void SaveOutfit(XmlWriter writer, Outfit outfit)
        {
            writer.WriteStartElement("Outfit");
            writer.WriteAttributeString("Path", "resource/" + _projectService.GetRelativePath(outfit.Directory.Path).Replace('\\', '/'));
            writer.WriteEndElement();
        }

        private void SaveScene(XmlWriter writer, Scene scene, INodeVisitor visitor)
        {
            writer.WriteStartElement("Scene");
            writer.WriteAttributeString("Name", scene.Name ?? string.Empty);

            writer.WriteStartElement("Nodes");
            foreach (var node in scene.Nodes) node.Accept(visitor);
            writer.WriteEndElement();

            writer.WriteEndElement();
        }

        private void SaveStartpoint(XmlWriter writer, Scene scene, IDictionary<INode, int> nodeMapping)
        {
            writer.WriteStartElement("Startpoint");
            writer.WriteAttributeString("StartpointName", scene.StartpointName);
            writer.WriteAttributeString("TargetNode", nodeMapping[scene.Nodes.First()].ToString());
            writer.WriteEndElement();
        }

        private IDictionary<INode, int> MapNodes()
        {
            Dictionary<INode, int> nodeMapping = new Dictionary<INode, int>();
            int counter = 0;

            foreach (var scene in _projectService.Scenes)
            {
                foreach (var node in scene.Nodes)
                {
                    if (node is NodeTodo) nodeMapping[node] = counter;
                    else nodeMapping[node] = counter++;
                }
            }

            return nodeMapping;
        }

        public void Run(int startNode)
        {
            if (string.IsNullOrWhiteSpace(_executablePath))
            {
                OpenFileDialog dialog = new OpenFileDialog
                {
                    Filter = "Angel Wings Executable|AngelStory.exe"
                };

                if (dialog.ShowDialog().GetValueOrDefault())
                {
                    _executablePath = dialog.FileName;
                }
            }

            var startInfo = new ProcessStartInfo(_executablePath, $"-n {startNode}")
            {
                WorkingDirectory = Path.GetDirectoryName(_executablePath)
            };

            Process.Start(startInfo);
        }
    }
}
