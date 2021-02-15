using System;
using System.Globalization;
using System.Text;
using AngelNode.Model.Node;

namespace AngelNode.Service.Implementations.RenPy
{
    public class RenPyNodeVisitor : INodeVisitor
    {
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private string _lastBackground;

        public void VisitNodeDialogue(NodeDialogue nd)
        {
            if (nd.Character.Name == "Player - Internal")
            {
                _stringBuilder.AppendLine($"\"{nd.Text ?? string.Empty}\"");
            }
            else
            {
                _stringBuilder.AppendLine($"\"{nd.Character.Name}\" \"{nd.Text ?? string.Empty}\"");
            }
        }

        public void VisitNodeFadeMusic(NodeFadeMusic ndm)
        {
            _stringBuilder.AppendLine("stop music");
        }

        public void VisitNodeChangeBackground(NodeChangeBackground ncb)
        {
            _lastBackground = ncb.Background.Name.Split('.')[0];
            _stringBuilder.AppendLine($"scene {ncb.Background.Name.Split('.')[0]}");
            switch (ncb.TransitionType)
            {
                case NodeChangeBackground.BackgroundTransitionTypeEnum.NewDay:
                    _stringBuilder.AppendLine("with fade");
                    break;
                case NodeChangeBackground.BackgroundTransitionTypeEnum.Blend:
                    _stringBuilder.AppendLine("with dissolve");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void VisitNodeChangePose(NodeChangePose ncp)
        {
            _stringBuilder.AppendLine($"show {ncp.Pose.File.Path.Split('.')[0]}");
        }

        public void VisitNodeJump(NodeJump nj)
        {
        }

        public void VisitNodeMovement(NodeMovement nm)
        {
            switch (nm.MovementType)
            {
                case NodeMovement.MovementTypeEnum.Enter:
                    break;
                case NodeMovement.MovementTypeEnum.Exit:
                    _stringBuilder.AppendLine($"hide {nm.Character.Name}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void VisitNodePlaySound(NodePlaySound nps)
        {
            switch (nps.SoundType)
            {
                case NodePlaySound.SoundTypeEnum.Music:
                    _stringBuilder.AppendLine($"play music \"sound/{nps.Sound.Name}\"");
                    break;
                case NodePlaySound.SoundTypeEnum.SFX:
                    _stringBuilder.AppendLine($"play sound \"sound/{nps.Sound.Name}\"");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void VisitNodeResponseDialogue(NodeResponseDialogue nrd)
        {
            _stringBuilder.AppendLine("menu:");

            foreach (var response in nrd.ResponseMap)
            {
                _stringBuilder.AppendLine($"    \"{response.Text}\":");
                _stringBuilder.AppendLine($"        \"ayy\"");
                //_stringBuilder.AppendLine($"        jump {response.Target}");
            }
        }

        public void VisitNodeSetVariable(NodeSetVariable nsv)
        {
            _stringBuilder.AppendLine($"$ {nsv.Variable.Name} = {nsv.Value}");
            
        }

        public void VisitNodeShake(NodeShake ns)
        {
            _stringBuilder.AppendLine($"scene bg {_lastBackground}");
            _stringBuilder.AppendLine("with hpunch");
        }

        public void VisitNodeVariableJump(NodeVariableJump nvj)
        {
        }

        public void VisitNodeWait(NodeWait nw)
        {
            _stringBuilder.AppendLine($"pause {nw.Duration.ToString("0.0#", CultureInfo.InvariantCulture)}");
        }

        public void VisitNodeEvent(NodeEvent ne)
        {
        }

        public void VisitNodeTodo(NodeTodo nt)
        {
        }
        public override string ToString()
        {
            return _stringBuilder.ToString();
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

        public void VisitNodeLua(NodeLua nl)
        {
            throw new NotImplementedException();
        }

        public void VisitNodeAchievement(NodeAchievement na)
        {
            throw new NotImplementedException();
        }

        public void VisitNodeRouteCompleted(NodeRouteCompleted nrc)
        {
            throw new NotImplementedException();
        }

        public void VisitNodeOutfitUnlocked(NodeOutfitUnlocked nou)
        {
            throw new NotImplementedException();
        }
    }
}
