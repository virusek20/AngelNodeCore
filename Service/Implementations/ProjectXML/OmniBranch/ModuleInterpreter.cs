using AngelNode.Model;
using AngelNode.Model.Node;
using AngelNode.Model.Project;
using AngelNode.Service.Interface;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AngelNode.Service.Implementations.ProjectXML.OmniBranch
{
    public class ModuleInterpreter : INodeVisitor
    {
        public ModuleState InputState { get; private set; } = new ModuleState();
        public ModuleState OutputState { get; private set; } = new ModuleState();
        public int Position { get; set; } = 0;
        public bool Running { get; set; } = true;
        public ProjectReport Report;

        public HashSet<Character> UsedCharacters { get; private set; } = new HashSet<Character>();
        public HashSet<Pose> UsedPoses { get; private set; } = new HashSet<Pose>();
        public HashSet<Variable> UsedVariables { get; private set; } = new HashSet<Variable>();

        private readonly Scene _scene;
        private readonly Module _parent;
        private readonly IProjectService _projectService; // TODO: EH
        private readonly ConcurrentDictionary<Scene, Module> _modules;

        public ModuleInterpreter(Scene scene, ConcurrentDictionary<Scene, Module> modules, Module parent, ProjectReport report)
        {
            _scene = scene;
            _parent = parent;
            _modules = modules;
            _projectService = SimpleIoc.Default.GetInstance<IProjectService>();
            Report = report;
        }

        public async Task Interpret()
        {
            while (Running)
            {
                if (_scene.Nodes.Count == Position)
                {
                    Running = false;
                    return;
                }

                var node = _scene.Nodes[Position++];
                node.Accept(this);

                if (node is NodeCall nc)
                {
                    if (nc.Target == null) continue;

                    var module = await WaitForModule(nc.Target);
                    var resultStates = module.States.FirstOrDefault(s => OutputState.Matches(s.Key)).Value;
                    if (resultStates == null) 
                    {
                        Report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "No overload for this call takes current state.", nc);
                        Running = false;
                        return;
                    }

                    OutputState.Merge(resultStates.First());

                    foreach (var outputState in resultStates.Skip(1))
                    {
                        var clone = Clone();
                        clone.OutputState.Merge(outputState);
                        _parent.AddInterpreter(clone);
                    }
                }
            }
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
        }

        public void VisitNodeChangePose(NodeChangePose ncp)
        {
            UsedCharacters.Add(ncp.Character);
            UsedPoses.Add(ncp.Pose);
        }

        public void VisitNodeJump(NodeJump nj)
        {
            if (nj.Target == null) return;

            var targetIndexJ = _scene.Nodes.IndexOf(nj.Target);
            if (targetIndexJ == -1)
            {
                Report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "Raptors dispatched.", nj);
                Running = false;
            }
            else if (targetIndexJ < Position)
            {
                Report.AddMessage(ProjectReportMessage.MessageSeverity.Info, "Detected jump backwards (potential loop), aborting analysis of this branch.", nj);
                Running = false;
            }

            Position = targetIndexJ;
        }

        public void VisitNodeMovement(NodeMovement nm)
        {
            UsedCharacters.Add(nm.Character);

            if (OutputState.Characters.ContainsKey(nm.MovementDirection)) // Operating on current module characters
            {
                switch (nm.MovementType)
                {
                    case NodeMovement.MovementTypeEnum.Enter:
                        if (OutputState.Characters.ContainsKey(nm.MovementDirection) && OutputState.Characters[nm.MovementDirection] != null) Report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A movement node is adding a character to an occupied location.", nm);
                        if (OutputState.Characters.ContainsValue(nm.Character)) Report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A movement node is adding a character that is already in the scene.", nm);
                        else
                        {
                            OutputState.Characters[nm.MovementDirection] = nm.Character;
                        }
                        break;
                    case NodeMovement.MovementTypeEnum.Exit:
                        if (!OutputState.Characters.ContainsKey(nm.MovementDirection) || OutputState.Characters[nm.MovementDirection] == null) Report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "Movement node is removing character from side with no present characters.", nm);
                        else if (OutputState.Characters[nm.MovementDirection] != nm.Character) Report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "A movement node is removing a character from the wrong side or another character overwrote it.", nm);
                        else
                        {
                            OutputState.Characters[nm.MovementDirection] = null;
                        }
                        break;
                }
            }
            else // Operating on caller characters
            {
                switch (nm.MovementType)
                {
                    case NodeMovement.MovementTypeEnum.Enter:
                        InputState.Characters[nm.MovementDirection] = null;
                        OutputState.Characters[nm.MovementDirection] = nm.Character;
                        break;
                    case NodeMovement.MovementTypeEnum.Exit:
                        InputState.Characters[nm.MovementDirection] = nm.Character;
                        OutputState.Characters[nm.MovementDirection] = null;
                        break;
                }
            }
        }

        public void VisitNodeCall(NodeCall nc)
        {
        }

        public void VisitNodeRet(NodeRet nr)
        {
            Running = false;
        }

        public void VisitNodePlaySound(NodePlaySound nps)
        {
        }

        public void VisitNodeResponseDialogue(NodeResponseDialogue nrd)
        {
            UsedCharacters.Add(nrd.Character);

            var targetIndex = _scene.Nodes.IndexOf(nrd.ResponseMap[0].Target);
            if (targetIndex == -1)
            {
                Report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "Extra ferocious raptors dispatched.", nrd);
                Running = false;
            }
            Position = targetIndex;

            foreach (var response in nrd.ResponseMap.Skip(1))
            {
                var target = _scene.Nodes.IndexOf(response.Target);

                if (target == -1)
                {
                    Report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "Extra ferocious raptors dispatched.", nrd);
                    continue;
                }

                var clone = Clone();
                clone.Position = target;
                _parent.AddInterpreter(clone);
            }
        }

        public void VisitNodeSetVariable(NodeSetVariable nsv)
        {
            UsedVariables.Add(nsv.Variable);
            OutputState.Variables[nsv.Variable] = nsv.Value;
        }

        public void VisitNodeShake(NodeShake ns)
        {
        }

        public void VisitNodeVariableJump(NodeVariableJump nvj)
        {
            UsedVariables.Add(nvj.Variable);

            if (nvj.Target == null) return;

            // We set this in this module
            if (OutputState.Variables.ContainsKey(nvj.Variable))
            {
                if (nvj.Value != OutputState.Variables[nvj.Variable]) return;
            }
            else if (InputState.Variables.ContainsKey(nvj.Variable)) // We decided on this beforehand but from caller
            {
                if (nvj.Value != InputState.Variables[nvj.Variable]) return;
            }
            else // May have been set in caller
            {
                InputState.Variables[nvj.Variable] = nvj.Value; // Current branch jumps

                // New branch skips
                var clone = Clone();

                // TODO: Fix
                //clone.InputState.Variables[nvj.Variable] = !nvj.Value;
                _parent.AddInterpreter(clone);
            }

            var targetIndexVJ = _scene.Nodes.IndexOf(nvj.Target);
            if (targetIndexVJ == -1)
            {
                Report.AddMessage(ProjectReportMessage.MessageSeverity.Error, "Variable raptors dispatched.", nvj);
                Running = false;
            }
            else if (targetIndexVJ < Position)
            {
                Report.AddMessage(ProjectReportMessage.MessageSeverity.Info, "Detected jump backwards (potential loop), aborting analysis of this branch.", nvj);
                Running = false;
            }

            Position = targetIndexVJ;
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

        public ModuleInterpreter Clone()
        {
            var interpreter = new ModuleInterpreter(_scene, _modules, _parent, Report)
            {
                InputState = InputState.Clone(),
                OutputState = OutputState.Clone(),
                Position = Position,
                Running = Running,
                UsedCharacters = new HashSet<Character>(UsedCharacters),
                UsedPoses = new HashSet<Pose>(UsedPoses),
                UsedVariables = new HashSet<Variable>(UsedVariables)
            };

            return interpreter;
        }

        private async Task<Module> WaitForModule(INode targetNode)
        {
            // TODO: Deprecated
            var targetScene = _projectService.Scenes.FirstOrDefault(s => s.Nodes.Contains(targetNode));
            if (targetScene == null) throw new ArgumentException();

            while (!_modules.ContainsKey(targetScene)) await Task.Delay(20);

            return _modules[targetScene];
        }

        private async Task<Module> WaitForModule(Scene targetScene)
        {
            while (!_modules.ContainsKey(targetScene)) await Task.Delay(20);

            return _modules[targetScene];
        }

        public void VisitNodePhone(NodePhone np)
        {
        }

        public void VisitNodeLua(NodeLua nl)
        {
            throw new NotImplementedException();
        }

        public void VisitNodeAchievement(NodeAchievement na)
        {
            throw new NotImplementedException();
        }

        public void VisitNodeAchivement(NodeAchievement na)
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
