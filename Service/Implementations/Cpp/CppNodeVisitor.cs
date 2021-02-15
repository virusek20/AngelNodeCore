using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using AngelNode.Model;
using AngelNode.Model.Node;
using AngelNode.Model.Resource;

namespace AngelNode.Service.Implementations.Cpp
{
    public class CppNodeVisitor : INodeVisitor
    {
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private readonly IDictionary<INode, int> _nodeMapping;
        private readonly IDictionary<File, string> _resourceMapping;
        private readonly IDictionary<Character, string> _characterMapping;
        private readonly IDictionary<Variable, string> _variableMapping;

        public CppNodeVisitor(IDictionary<INode, int> nodeMapping, IDictionary<File, string> resourceMapping,
            IDictionary<Character, string> characterMapping, IDictionary<Variable, string> variableMapping)
        {
            _nodeMapping = nodeMapping;
            _resourceMapping = resourceMapping;
            _characterMapping = characterMapping;
            _variableMapping = variableMapping;
        }

        public void VisitNodeDialogue(NodeDialogue nd)
        {
            _stringBuilder.AppendLine($"AddDialogueNode({_characterMapping[nd.Character]}, \"{nd.Text?.Replace(Environment.NewLine, '\\' + Environment.NewLine) ?? string.Empty}\");");
        }

        public void VisitNodeFadeMusic(NodeFadeMusic ndm)
        {
            _stringBuilder.AppendLine("AddFadeMusicNode();");
        }

        public void VisitNodeChangeBackground(NodeChangeBackground ncb)
        {
            _stringBuilder.AppendLine($"AddBackgroundNode(\"{_resourceMapping[ncb.Background]}\", \"{ncb.DayText ?? string.Empty}\", {GetBoolCpp(ncb.TransitionType == NodeChangeBackground.BackgroundTransitionTypeEnum.FadeToBlack)}, TransitionSpeed::{ncb.TransitionSpeed.ToString().ToUpper()});");
        }

        public void VisitNodeChangePose(NodeChangePose ncp)
        {
            _stringBuilder.AppendLine($"AddPoseChangeNode({_characterMapping[ncp.Character]}, \"{ncp.Pose.Name}\");");
        }

        public void VisitNodeJump(NodeJump nj)
        {
            _stringBuilder.AppendLine($"AddJumpNode({_nodeMapping[nj.Target]});");
        }

        public void VisitNodeMovement(NodeMovement nm)
        {
            _stringBuilder.AppendLine($"AddMovementNode({_characterMapping[nm.Character]}, {nm.MovementTypeString}, {nm.MovementDirectionString});");
        }

        public void VisitNodePlaySound(NodePlaySound nps)
        {
            _stringBuilder.AppendLine($"AddPlaySoundNode(\"{_resourceMapping[nps.Sound]}\", {GetBoolCpp(nps.SoundType == NodePlaySound.SoundTypeEnum.Music)});");
        }

        public void VisitNodeResponseDialogue(NodeResponseDialogue nrd)
        {
            _stringBuilder.AppendLine($"AddResponseDialogueNode({_characterMapping[nrd.Character]}, \"{nrd.Dialogue ?? string.Empty}\", {{");
            foreach (var response in nrd.ResponseMap) _stringBuilder.AppendLine($"\t{{\"{response.Text ?? string.Empty}\", {_nodeMapping[response.Target]}}},");
            _stringBuilder.AppendLine("});");
        }

        public void VisitNodeSetVariable(NodeSetVariable nsv)
        {
            _stringBuilder.AppendLine($"AddSetVariableNode(\"{_variableMapping[nsv.Variable]}\", {nsv.Value});");
        }

        public void VisitNodeShake(NodeShake ns)
        {
            _stringBuilder.AppendLine($"AddShakeNode({GetBoolCpp(ns.ShakeBackground)}, {GetBoolCpp(ns.ShakeCharacters)}, {ns.Falloff.ToString("0.0#", CultureInfo.InvariantCulture)}f, {ns.Magnitude.ToString("0.0#", CultureInfo.InvariantCulture)}f);");
        }

        public void VisitNodeVariableJump(NodeVariableJump nvj)
        {
            _stringBuilder.AppendLine($"AddVariableJumpNode(\"{_variableMapping[nvj.Variable]}\", {_nodeMapping[nvj.Target]}, {nvj.Value});");
        }

        public void VisitNodeWait(NodeWait nw)
        {
            _stringBuilder.AppendLine($"AddWaitNode({nw.Duration.ToString("0.0#", CultureInfo.InvariantCulture)}f);");
        }

        public void VisitNodeEvent(NodeEvent ne)
        {
            _stringBuilder.AppendLine($"AddEffectNode(\"{ne.EventName}\");");
        }

        public void VisitNodeTodo(NodeTodo nt)
        {
        }

        public override string ToString()
        {
            return _stringBuilder.ToString();
        }

        private string GetBoolCpp(bool value)
        {
            return value ? "true" : "false";
        }

        public void VisitNodeCall(NodeCall nc)
        {
            throw new NotImplementedException();
        }

        public void VisitNodeRet(NodeRet nr)
        {
            throw new NotImplementedException();
        }

        public void VisitNodePhone(NodePhone np)
        {
            throw new NotImplementedException();
        }
    }
}
