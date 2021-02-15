using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AngelNode.Model;
using AngelNode.Model.Node;
using AngelNode.Model.Project;
using AngelNode.Model.Resource;

namespace AngelNode.Service.Implementations.ProjectXML
{
    public class AnalysisNodeVisitor : INodeVisitor
    {
        private readonly ProjectReport _report;
        private readonly IDictionary<INode, LocalProjectService.NodeLocation> _nodeMap;
        private readonly IEnumerable<File> _poseFiles;
        private readonly IList<Scene> _scenes;

        public AnalysisNodeVisitor(ProjectReport report, IDictionary<INode, LocalProjectService.NodeLocation> nodeMap, IEnumerable<File> poseFiles, IList<Scene> scenes)
        {
            _report = report;
            _nodeMap = nodeMap;
            _poseFiles = poseFiles;
            _scenes = scenes;
        }

        public void VisitNodeDialogue(NodeDialogue nd)
        {
            if (nd.Character == null) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A dialogue node has no assigned character.", nd);
            if (string.IsNullOrWhiteSpace(nd.Text)) _report.AddMessage(ProjectReportMessage.MessageSeverity.Warning, "A dialogue node has no assigned text.", nd);
            if (Regex.IsMatch(nd.Text, @"[^\x00-\x7F]")) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A dialogue node contains non-ASCII characters.", nd);
            if (nd.Text?.Length > 300) _report.AddMessage(ProjectReportMessage.MessageSeverity.Warning, "A dialogue node has more than 300 characters of text.", nd);
            if (nd.Text?.Split('\n').Length > 3) _report.AddMessage(ProjectReportMessage.MessageSeverity.Warning, "A dialogue node has more than 3 lines of text.", nd);
        }

        public void VisitNodeFadeMusic(NodeFadeMusic ndm)
        {
            if (ndm.FadeTime <= 0f) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A fade music node has a non-positive fade time.", ndm);
        }

        public void VisitNodeChangeBackground(NodeChangeBackground ncb)
        {
            if (ncb.Background == null) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A change background node has no assigned background.", ncb);
            else if (_poseFiles.Contains(ncb.Background)) _report.AddMessage(ProjectReportMessage.MessageSeverity.Warning, "A change background node has a background that's also being used as a pose.", ncb);
        }

        public void VisitNodeChangePose(NodeChangePose ncp)
        {
            if (ncp.Character == null) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A change pose node has no assigned character.", ncp);
            if (ncp.Character == null && ncp.Pose == null) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A change pose node has no assigned pose.", ncp);
        }

        public void VisitNodeJump(NodeJump nj)
        {
            if (nj.Target == null) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A jump node has no target set.", nj);
            else if (!_nodeMap.ContainsKey(nj.Target)) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A jump node has an invalid target.", nj);
            else
            {
                var jumpOriginScene = _nodeMap[nj].SceneId;
                var jumpTargetScene = _nodeMap[nj.Target].SceneId;

                if (jumpOriginScene != jumpTargetScene) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "Please refer to https://xkcd.com/292/ for additional help.", nj);
            }

            if (nj.Target == nj) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A jump node has itself as the target.", nj);
        }

        public void VisitNodeMovement(NodeMovement nm)
        {
            if (nm.Character == null) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A movement node has no assigned character.", nm);
        }

        public void VisitNodePlaySound(NodePlaySound nps)
        {
            if (nps.Sound == null) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A play sound node has no sound.", nps);
            if (nps.Volume == 0f) _report.AddMessage(ProjectReportMessage.MessageSeverity.Warning, "A play sound node has volume set to 0.", nps);
        }

