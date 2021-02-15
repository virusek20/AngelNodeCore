using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Xml;
using AngelNode.Extension;
using AngelNode.Model;
using AngelNode.Model.Message;
using AngelNode.Model.Node;
using AngelNode.Model.Project;
using AngelNode.Service.Interface;
using GalaSoft.MvvmLight.Messaging;
using NuGet.Versioning;

namespace AngelNode.Service.Implementations.ProjectXML
{
    public class LocalProjectService : IProjectService
    {
        public struct NodeLocation
        {
            public int SceneId;
            public int NodeId;

            public static implicit operator NodeLocation((int sceneId, int nodeId) v)
            {
                return new NodeLocation
                {
                    NodeId = v.nodeId,
                    SceneId = v.sceneId
                };
            }

            public void Deconstruct(out int sceneId, out int nodeId)
            {
                sceneId = SceneId;
                nodeId = NodeId;
            }
        }

        public Project CurrentProject { get; } = new Project();
        public string ProjectFilePath { get; private set; }

        public ObservableCollection<Variable> Variables { get; } = new ObservableCollection<Variable>();
        public ObservableCollection<Character> Characters { get; } = new ObservableCollection<Character>();
        public ObservableCollection<Scene> Scenes { get; } = new ObservableCollection<Scene>();

        public LocalProjectService()
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Length <= 1) return;

