using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using AngelNode.Model.Node;
using AngelNode.Service.Interface;

namespace AngelNode.Service.Implementations.ExportXML
{
    public class XmlExportNodeVisitor : INodeVisitor
    {
        private readonly XmlWriter _writer;
        private readonly IProjectService _projectService;
        private readonly IDictionary<INode, int> _nodeMapping;

        public HashSet<string> Backgrounds { get; } = new HashSet<string>();
        public HashSet<string> Sounds { get; } = new HashSet<string>();

        public XmlExportNodeVisitor(IProjectService projectService, XmlWriter writer, IDictionary<INode, int> nodeMapping)
        {
            _projectService = projectService;
            _writer = writer;
            _nodeMapping = nodeMapping;
        }

        public void VisitNodeDialogue(NodeDialogue nd)
        {
            _writer.WriteStartElement("NodeDialogue");
            if (nd.Character != null) _writer.WriteAttributeString("Character", _projectService.Characters.IndexOf(nd.Character).ToString());
            _writer.WriteString(nd.Text ?? string.Empty);
            _writer.WriteEndElement();
        }

        public void VisitNodeFadeMusic(NodeFadeMusic ndm)
        {
            _writer.WriteStartElement("NodeFadeMusic");
            _writer.WriteAttributeString("FadeTime", ndm.FadeTime.ToString(CultureInfo.InvariantCulture));
            _writer.WriteAttributeString("Type", ndm.AudioFadeType.ToString());
            _writer.WriteEndElement();
        }

        public void VisitNodeChangeBackground(NodeChangeBackground ncb)
        {
            var resourcePath = "resource/" + _projectService.GetRelativePath(ncb.Background.Path).Replace('\\', '/').Replace(".png", ".DDS");
            Backgrounds.Add(resourcePath);

            _writer.WriteStartElement("NodeChangeBackground");
            if (ncb.Background != null) _writer.WriteAttributeString("Path", resourcePath);
            _writer.WriteAttributeString("TransitionType", ncb.TransitionType.ToString());
            _writer.WriteAttributeString("TransitionSpeed", ncb.TransitionSpeed.ToString());
            _writer.WriteEndElement();
        }

        public void VisitNodeChangePose(NodeChangePose ncp)
        {
            _writer.WriteStartElement("NodeChangePose");
            if (ncp.Character != null) _writer.WriteAttributeString("Character", _projectService.Characters.IndexOf(ncp.Character).ToString());
            if (ncp.Character != null && ncp.Pose != null) _writer.WriteAttributeString("Pose", ncp.Character.Poses.IndexOf(ncp.Pose).ToString());
            _writer.WriteEndElement();
        }

        public void VisitNodeJump(NodeJump nj)
        {
            _writer.WriteStartElement("NodeJump");
            if (nj.Target != null)
            {
                var nodeIdj = _nodeMapping[nj.Target];
                _writer.WriteAttributeString("TargetNode", nodeIdj.ToString());
            }
            _writer.WriteEndElement();
        }

        public void VisitNodeMovement(NodeMovement nm)
        {
            _writer.WriteStartElement("NodeMovement");
            _writer.WriteAttributeString("Direction", nm.MovementDirection.ToString());
            _writer.WriteAttributeString("Type", nm.MovementType.ToString());
            _writer.WriteAttributeString("Animation", nm.MovementAnimation.ToString());
            if (nm.Character != null) _writer.WriteAttributeString("Character", _projectService.Characters.IndexOf(nm.Character).ToString());
            _writer.WriteEndElement();
        }

        public void VisitNodePlaySound(NodePlaySound nps)
        {
            var resourcePath = "resource/" + _projectService.GetRelativePath(nps.Sound.Path).Replace('\\', '/');
            if (nps.SoundType == NodePlaySound.SoundTypeEnum.SFX) Sounds.Add(resourcePath);

            _writer.WriteStartElement("NodePlaySound");
            if (nps.Sound != null) _writer.WriteAttributeString("Path", resourcePath);
            _writer.WriteAttributeString("SoundType", nps.SoundType.ToString());
            _writer.WriteAttributeString("StartTime", nps.StartTime.ToString(CultureInfo.InvariantCulture));
            _writer.WriteAttributeString("Volume", nps.Volume.ToString(CultureInfo.InvariantCulture));
            _writer.WriteEndElement();
        }

        public void VisitNodeResponseDialogue(NodeResponseDialogue nrd)
        {
            _writer.WriteStartElement("NodeResponseDialogue");
            if (nrd.Character != null) _writer.WriteAttributeString("Character", _projectService.Characters.IndexOf(nrd.Character).ToString());

            _writer.WriteStartElement("Dialogue");
            _writer.WriteString(nrd.Dialogue ?? string.Empty);
            _writer.WriteEndElement();

            _writer.WriteStartElement("ResponseMap");
            foreach (var keyPair in nrd.ResponseMap)
            {
                _writer.WriteStartElement("Response");

                if (keyPair.Target != null && _nodeMapping.ContainsKey(keyPair.Target))
                {
                    var nodeId = _nodeMapping[keyPair.Target];
                    _writer.WriteAttributeString("TargetNode", nodeId.ToString());
                }

                _writer.WriteString(keyPair.Text ?? string.Empty);
                _writer.WriteEndElement();
            }
            _writer.WriteEndElement();
            _writer.WriteEndElement();
        }

