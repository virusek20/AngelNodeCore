using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using AngelNode.Model;
using AngelNode.Model.Node;
using AngelNode.Service.Interface;

namespace AngelNode.Service.Implementations.ProjectXML
{
    public class XmlSaveNodeVisitor : INodeVisitor
    {
        private readonly XmlWriter _writer;
        private readonly IProjectService _projectService;
        private readonly IDictionary<INode, LocalProjectService.NodeLocation> _nodeMap;
        private readonly IList<Scene> _scenes;

        public XmlSaveNodeVisitor(IProjectService projectService, XmlWriter writer, IDictionary<INode, LocalProjectService.NodeLocation> nodeMap, IList<Scene> scenes)
        {
            _projectService = projectService;
            _writer = writer;
            _nodeMap = nodeMap;
            _scenes = scenes;
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
            _writer.WriteStartElement("NodeChangeBackground");
            if (ncb.Background != null) _writer.WriteAttributeString("Path", _projectService.GetRelativePath(ncb.Background.Path));
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
            if (nj.Target != null && _nodeMap.ContainsKey(nj.Target))
            {
                var (sceneIdj, nodeIdj) = _nodeMap[nj.Target];
                _writer.WriteAttributeString("TargetScene", sceneIdj.ToString());
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
            _writer.WriteStartElement("NodePlaySound");
            if (nps.Sound != null) _writer.WriteAttributeString("Path", _projectService.GetRelativePath(nps.Sound.Path));
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

                if (keyPair.Target != null && _nodeMap.ContainsKey(keyPair.Target))
                {
                    var (sceneId, nodeId) = _nodeMap[keyPair.Target];
                    _writer.WriteAttributeString("TargetScene", sceneId.ToString());
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
            if (nsv.Variable != null) _writer.WriteAttributeString("Variable", _projectService.Variables.IndexOf(nsv.Variable).ToString());
            _writer.WriteAttributeString("SetType", nsv.Type.ToString());
            _writer.WriteAttributeString("Value", nsv.Value.ToString());
            _writer.WriteEndElement();
        }

        public void VisitNodeShake(NodeShake ns)
        {
            _writer.WriteStartElement("NodeShake");
            _writer.WriteAttributeString("ShakeBackground", ns.ShakeBackground.ToString());
            _writer.WriteAttributeString("ShakeCharacters", ns.ShakeCharacters.ToString());
            _writer.WriteAttributeString("Amplitude", ns.Amplitude.ToString(CultureInfo.InvariantCulture));
            _writer.WriteAttributeString("Duration", ns.Duration.ToString(CultureInfo.InvariantCulture));
            _writer.WriteEndElement();
        }

        public void VisitNodeVariableJump(NodeVariableJump nvj)
        {
            _writer.WriteStartElement("NodeVariableJump");
            if (nvj.Target != null && _nodeMap.ContainsKey(nvj.Target))
            {
                var (sceneIdvj, nodeIdvj) = _nodeMap[nvj.Target];
                _writer.WriteAttributeString("TargetScene", sceneIdvj.ToString());
                _writer.WriteAttributeString("TargetNode", nodeIdvj.ToString());
            }
            if (nvj.Variable != null) _writer.WriteAttributeString("Variable", _projectService.Variables.IndexOf(nvj.Variable).ToString());
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
                    _writer.WriteAttributeString("EventName", ne.EventName ?? string.Empty);
                    break;
                case NodeEvent.EventTypeEnum.HookBrackets:
                    _writer.WriteAttributeString("Text", ne.Text ?? string.Empty);
                    _writer.WriteAttributeString("Duration", ne.Duration.ToString(CultureInfo.InvariantCulture));
                    _writer.WriteAttributeString("BlackOnWhite", ne.BlackOnWhite.ToString(CultureInfo.InvariantCulture));
                    break;
            }

            _writer.WriteEndElement();
        }

        public void VisitNodeTodo(NodeTodo nt)
        {
            _writer.WriteStartElement("NodeTodo");
            _writer.WriteString(nt.Note);
            _writer.WriteEndElement();
        }
        public void VisitNodeCall(NodeCall nc)
        {
            _writer.WriteStartElement("NodeCall");
            if (nc.Target != null)
            {
                var sceneId = _scenes.IndexOf(nc.Target);
                if (sceneId == -1)
                {
                    _writer.WriteEndElement();
                    return;
                }

                _writer.WriteAttributeString("TargetScene", sceneId.ToString());
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
            _writer.WriteString(nl.Script ?? string.Empty);
            _writer.WriteEndElement();
        }

        public void VisitNodeAchievement(NodeAchievement na)
        {
            _writer.WriteStartElement("NodeAchievement");
            _writer.WriteAttributeString("Name", na.Name ?? string.Empty);
            _writer.WriteEndElement();
        }

        public void VisitNodeRouteCompleted(NodeRouteCompleted nrc)
        {
            _writer.WriteStartElement("NodeRouteCompleted");
            _writer.WriteAttributeString("Name", nrc.Name ?? string.Empty);
            _writer.WriteEndElement();
        }

        public void VisitNodeOutfitUnlocked(NodeOutfitUnlocked nou)
        {
            _writer.WriteStartElement("NodeOutfitUnlocked");
            if (nou.Character != null) _writer.WriteAttributeString("Character", _projectService.Characters.IndexOf(nou.Character).ToString());
            if (nou.Character != null && nou.Outfit != null) _writer.WriteAttributeString("Outfit", nou.Character.Outfits.IndexOf(nou.Outfit).ToString());
            _writer.WriteEndElement();
        }
    }
}
