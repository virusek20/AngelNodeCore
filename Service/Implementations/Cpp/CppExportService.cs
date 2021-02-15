using System;
using AngelNode.Service.Interface;
using CommonServiceLocator;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AngelNode.Model;
using AngelNode.Model.Node;
using AngelNode.Model.Resource;

namespace AngelNode.Service.Implementations.Cpp
{
    public class CppExportService : IExportService
    {
        private readonly IProjectService _projectService;

        public CppExportService()
        {
            _projectService = SimpleIoc.Default.GetInstance<IProjectService>();
        }

        public void Export(string path)
        {
            StringBuilder output = new StringBuilder();
            
            var variableMapping = MapVariables();
            var resourceMapping = ExportResources(output);
            var characterMapping = ExportCharacters(output, resourceMapping);
            var nodeMapping = MapNodes();
            var sceneMapping = ExportScenes(output, nodeMapping, resourceMapping, characterMapping, variableMapping);

            System.IO.File.WriteAllText(path, output.ToString());
        }

        public int GetNodeNumber(INode node)
        {
            throw new NotImplementedException();
        }

        public string DefaultExtension { get; } = ".cpp";
        public string FileFilter { get; } = "C++ code files|*.cpp";

        private IDictionary<Variable, string> MapVariables()
        {
            var variableMapping = new Dictionary<Variable, string>();

            foreach (var variable in _projectService.Variables)
            {
                var guid = GenerateIdentifier();
                variableMapping.Add(variable, guid);
            }

            return variableMapping;
        }

        private IDictionary<Character, string> ExportCharacters(StringBuilder builder, IDictionary<File, string> resourceMapping)
        {
            builder.AppendLine("// CHARACTERS");
            var characterMapping = new Dictionary<Character, string>();

            foreach (var character in _projectService.Characters)
            {
                var guid = GenerateIdentifier();
                characterMapping.Add(character, guid);

                builder.AppendLine($"// '{character.Name}' as '{guid}'");
                builder.AppendLine($"Character* {guid} = new Character();");
                builder.AppendLine($"{guid}->Name = \"{character.Name ?? string.Empty}\";");
                builder.AppendLine($"{guid}->Height = {character.Height};");

                foreach (var pose in character.Poses)
                {
                    if (resourceMapping.ContainsKey(pose.File)) builder.AppendLine($"{guid}->AddPose(\"{pose.Name}\", \"{resourceMapping[pose.File]}\");");
                }

                if (character.DefaultPose != null) builder.AppendLine($"{guid}->SetDefaultPose(\"{character.DefaultPose.Name}\");");
                builder.AppendLine($"Characters.push_back({guid});");
            }

            return characterMapping;
        }

        private IDictionary<File, string> ExportResources(StringBuilder builder)
        {
            builder.AppendLine("// RESOURCES");
            var resourceMapping = new Dictionary<File, string>();

            foreach (var character in _projectService.Characters)
            {
                if (character.DefaultPose != null && !resourceMapping.ContainsKey(character.DefaultPose.File)) resourceMapping.Add(character.DefaultPose.File, GenerateIdentifier());
            }

            foreach (var scene in _projectService.Scenes)
            {
                foreach (var node in scene.Nodes)
                {
                    switch (node)
                    {
                        case NodeChangeBackground ncb:
                            if (!resourceMapping.ContainsKey(ncb.Background)) resourceMapping.Add(ncb.Background, GenerateIdentifier());
                            break;
                        case NodePlaySound nps:
                            if (!resourceMapping.ContainsKey(nps.Sound)) resourceMapping.Add(nps.Sound, GenerateIdentifier());
                            break;
                        case NodeChangePose ncp:
                            if (!resourceMapping.ContainsKey(ncp.Pose.File)) resourceMapping.Add(ncp.Pose.File, GenerateIdentifier());
                            break;
                    }
                }
            }

            var resourceTypes = resourceMapping.GroupBy(pair => pair.Key.GuessType());
            foreach (var resourceType in resourceTypes)
            {
                switch (resourceType.Key)
                {
                    case ResourceType.Image:
                        foreach (var resource in resourceType)
                        {
                            builder.AppendLine($"CRUCIBLE::ResourceManager::LoadTexture(\"{resource.Value}\", \"resource/{_projectService.GetRelativePath(resource.Key.Path).Replace('\\', '/')}\");");
                        }
                        break;
                    case ResourceType.Sound:
                        foreach (var resource in resourceType)
                        {
                            builder.AppendLine($"CRUCIBLE::ResourceManager::LoadSound(\"{resource.Value}\", \"resource/{_projectService.GetRelativePath(resource.Key.Path).Replace('\\', '/')}\");");
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return resourceMapping;
        }

        private IDictionary<Scene, string> ExportScenes(StringBuilder builder, IDictionary<INode, int> nodeMapping, IDictionary<File, string> resourceMapping, IDictionary<Character, string> characterMapping, IDictionary<Variable, string> variableMapping)
        {
            builder.AppendLine("// SCENES");
            var sceneMapping = new Dictionary<Scene, string>();
            var nodeVisitor = new CppNodeVisitor(nodeMapping, resourceMapping, characterMapping, variableMapping);

            foreach (var scene in _projectService.Scenes)
            {
                var guid = GenerateIdentifier();
                sceneMapping.Add(scene, guid);

                builder.AppendLine($"// '{scene.Name}' as {guid}");

                foreach (var node in scene.Nodes) node.Accept(nodeVisitor);
            }

            builder.Append(nodeVisitor);
            return sceneMapping;
        }

        private IDictionary<INode, int> MapNodes()
        {
            Dictionary<INode, int> nodeMapping = new Dictionary<INode, int>();
            int counter = 0;

            foreach (var scene in _projectService.Scenes)
            {
                foreach (var node in scene.Nodes)
                {
                    nodeMapping[node] = counter++;
                }
            }

            return nodeMapping;
        }

        private string GenerateIdentifier()
        {
            return string.Concat(Guid.NewGuid().ToString("N").Select(c => (char)(c + 17)));
        }

        public void Run(int startNode)
        {
            throw new NotImplementedException();
        }
    }
}