        public void VisitNodeSetVariable(NodeSetVariable nsv)
        {
            _writer.WriteStartElement("NodeSetVariable");
            if (nsv.Variable != null) _writer.WriteAttributeString("Variable", nsv.Variable.Name);
            _writer.WriteAttributeString("Value", nsv.Value.ToString());
            _writer.WriteAttributeString("SetType", nsv.Type.ToString());
            _writer.WriteEndElement();
        }

        public void VisitNodeShake(NodeShake ns)
        {
            _writer.WriteStartElement("NodeShake");
            _writer.WriteAttributeString("ShakeBackground", ns.ShakeBackground.ToString());
            _writer.WriteAttributeString("ShakeCharacters", ns.ShakeCharacters.ToString());
            _writer.WriteAttributeString("Duration", ns.Duration.ToString(CultureInfo.InvariantCulture));
            _writer.WriteAttributeString("Amplitude", ns.Amplitude.ToString(CultureInfo.InvariantCulture));
            _writer.WriteEndElement();
        }

        public void VisitNodeVariableJump(NodeVariableJump nvj)
        {
            _writer.WriteStartElement("NodeVariableJump");
            if (nvj.Target != null)
            {
                var nodeIdvj = _nodeMapping[nvj.Target];
                _writer.WriteAttributeString("TargetNode", nodeIdvj.ToString());
            }
            if (nvj.Variable != null) _writer.WriteAttributeString("Variable", nvj.Variable.Name);
            _writer.WriteAttributeString("ComparisonType", nvj.Type.ToString());
            _writer.WriteAttributeString("Value", nvj.Value.ToString());
            _writer.WriteEndElement();
        }

        public void VisitNodeWait(NodeWait nw)
        {
            _writer.WriteStartElement("NodeWait");
            _writer.WriteAttributeString("Duration", nw.Duration.ToString(CultureInfo.InvariantCulture));
            _writer.WriteEndElement();
        }

        public void VisitNodeEvent(NodeEvent ne)
        {
            _writer.WriteStartElement("NodeEvent");
            _writer.WriteAttributeString("EventType", ne.EventType.ToString());

            switch (ne.EventType)
            {
                case NodeEvent.EventTypeEnum.Call:
                    if (ne.Caller != null) _writer.WriteAttributeString("Caller", _projectService.Characters.IndexOf(ne.Caller).ToString());
                    _writer.WriteAttributeString("IsPlayerInitiated", ne.IsPlayerInitiated.ToString());
                    _writer.WriteAttributeString("IsOngoing", ne.IsOngoing.ToString());
                    _writer.WriteAttributeString("PhoneTime", ne.PhoneTime ?? string.Empty);
                    break;
                case NodeEvent.EventTypeEnum.Contacts:
                    break;
                case NodeEvent.EventTypeEnum.CallEnd:
                    break;
                case NodeEvent.EventTypeEnum.Custom:
                    _writer.WriteAttributeString("EventName", ne.EventName);
                    break;
                case NodeEvent.EventTypeEnum.HookBrackets:
                    _writer.WriteAttributeString("Text", ne.Text);
                    _writer.WriteAttributeString("Duration", ne.Duration.ToString(CultureInfo.InvariantCulture));
                    _writer.WriteAttributeString("BlackOnWhite", ne.BlackOnWhite.ToString(CultureInfo.InvariantCulture));
                    break;
            }

            _writer.WriteEndElement();
        }

        public void VisitNodeTodo(NodeTodo nt)
        {
        }

        public void VisitNodeCall(NodeCall nc)
        {
            _writer.WriteStartElement("NodeCall");
            if (nc.Target != null)
            {
                var firstNode = nc.Target.Nodes.First();
                var nodeIdj = _nodeMapping[firstNode];
                _writer.WriteAttributeString("TargetNode", nodeIdj.ToString());
            }
            _writer.WriteEndElement();
        }

        public void VisitNodeRet(NodeRet nr)
        {
            _writer.WriteStartElement("NodeRet");
            _writer.WriteEndElement();
        }

        public void VisitNodePhone(NodePhone np)
        {
            _writer.WriteStartElement("NodePhone");
            if (np.Character != null) _writer.WriteAttributeString("Character", _projectService.Characters.IndexOf(np.Character).ToString());
            _writer.WriteAttributeString("Time", np.Time ?? string.Empty);

            foreach (var message in np.PhoneMessages)
            {
                _writer.WriteStartElement("Message");
                _writer.WriteAttributeString("Skip", message.Skip.ToString());
                _writer.WriteAttributeString("PlayerMade", message.PlayerMade.ToString());
                _writer.WriteString(message.Text ?? string.Empty);
                _writer.WriteEndElement();
            }
            _writer.WriteEndElement();
        }

        public void VisitNodeLua(NodeLua nl)
        {
            _writer.WriteStartElement("NodeLua");
            _writer.WriteString(nl.Script);
            _writer.WriteEndElement();
        }

        public void VisitNodeAchievement(NodeAchievement na)
        {
            _writer.WriteStartElement("NodeAchievement");
            _writer.WriteAttributeString("Name", na.Name);
            _writer.WriteEndElement();
        }

        public void VisitNodeRouteCompleted(NodeRouteCompleted nrc)
        {
            _writer.WriteStartElement("NodeRouteCompleted");
            _writer.WriteAttributeString("Name", nrc.Name);
            _writer.WriteEndElement();
        }

        public void VisitNodeOutfitUnlocked(NodeOutfitUnlocked nou)
        {
            _writer.WriteStartElement("NodeOutfitUnlocked");
            _writer.WriteAttributeString("Character", nou.Character.Name);
            _writer.WriteAttributeString("Outfit", nou.Outfit.Name);
            _writer.WriteEndElement();
        }
    }
}