            if (File.Exists(args[1])) Load(args[1]);
        }

        public void Load(string path)
        {
            Messenger.Default.Send(new TabCloseMessage
            {
                TabCloseType = TabCloseMessage.TabType.All
            });

            if (!File.Exists(path)) throw new FileNotFoundException("Project file could not be found.", path);

            ProjectFilePath = path;
            CurrentProject.Path = Path.GetDirectoryName(path);

            var doc = new XmlDocument();
            doc.Load(path);

            // General project
            CurrentProject.Path = Path.GetDirectoryName(Path.GetFullPath(path));
            CurrentProject.Name = doc.DocumentElement?.Attributes["Name"]?.Value ?? "MISSING PROJECT NAME";

            var parsedVersion = SemanticVersion.TryParse(doc.DocumentElement?.Attributes["Version"]?.Value ?? "0.1.0", out SemanticVersion version);
            CurrentProject.Version = !parsedVersion ? new SemanticVersion(0, 1, 0) : version;

            // Variables
            Variables.Clear();
            Variables.AddRange(LoadVariables(doc));

            //Characters
            Characters.Clear();
            Characters.AddRange(LoadCharacters(doc));

            //Scenes
            Scenes.Clear();
            Scenes.AddRange(LoadScenes(doc));
        }

        private IEnumerable<Variable> LoadVariables(XmlDocument document)
        {
            return document.SelectNodes("//Variable")
                ?.Cast<XmlNode>()
                .Select(variableNode =>
                {
                    var variable =  new Variable
                    {
                        Name = variableNode.Attributes?["Name"]?.Value ?? string.Empty
                    };

                    _ = bool.TryParse(variableNode.Attributes?["HighlightChanges"]?.Value, out bool highlight);
                    variable.HighlightChanges = highlight;

                    _ = bool.TryParse(variableNode.Attributes?["Binary"]?.Value, out bool binary);
                    variable.Binary = binary;

                    return variable;
                });
        }

        private IEnumerable<Character> LoadCharacters(XmlDocument document)
        {
            return document.SelectNodes("//Character")?.Cast<XmlNode>().Select(characterNode =>
            {
                var character = new Character
                {
                    Name = characterNode.Attributes?["Name"]?.Value ?? "MISSING CHARACTER NAME",
                    Height = int.Parse(characterNode.Attributes?["Height"]?.Value ?? "0"),
                    Pitch = float.Parse(characterNode.Attributes?["Pitch"]?.Value ?? "1", CultureInfo.InvariantCulture),
                    Color = (Color) (ColorConverter.ConvertFromString(characterNode.Attributes?["Color"]?.Value) ?? Color.FromArgb(255, 149, 228, 246)),
                    PhoneNumber = characterNode.Attributes?["PhoneNumber"]?.Value ?? string.Empty
                };

                var phonePicturePath = characterNode.Attributes?["PhonePicture"]?.Value;
                if (!string.IsNullOrWhiteSpace(phonePicturePath))
                {
                    if (!File.Exists(CurrentProject.GetResourceFullPath(phonePicturePath))) throw new FileNotFoundException("Resource file could not be located.", phonePicturePath);
                    character.PhonePicture = new Model.Resource.File(CurrentProject.GetResourceFullPath(phonePicturePath));
                }

                foreach (var outfit in LoadOutfits(characterNode)) character.Outfits.Add(outfit);
                foreach (var pose in LoadPoses(characterNode, character))
                {
                    if (pose.Relative) character.OutfitPoses.Add(pose);
                    else character.SharedPoses.Add(pose);
                }

                var defaultPoseIndex = characterNode.Attributes?["DefaultPose"]?.Value;
                if (defaultPoseIndex != null)
                {
                    var defaultPoseIndexNumber = int.Parse(defaultPoseIndex);

                    if (defaultPoseIndexNumber < 0 || defaultPoseIndexNumber >= character.Poses.Count) throw new InvalidDataException();
                    character.DefaultPose = character.Poses[defaultPoseIndexNumber];
                }

                var showcasePoseIndex = characterNode.Attributes?["ShowcasePose"]?.Value;
                if (showcasePoseIndex != null)
                {
                    var showcasePoseIndexNumber = int.Parse(showcasePoseIndex);

                    if (showcasePoseIndexNumber < 0 || showcasePoseIndexNumber >= character.Poses.Count) throw new InvalidDataException();
                    character.ShowcasePose = character.Poses[showcasePoseIndexNumber];
                }

                return character;
            }) ?? new List<Character>();
        }

        private IEnumerable<Pose> LoadPoses(XmlNode characterNode, Character character)
        {
            return characterNode.SelectNodes(".//Pose")
                       ?.Cast<XmlNode>()
                        .Select(poseNode =>
                        {
                            var pose = new Pose();
                            var posePath = poseNode.Attributes?["Path"]?.Value;

                            if (posePath == null)
                            {
                                pose.Relative = true;
                                posePath = poseNode.Attributes?["RelativePath"]?.Value;
                                if (posePath == null) throw new InvalidDataException();

                                var outfit = character.Outfits.FirstOrDefault();
                                if (outfit == null) throw new InvalidDataException("Attempted to load relative pose with no outfits.");

                                var fullPath = outfit.Directory.Path + "\\" + posePath;
                                if (!File.Exists(CurrentProject.GetResourceFullPath(fullPath))) throw new FileNotFoundException("Relative resource file could not be located.", fullPath);

                                pose.File = new Model.Resource.File(fullPath);
                            }
                            else 
                            {
                                pose.Relative = false;
                                if (!File.Exists(CurrentProject.GetResourceFullPath(posePath))) throw new FileNotFoundException("Absolute resource file could not be located.", posePath);

                                pose.File = new Model.Resource.File(CurrentProject.GetResourceFullPath(posePath));
                            }

                            pose.Name = Path.GetFileNameWithoutExtension(posePath);

                            return pose;
                        }) ?? new List<Pose>();
        }

        private IEnumerable<Outfit> LoadOutfits(XmlNode characterNode)
        {
            return characterNode.SelectNodes(".//Outfit")
                       ?.Cast<XmlNode>()
                        .Select(outfitNode =>
                        {
                            var outfit = new Outfit();
                            var outfitPath = outfitNode.Attributes?["Path"]?.Value;

                            if (outfitPath == null) throw new InvalidDataException();
                            if (!Directory.Exists(CurrentProject.GetResourceFullPath(outfitPath))) throw new DirectoryNotFoundException($"Resource directory '{outfitPath}' could not be located.");

                            var info = new DirectoryInfo(CurrentProject.GetResourceFullPath(outfitPath));
                            outfit.Directory = new Model.Resource.Directory(info);
                            outfit.Name = info.Name;

                            return outfit;
                        }) ?? new List<Outfit>();
        }

        private IEnumerable<Scene> LoadScenes(XmlDocument document)
        {
            Dictionary<DialogueResponse, XmlNode> responseNodes = new Dictionary<DialogueResponse, XmlNode>();
            Dictionary<NodeJump, XmlNode> jumpNodes = new Dictionary<NodeJump, XmlNode>();
            Dictionary<NodeVariableJump, XmlNode> conditionalJumpNodes = new Dictionary<NodeVariableJump, XmlNode>();
            Dictionary<NodeCall, XmlNode> callNodes = new Dictionary<NodeCall, XmlNode>();

            var scenes = document.SelectNodes("//Scene")
                ?.Cast<XmlNode>()
                .Select(n => {
                    return new Scene(n.Attributes["Name"].Value, n.Attributes["Tag"]?.Value ?? string.Empty, LoadNodes(n, responseNodes, jumpNodes, conditionalJumpNodes, callNodes))
                    {
                        IsStartpoint = n.Attributes["StartpointName"] != null,
                        StartpointName = n.Attributes["StartpointName"]?.Value ?? string.Empty
                    };
                })
                .ToList() ?? new List<Scene>();

            ResolveNodeIds(responseNodes, scenes);
            ResolveNodeIds(jumpNodes, scenes);
            ResolveNodeIds(conditionalJumpNodes, scenes);
            ResolveNodeIds(callNodes, scenes);
            return scenes;
        }

        private IEnumerable<INode> LoadNodes(XmlNode sceneNode, IDictionary<DialogueResponse, XmlNode> responseNodes, IDictionary<NodeJump, XmlNode> jumpNodes, IDictionary<NodeVariableJump, XmlNode> conditionalJumpNodes, IDictionary<NodeCall, XmlNode> callNodes)
        {
            List<INode> nodes = new List<INode>();

            foreach (var node in sceneNode.SelectSingleNode(".//Nodes").ChildNodes.Cast<XmlNode>())
            {
                switch (node.Name)
                {
                    case "NodeDialogue":
                        int characterDialogueRef = int.Parse(node.Attributes?["Character"]?.Value ?? "-1");

                        nodes.Add(new NodeDialogue
                        {
                            Text = node.InnerText,
                            Character = characterDialogueRef == -1 ? null : Characters[characterDialogueRef]
                        });
                        break;
                    case "NodeChangeBackground":
                        var backgroundPath = node.Attributes?["Path"]?.Value;

                        nodes.Add(new NodeChangeBackground
                        {
                            Background = backgroundPath == null ? null : new Model.Resource.File(CurrentProject.GetResourceFullPath(backgroundPath)),
                            TransitionType = (NodeChangeBackground.BackgroundTransitionTypeEnum)Enum.Parse(typeof(NodeChangeBackground.BackgroundTransitionTypeEnum), node.Attributes["TransitionType"].Value),
                            TransitionSpeed = (NodeChangeBackground.BackgroundTransitionSpeedEnum)Enum.Parse(typeof(NodeChangeBackground.BackgroundTransitionSpeedEnum), node.Attributes["TransitionSpeed"].Value)
                        });
                        break;
                    case "NodeChangePose":
                        int characterPoseRef = int.Parse(node.Attributes?["Character"]?.Value ?? "-1");
                        int poseRef = int.Parse(node.Attributes?["Pose"]?.Value ?? "-1");
                        var poseCharacter = characterPoseRef == -1 ? null : Characters[characterPoseRef];

                        nodes.Add(new NodeChangePose
                        {
                            Character = poseCharacter,
                            Pose = poseRef == -1 || poseCharacter == null ? null : poseCharacter?.Poses[poseRef]
                        });
                        break;
                    case "NodeMovement":
                        int characterMoveRef = int.Parse(node.Attributes?["Character"]?.Value ?? "-1");

                        nodes.Add(new NodeMovement
                        {
                            MovementDirection = (NodeMovement.MovementDirectionEnum)Enum.Parse(typeof(NodeMovement.MovementDirectionEnum), node.Attributes["Direction"].Value),
                            MovementType = (NodeMovement.MovementTypeEnum)Enum.Parse(typeof(NodeMovement.MovementTypeEnum), node.Attributes["Type"].Value),
                            MovementAnimation = (NodeMovement.MovementAnimationEnum)Enum.Parse(typeof(NodeMovement.MovementAnimationEnum), node.Attributes["Animation"].Value),
                            Character = characterMoveRef == -1 ? null : Characters[characterMoveRef]
                        });
                        break;
                    case "NodeResponseDialogue":
                        int characterRefResponse = int.Parse(node.Attributes?["Character"]?.Value ?? "-1");

                        var responseDialogue = new NodeResponseDialogue
                        {
                            Character = characterRefResponse == -1 ? null : Characters[characterRefResponse],
                            Dialogue = node.SelectSingleNode(".//Dialogue")?.InnerText
                        };

                        foreach (var responseNode in node.SelectNodes(".//Response").Cast<XmlNode>())
                        {
                            var response = new DialogueResponse { Text = responseNode.InnerText };
                            responseDialogue.ResponseMap.Add(response);
                            responseNodes.Add(response, responseNode);
                        }
                        nodes.Add(responseDialogue);
                        break;
                    case "NodeJump":
                        var jumpNode = new NodeJump();
                        nodes.Add(jumpNode);
                        jumpNodes.Add(jumpNode, node);
                        break;
                    case "NodeVariableJump":
                        int variableJumpRef = int.Parse(node.Attributes?["Variable"]?.Value ?? "-1");
                        var variableJumpNode = new NodeVariableJump
                        {
                            Variable = variableJumpRef == -1 ? null : Variables[variableJumpRef],
                            Type = (NodeVariableJump.ComparisonType)Enum.Parse(typeof(NodeVariableJump.ComparisonType), node.Attributes["ComparisonType"].Value),
                            Value = int.Parse(node.Attributes["Value"].Value)
                        };
                        nodes.Add(variableJumpNode);
                        conditionalJumpNodes.Add(variableJumpNode, node);
                        break;
                    case "NodeSetVariable":
                        int variableSetRef = int.Parse(node.Attributes?["Variable"]?.Value ?? "-1");
                        var setVariableNode = new NodeSetVariable
                        {
                            Variable = variableSetRef == -1 ? null : Variables[variableSetRef],
                            Type = (NodeSetVariable.SetType)Enum.Parse(typeof(NodeSetVariable.SetType), node.Attributes["SetType"].Value),
                            Value = int.Parse(node.Attributes["Value"].Value)
                        };
                        nodes.Add(setVariableNode);
                        break;
                    case "NodePlaySound":
                        var soundPath = node.Attributes?["Path"]?.Value;

                        var playSoundNode = new NodePlaySound
                        {
                            Sound = soundPath == null ? null : new Model.Resource.File(CurrentProject.GetResourceFullPath(soundPath)),
                            SoundType = (NodePlaySound.SoundTypeEnum)Enum.Parse(typeof(NodePlaySound.SoundTypeEnum), node.Attributes["SoundType"].Value),
                            StartTime = float.Parse(node.Attributes["StartTime"].Value, CultureInfo.InvariantCulture),
                            Volume = float.Parse(node.Attributes["Volume"]?.Value ?? "1.0", CultureInfo.InvariantCulture)
                        };
                        nodes.Add(playSoundNode);
                        break;
                    case "NodeShake":
                        var shakeNode = new NodeShake
                        {
                            ShakeBackground = bool.Parse(node.Attributes["ShakeBackground"].Value),
                            ShakeCharacters = bool.Parse(node.Attributes["ShakeCharacters"].Value),
                            Amplitude = float.Parse(node.Attributes["Amplitude"].Value, CultureInfo.InvariantCulture),
                            Duration = float.Parse(node.Attributes["Duration"].Value, CultureInfo.InvariantCulture)
                        };
                        nodes.Add(shakeNode);
                        break;
                    case "NodeWait":
                        var waitNode = new NodeWait
                        {
                            Duration = float.Parse(node.Attributes["Duration"].Value, CultureInfo.InvariantCulture)
                        };
                        nodes.Add(waitNode);
                        break;
                    case "NodeFadeMusic":
                        var fadeNode = new NodeFadeMusic
                        {
                            FadeTime = float.Parse(node.Attributes["FadeTime"].Value, CultureInfo.InvariantCulture),
                            AudioFadeType = (NodeFadeMusic.AudioFadeTypeEnum)Enum.Parse(typeof(NodeFadeMusic.AudioFadeTypeEnum), node.Attributes["Type"].Value)
                        };
                        nodes.Add(fadeNode);
                        break;
                    case "NodeEvent":
                        var eventNode = new NodeEvent
                        {
                            EventType = (NodeEvent.EventTypeEnum)Enum.Parse(typeof(NodeEvent.EventTypeEnum), node.Attributes["EventType"].Value)
                        };

                        switch (eventNode.EventType)
                        {
                            case NodeEvent.EventTypeEnum.Call:
                                int characterRefEvent = int.Parse(node.Attributes?["Caller"]?.Value ?? "-1");
                                eventNode.Caller = characterRefEvent == -1 ? null : Characters[characterRefEvent];
                                eventNode.IsPlayerInitiated = bool.Parse(node.Attributes?["IsPlayerInitiated"]?.Value ?? "false");
                                eventNode.IsOngoing = bool.Parse(node.Attributes?["IsOngoing"]?.Value ?? "false");
                                eventNode.PhoneTime = node.Attributes?["PhoneTime"]?.Value ?? string.Empty;
                                break;
                            case NodeEvent.EventTypeEnum.Contacts:
                                break;
                            case NodeEvent.EventTypeEnum.Custom:
                                eventNode.EventName = node.Attributes?["EventName"]?.Value ?? string.Empty;
                                break;
                            case NodeEvent.EventTypeEnum.HookBrackets:
                                eventNode.Text = node.Attributes?["Text"]?.Value ?? string.Empty;
                                eventNode.Duration = float.Parse(node.Attributes["Duration"].Value, CultureInfo.InvariantCulture);
                                eventNode.BlackOnWhite = bool.Parse(node.Attributes["BlackOnWhite"]?.Value ?? "false");
                                break;
                        }

                        nodes.Add(eventNode);
                        break;
                    case "NodeTodo":
                        nodes.Add(new NodeTodo
                        {
                            Note = node.InnerText
                        });
                        break;
                    case "NodeCall":
                        var callNode = new NodeCall();
                        nodes.Add(callNode);
                        callNodes.Add(callNode, node);
                        break;
                    case "NodeRet":
                        nodes.Add(new NodeRet());
                        break;
                    case "NodePhone":
                        int characterRefPhone = int.Parse(node.Attributes?["Character"]?.Value ?? "-1");

                        var phoneNode = new NodePhone
                        {
                            Character = characterRefPhone == -1 ? null : Characters[characterRefPhone],
                            Time = node.Attributes["Time"].Value
                        };

                        foreach (var messageNode in node.SelectNodes(".//Message").Cast<XmlNode>())
                        {
                            var message = new NodePhone.PhoneMessage
                            { 
                                Text = messageNode.InnerText,
                                PlayerMade = bool.Parse(messageNode.Attributes["PlayerMade"].Value),
                                Skip = bool.Parse(messageNode.Attributes["Skip"].Value),
                            };
                            phoneNode.PhoneMessages.Add(message);
                        }
                        nodes.Add(phoneNode);
                        break;
                    case "NodeLua":
                        nodes.Add(new NodeLua
                        {
                            Script = node.InnerText
                        });
                        break;
                    case "NodeAchievement":
                        nodes.Add(new NodeAchievement
                        {
                            Name = node.Attributes?["Name"]?.Value ?? string.Empty
                        });
                        break;
                    case "NodeOutfitUnlocked":
                        int characterOutfitUnlockRef = int.Parse(node.Attributes?["Character"]?.Value ?? "-1");
                        int outfitRef = int.Parse(node.Attributes?["Outfit"]?.Value ?? "-1");
                        var outfitCharacter = characterOutfitUnlockRef == -1 ? null : Characters[characterOutfitUnlockRef];

                        nodes.Add(new NodeOutfitUnlocked
                        {
                            Character = outfitCharacter,
                            Outfit = outfitRef == -1 || outfitCharacter == null ? null : outfitCharacter?.Outfits[outfitRef]
                        });
                        break;
                    case "NodeRouteCompleted":
                        nodes.Add(new NodeRouteCompleted
                        {
                            Name = node.Attributes?["Name"]?.Value ?? string.Empty
                        });
                        break;
                }
            }

            return nodes;
        }

        private void ResolveNodeIds(IDictionary<DialogueResponse, XmlNode> responseNodes, IList<Scene> scenes)
        {
            foreach (var responsePair in responseNodes)
            {
                var sceneId = responsePair.Value.Attributes?["TargetScene"]?.Value;
                var nodeId = responsePair.Value.Attributes?["TargetNode"]?.Value;
                if (sceneId == null || nodeId == null) continue;

                var targetScene = scenes[int.Parse(sceneId)];
                var targetNode = targetScene.Nodes[int.Parse(nodeId)];
                responsePair.Key.Target = targetNode;
            }
        }

        private void ResolveNodeIds(IDictionary<NodeVariableJump, XmlNode> conditionalJumpNodes, IList<Scene> scenes)
        {
            foreach (var conditionalJumpNode in conditionalJumpNodes)
            {
                var sceneId = conditionalJumpNode.Value.Attributes?["TargetScene"]?.Value;
                var nodeId = conditionalJumpNode.Value.Attributes?["TargetNode"]?.Value;
                if (sceneId == null || nodeId == null) continue;

                var targetScene = scenes[int.Parse(sceneId)];
                var targetNode = targetScene.Nodes[int.Parse(nodeId)];
                conditionalJumpNode.Key.Target = targetNode;
            }
        }

        private void ResolveNodeIds(IDictionary<NodeJump, XmlNode> jumpNodes, IList<Scene> scenes)
        {
            foreach (var jumpNode in jumpNodes)
            {
                var sceneId = jumpNode.Value.Attributes?["TargetScene"]?.Value;
                var nodeId = jumpNode.Value.Attributes?["TargetNode"]?.Value;
                if (sceneId == null || nodeId == null) continue;

                var targetScene = scenes[int.Parse(sceneId)];
                var targetNode = targetScene.Nodes[int.Parse(nodeId)];
                jumpNode.Key.Target = targetNode;
            }
        }

        private void ResolveNodeIds(IDictionary<NodeCall, XmlNode> callNodes, IList<Scene> scenes)
        {
            foreach (var callNode in callNodes)
            {
                var sceneId = callNode.Value.Attributes?["TargetScene"]?.Value;
                if (sceneId == null) continue;

                var targetScene = scenes[int.Parse(sceneId)];
                callNode.Key.Target = targetScene;
            }
        }

        /// <summary>
        /// Saves the currently opened project to a .xml file
        /// </summary>
        /// <param name="path">Path to file used for saving | Use null for default</param>
        public void Save(string path)
        {
            if (path == null) path = ProjectFilePath;

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true
            };

            using XmlWriter writer = XmlWriter.Create(path, settings);

            writer.WriteStartDocument();
            writer.WriteStartElement("Project");
            writer.WriteAttributeString("Name", CurrentProject.Name ?? string.Empty);
            writer.WriteAttributeString("Version", CurrentProject.Version.ToString());

            writer.WriteStartElement("Variables");
            foreach (var variable in Variables) SaveVariable(writer, variable);
            writer.WriteEndElement();

            writer.WriteStartElement("Characters");
            foreach (var character in Characters) SaveCharacter(writer, character);
            writer.WriteEndElement();

            /*
            writer.WriteStartElement("Backgrounds");
            var backgrounds = Scenes.SelectMany(s => s.Nodes)
                .OfType<NodeChangeBackground>()
                .Select(ncb => ncb.Background?.Path)
                .ToHashSet();

            foreach (var background in backgrounds) SaveBackground(writer, background);
            writer.WriteEndElement();
            */

            writer.WriteStartElement("Scenes");
            var nodeMap = MapNodes(Scenes);
            var nodeVisitor = new XmlSaveNodeVisitor(this, writer, nodeMap, Scenes);
            foreach (var scene in Scenes) SaveScene(writer, scene, nodeVisitor);
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        private void SaveVariable(XmlWriter writer, Variable variable)
        {
            writer.WriteStartElement("Variable");
            writer.WriteAttributeString("Name", variable.Name ?? string.Empty);
            writer.WriteAttributeString("HighlightChanges", variable.HighlightChanges.ToString());
            writer.WriteAttributeString("Binary", variable.Binary.ToString());
            writer.WriteEndElement();
        }

        private void SaveCharacter(XmlWriter writer, Character character)
        {
            writer.WriteStartElement("Character");
            writer.WriteAttributeString("Name", character.Name ?? string.Empty);
            writer.WriteAttributeString("Height", character.Height.ToString());
            writer.WriteAttributeString("Pitch", character.Pitch.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("Color", $"#{character.Color.A:X2}{character.Color.R:X2}{character.Color.G:X2}{character.Color.B:X2}");
            writer.WriteAttributeString("PhoneNumber", character.PhoneNumber ?? string.Empty);
            if (character.DefaultPose != null) writer.WriteAttributeString("DefaultPose", character.Poses.IndexOf(character.DefaultPose).ToString());
            if (character.ShowcasePose != null) writer.WriteAttributeString("ShowcasePose", character.Poses.IndexOf(character.ShowcasePose).ToString());
            if (character.PhonePicture != null) writer.WriteAttributeString("PhonePicture", GetRelativePath(character.PhonePicture.Path));

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
            if (pose.Relative) writer.WriteAttributeString("RelativePath", pose.File.Name);
            else writer.WriteAttributeString("Path", GetRelativePath(pose.File.Path));
            writer.WriteEndElement();
        }

        private void SaveOutfit(XmlWriter writer, Outfit outfit)
        {
            writer.WriteStartElement("Outfit");
            writer.WriteAttributeString("Path", GetRelativePath(outfit.Directory.Path));
            writer.WriteEndElement();
        }

        private void SaveScene(XmlWriter writer, Scene scene, INodeVisitor visitor)
        {
            writer.WriteStartElement("Scene");
            writer.WriteAttributeString("Name", scene.Name ?? string.Empty);
            writer.WriteAttributeString("Tag", scene.Tag ?? string.Empty);

            if (scene.IsStartpoint) writer.WriteAttributeString("StartpointName", scene.StartpointName ?? string.Empty);

            writer.WriteStartElement("Nodes");
            foreach (var node in scene.Nodes) node.Accept(visitor);
            writer.WriteEndElement();

            writer.WriteEndElement();
        }

        private IDictionary<INode, NodeLocation> MapNodes(IEnumerable<Scene> scenes)
        {
            Dictionary<INode, NodeLocation> nodeNumbers = new Dictionary<INode, NodeLocation>();

            int sceneId = 0;
            foreach (var scene in scenes)
            {
                int nodeId = 0;
                foreach (var node in scene.Nodes)
                {
                    nodeNumbers[node] = (sceneId, nodeId++);
                }

                sceneId++;
            }

            return nodeNumbers;
        }

        public void CreateNew(string path)
        {
            if (!Directory.Exists(path)) throw new DirectoryNotFoundException();
            var newProjectFilePath = Path.Combine(path, "project.xml");
            if (File.Exists(newProjectFilePath)) throw new IOException($"File '{newProjectFilePath}' already exists");

            CurrentProject.Name = "New Project";
            CurrentProject.Path = path;
            CurrentProject.Version = new SemanticVersion(0, 1, 0);

            ProjectFilePath = newProjectFilePath;
            Directory.CreateDirectory(Path.Combine(path, "resources"));

            Characters.Clear();
            Scenes.Clear();
            Variables.Clear();

            Save(ProjectFilePath);
        }

        public string GetRelativePath(string fullPath)
        {
            return fullPath.Replace(CurrentProject.ResourcesPath, "").TrimStart('\\');
        }

        public ProjectReport Analyze()
        {
            var report = new ProjectReport();
            var nodeMap = MapNodes(Scenes);

            if (string.IsNullOrWhiteSpace(CurrentProject.Name)) report.AddMessage(ProjectReportMessage.MessageSeverity.Warning, "Project has no name.");

            foreach (var variable in Variables)
            {
                if (string.IsNullOrWhiteSpace(variable.Name)) report.AddMessage(ProjectReportMessage.MessageSeverity.Warning, "A variable has no name.", variable);
            }

            var poseFiles = new HashSet<Model.Resource.File>();
            var nodeVisitor = new AnalysisNodeVisitor(report, nodeMap, poseFiles, Scenes);

            foreach (var character in Characters)
            {
                if (string.IsNullOrWhiteSpace(character.Name)) report.AddMessage(ProjectReportMessage.MessageSeverity.Warning, "A character has no name.", character);
                if (character.DefaultPose == null) report.AddMessage(ProjectReportMessage.MessageSeverity.Warning, "A character has no default pose.", character);
                if (character.ShowcasePose == null && character.Outfits.Count > 0) report.AddMessage(ProjectReportMessage.MessageSeverity.Warning, "A character has no showcase pose.", character);
                if (character.Poses.Count == 0) report.AddMessage(ProjectReportMessage.MessageSeverity.Warning, "A character has no poses.", character);
                // TODO: Maybe check missing profile pic?

                foreach (var pose in character.Poses) poseFiles.Add(pose.File);
            }

            var visitedScenes = new HashSet<Scene>();
            foreach (var scene in Scenes)
            {
                if (string.IsNullOrWhiteSpace(scene.Name)) report.AddMessage(ProjectReportMessage.MessageSeverity.Warning, "A scene has no name.", scene);
                if (scene.Nodes.Count == 0 || (scene.Nodes.Count == 1 && scene.Nodes.First() is NodeRet)) report.AddMessage(ProjectReportMessage.MessageSeverity.Info, "A scene has no nodes.", scene);

                foreach (var node in scene.Nodes)
                {
                    node.Accept(nodeVisitor);
                    if (node is NodeCall nc) visitedScenes.Add(nc.Target);
                }

                if (scene.Nodes.Count == 0 || !(scene.Nodes.Last() is NodeRet)) report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A scene never returns.", scene);
            }

            foreach (var scene in Scenes)
            {
                if (!scene.IsStartpoint && !visitedScenes.Contains(scene)) report.AddMessage(ProjectReportMessage.MessageSeverity.Warning, "A scene is not reachable.", scene);
            }

            // TODO: Fix
            /*
            OmniInterpreter interpreter = new OmniInterpreter(report);
            interpreter.Interpret(Scenes);
            var poses = interpreter.UsedPoses.ToList();
            var characters = interpreter.UsedCharacters.ToList();
            var variables = interpreter.UsedVariables.ToList();

            foreach (var character in Characters)
            {
                if (!characters.Contains(character))
                {
                    report.AddMessage(ProjectReportMessage.MessageSeverity.Warning, $"A character is never used '{character.Name}'.", character);
                    continue;
                }

                if (character.DefaultPose != null) poses.Add(character.DefaultPose);

                foreach (var pose in character.Poses)
                {
                    if (!poses.Contains(pose)) report.AddMessage(ProjectReportMessage.MessageSeverity.Warning, $"A character has an unused pose '{pose.Name}'.", character);
                }
            }

            foreach (var variable in Variables)
            {
                if (!variables.Contains(variable)) report.AddMessage(ProjectReportMessage.MessageSeverity.Warning, $"A variable is never used '{variable.Name}'.", variable);
            }
            */

            CurrentProject.ProjectReport = report;
            return report;
        }
    }
}