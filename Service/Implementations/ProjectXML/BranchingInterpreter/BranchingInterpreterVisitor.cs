using System;
using System.Collections.Generic;
using AngelNode.Model;
using AngelNode.Model.Node;
using AngelNode.Model.Project;
using AngelNode.Model.Resource;

namespace AngelNode.Service.Implementations.ProjectXML.BranchingInterpreter
{
    public class BranchingInterpreterVisitor : INodeVisitor, ICloneable
    {
        public ProjectReport Report { get; set; }
        public Dictionary<NodeMovement.MovementDirectionEnum, Character> PresentCharactersSide { get; } = new Dictionary<NodeMovement.MovementDirectionEnum, Character>();
        public HashSet<Pose> UsedPoses { get; } = new HashSet<Pose>();
        public HashSet<Character> PresentCharacters { get; } = new HashSet<Character>();
        public HashSet<Character> UsedCharacters { get; } = new HashSet<Character>();
        public HashSet<File> UsedResources { get; } = new HashSet<File>();
        public HashSet<Variable> SetVariables { get; } = new HashSet<Variable>();

        public BranchingInterpreterVisitor() { }

        public BranchingInterpreterVisitor(IEnumerable<Character> presentCharacters, IDictionary<NodeMovement.MovementDirectionEnum, Character> presentCharactersSide, IEnumerable<Character> usedCharacters, IEnumerable<File> usedResources, IEnumerable<Variable> setVariables)
        {
            PresentCharacters = new HashSet<Character>(presentCharacters);
            PresentCharactersSide = new Dictionary<NodeMovement.MovementDirectionEnum, Character>(presentCharactersSide);
            UsedCharacters = new HashSet<Character>(usedCharacters);
            UsedResources = new HashSet<File>(usedResources);
            SetVariables = new HashSet<Variable>(setVariables);
        }

        public void VisitNodeDialogue(NodeDialogue nd)
        {
            UsedCharacters.Add(nd.Character);
        }

        public void VisitNodeFadeMusic(NodeFadeMusic ndm)
        {
        }

        public void VisitNodeChangeBackground(NodeChangeBackground ncb)
        {
            UsedResources.Add(ncb.Background);
        }

        public void VisitNodeChangePose(NodeChangePose ncp)
        {
            UsedCharacters.Add(ncp.Character);
            UsedPoses.Add(ncp.Pose);
        }

        public void VisitNodeJump(NodeJump nj)
        {
        }

        public void VisitNodeMovement(NodeMovement nm)
        {
            UsedCharacters.Add(nm.Character);

            switch (nm.MovementType)
            {
                case NodeMovement.MovementTypeEnum.Exit:
                    if (!PresentCharacters.Contains(nm.Character))
                    {
                        Report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A movement node is removing a character that is not in the scene.", nm);
                    }
                    else PresentCharacters.Remove(nm.Character);

                    if (!PresentCharactersSide.ContainsKey(nm.MovementDirection) || PresentCharactersSide[nm.MovementDirection] != nm.Character)
                    {
                        Report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A movement node is removing a character from the wrong side or another character overwrote it.", nm);
                    }
                    else PresentCharactersSide.Remove(nm.MovementDirection);
                    break;
                case NodeMovement.MovementTypeEnum.Enter:
                    if (PresentCharacters.Contains(nm.Character))
                    {
                        Report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A movement node is adding a character that is already in the scene.", nm);
                    }
                    else PresentCharacters.Add(nm.Character);

                    if (PresentCharactersSide.ContainsKey(nm.MovementDirection))
                    {
                        Report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A movement node is adding a character to an occupied location.", nm);
                    }
                    else PresentCharactersSide.Add(nm.MovementDirection, nm.Character);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(nm.MovementType), nm.MovementType, null);
            }
        }

        public void VisitNodePlaySound(NodePlaySound nps)
        {
            UsedResources.Add(nps.Sound);
        }

        public void VisitNodeResponseDialogue(NodeResponseDialogue nrd)
        {
            UsedCharacters.Add(nrd.Character);
        }

        public void VisitNodeSetVariable(NodeSetVariable nsv)
        {
            if (nsv.Value) SetVariables.Add(nsv.Variable);
            else SetVariables.Remove(nsv.Variable);
        }

        public void VisitNodeShake(NodeShake ns)
        {
        }

        public void VisitNodeVariableJump(NodeVariableJump nvj)
        {
        }

        public void VisitNodeWait(NodeWait nw)
        {
        }

        public void VisitNodeEvent(NodeEvent ne)
        {
        }

        public void VisitNodeTodo(NodeTodo nt)
        {
        }

        public object Clone()
        {
            return new BranchingInterpreterVisitor(PresentCharacters, PresentCharactersSide, UsedCharacters, UsedResources, SetVariables)
            {
                Report = Report
            };
        }

        public void VisitNodeCall(NodeCall nc)
        {
        }

        public void VisitNodeRet(NodeRet nr)
        {
        }
    }
}