        public void VisitNodeResponseDialogue(NodeResponseDialogue nrd)
        {
            if (nrd.Character == null) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A response dialogue node has no assigned character.", nrd);
            if (string.IsNullOrWhiteSpace(nrd.Dialogue)) _report.AddMessage(ProjectReportMessage.MessageSeverity.Warning, "A response dialogue node has no assigned text.", nrd);
            if (Regex.IsMatch(nrd.Dialogue, @"[^\x00-\x7F]")) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A response dialogue node contains non-ASCII characters.", nrd);
            if (nrd.Dialogue?.Length > 180) _report.AddMessage(ProjectReportMessage.MessageSeverity.Warning, "A response dialogue node has more than 180 characters of text.", nrd);
            if (nrd.Dialogue?.Split('\n').Length > 3) _report.AddMessage(ProjectReportMessage.MessageSeverity.Warning, "A response dialogue node has more than 3 lines of text.", nrd);
            if (nrd.ResponseMap.Count == 0) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A response dialogue node has no options.", nrd);

            foreach (var keyPair in nrd.ResponseMap)
            {
                if (keyPair.Target == null || !_nodeMap.ContainsKey(keyPair.Target)) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A response dialogue node has an invalid target.", nrd);
                else if (keyPair.Target == nrd) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A response dialogue node has itself as the target.", nrd);
                else
                {
                    var responseOriginScene = _nodeMap[keyPair.Target].SceneId;
                    var responseTargetScene = _nodeMap[keyPair.Target].SceneId;

                    if (responseOriginScene != responseTargetScene) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "Please refer to https://xkcd.com/292/ for additional help.", nrd);
                }

                if (string.IsNullOrWhiteSpace(keyPair.Text)) _report.AddMessage(ProjectReportMessage.MessageSeverity.Warning, "A response dialogue node has no assigned text.", nrd);
            }
        }

        public void VisitNodeSetVariable(NodeSetVariable nsv)
        {
            if (nsv.Variable == null) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A set variable node has no variable.", nsv);
            else if (nsv.Type == NodeSetVariable.SetType.Add && nsv.Value == 0) _report.AddMessage(ProjectReportMessage.MessageSeverity.Warning, "A set variable node set to 'Add' has a value of 0.", nsv);
        }

        public void VisitNodeShake(NodeShake ns)
        {
            if (Math.Abs(ns.Amplitude) < float.Epsilon) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A shake node has no amplitude.", ns);
            if (Math.Abs(ns.Duration) < float.Epsilon) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A shake node has no duration.", ns);
        }

        public void VisitNodeVariableJump(NodeVariableJump nvj)
        {
            if (nvj.Variable == null) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A conditional jump node has no variable.", nvj);

            if (nvj.Target == null || !_nodeMap.ContainsKey(nvj.Target)) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A conditional jump node has an invalid target.", nvj);
            else if (nvj.Target == nvj) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A conditional jump node has itself as the target.", nvj);
            else
            {
                var jumpOriginScene = _nodeMap[nvj].SceneId;
                var jumpTargetScene = _nodeMap[nvj.Target].SceneId;

                if (jumpOriginScene != jumpTargetScene) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "Please refer to https://xkcd.com/292/ for additional help.", nvj);
            }
        }

        public void VisitNodeWait(NodeWait nw)
        {
            if (Math.Abs(nw.Duration) < float.Epsilon) _report.AddMessage(ProjectReportMessage.MessageSeverity.Warning, "A wait node has no delay.", nw);
        }

        public void VisitNodeEvent(NodeEvent ne)
        {
            switch (ne.EventType)
            {
                case NodeEvent.EventTypeEnum.Call:
                    if (ne.Caller == null) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A call event node has no set caller.", ne);
                    break;
                case NodeEvent.EventTypeEnum.Custom:
                    if (string.IsNullOrWhiteSpace(ne.EventName)) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A custom event node has no set event name.", ne);
                    break;
                case NodeEvent.EventTypeEnum.HookBrackets:
                    if (string.IsNullOrWhiteSpace(ne.Text)) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A hook bracket event node has no set text.", ne);
                    if (Math.Abs(ne.Duration) < float.Epsilon) _report.AddMessage(ProjectReportMessage.MessageSeverity.Warning, "A hook bracket event node has no duration.", ne);
                    break;
            }
        }

        public void VisitNodeTodo(NodeTodo nt)
        {
            _report.AddMessage(ProjectReportMessage.MessageSeverity.Info, nt.Note, nt);
        }

        public void VisitNodeCall(NodeCall nc)
        {
            if (nc.Target == null) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A call node has no target set.", nc);
            else if (!_scenes.Contains(nc.Target)) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A call node has an invalid target set.", nc);
        }

        public void VisitNodeRet(NodeRet nr)
        {
        }

        public void VisitNodePhone(NodePhone np)
        {
            if (np.Character == null) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A phone node has no assigned character.", np);
            if (string.IsNullOrWhiteSpace(np.Time)) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A phone node has no set time.", np);
            if (np.PhoneMessages.Count == 0) _report.AddMessage(ProjectReportMessage.MessageSeverity.Info, "A phone node has no messages.", np);

            foreach (var message in np.PhoneMessages)
            {
                if (Regex.IsMatch(message.Text, @"[^\x00-\x7F]")) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A phone node has a message with non-ASCII characters.", np);
            }
        }

        public void VisitNodeLua(NodeLua nl)
        {
            if (string.IsNullOrWhiteSpace(nl.Script)) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A Lua node has no attached script.", nl);
        }

        public void VisitNodeAchievement(NodeAchievement na)
        {
            if (string.IsNullOrWhiteSpace(na.Name)) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "An achievement node has no achievement associated.", na);
        }

        public void VisitNodeRouteCompleted(NodeRouteCompleted nrc)
        {
            if (string.IsNullOrWhiteSpace(nrc.Name)) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A route complete node has no assigned name.", nrc);
        }

        public void VisitNodeOutfitUnlocked(NodeOutfitUnlocked nou)
        {
            if (nou.Character == null) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A outfit unlock node has no assigned character.", nou);
            else if (nou.Outfit == null) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A outfit unlock node has no assigned pose.", nou);
            else if (nou.Character.Outfits.IndexOf(nou.Outfit) == -1) _report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A outfit unlock node has an invalid pose assigned.", nou);
        }
    }
}
