using System.Collections.Generic;
using System.Linq;
using AngelNode.Model;
using AngelNode.Model.Node;
using AngelNode.Model.Project;
using AngelNode.Model.Resource;

namespace AngelNode.Service.Implementations.ProjectXML.BranchingInterpreter
{
    public class BranchingInterpreter
    {
        private class InterpreterState
        {
            public int Position;
            public BranchingInterpreterVisitor Interpreter;
            public bool Running = true;
        }

        private readonly List<InterpreterState> _interpreters = new List<InterpreterState>
        {
            new InterpreterState
            {
                Interpreter = new BranchingInterpreterVisitor(),
                Position = 0
            }
        };
        public IEnumerable<Character> PresentCharacters => _interpreters.SelectMany(i => i.Interpreter.PresentCharacters).Distinct();
        public IEnumerable<Character> UsedCharacters => _interpreters.SelectMany(i => i.Interpreter.UsedCharacters).Distinct();
        public IEnumerable<Pose> UsedPoses => _interpreters.SelectMany(i => i.Interpreter.UsedPoses).Distinct();
        public IEnumerable<File> UsedResources => _interpreters.SelectMany(i => i.Interpreter.UsedResources).Distinct();
        public IEnumerable<Variable> SetVariables => _interpreters.SelectMany(i => i.Interpreter.SetVariables).Distinct();

        public void InterpretScene(Scene scene, ProjectReport report)
        {
            _interpreters[0].Interpreter.Report = report;

            while (_interpreters.Count(i => i.Running) != 0)
            {
                var newStates = new List<InterpreterState>();

                foreach (var interpreter in _interpreters)
                {
                    if (!interpreter.Running) continue;

                    if (scene.Nodes.Count == interpreter.Position)
                    {
                        interpreter.Running = false;
                        continue;
                    }

                    var node = scene.Nodes[interpreter.Position++];
                    node.Accept(interpreter.Interpreter);

                    if (node is NodeJump nj)
                    {
                        if (nj.Target == null) continue;

                        var targetIndexJ = scene.Nodes.IndexOf(nj.Target);
                        if (targetIndexJ == -1)
                        {
                            report.AddMessage(ProjectReportMessage.MessageSeverity.Info, "Detected jump outside of current scope, aborting analysis of this branch.", nj);
                            interpreter.Running = false;
                        }
                        else if (targetIndexJ < interpreter.Position)
                        {
                            report.AddMessage(ProjectReportMessage.MessageSeverity.Info, "Detected jump backwards (potential loop), aborting analysis of this branch.", nj);
                            interpreter.Running = false;
                        }
                        interpreter.Position = targetIndexJ;
                    }
                    else if (node is NodeVariableJump nvj)
                    {
                        if (nvj.Target == null) continue;

                        bool interpreterState = interpreter.Interpreter.SetVariables.Contains(nvj.Variable);
                        bool requiredState = nvj.Value; 
                        if (interpreterState != requiredState) continue;

                        var targetIndexVJ = scene.Nodes.IndexOf(nvj.Target);
                        if (targetIndexVJ == -1)
                        {
                            report.AddMessage(ProjectReportMessage.MessageSeverity.Info, "Detected jump outside of current scope, aborting analysis of this branch.", nvj);
                            interpreter.Running = false;
                        }
                        else if (targetIndexVJ < interpreter.Position)
                        {
                            report.AddMessage(ProjectReportMessage.MessageSeverity.Info, "Detected jump backwards (potential loop), aborting analysis of this branch.", nvj);
                            interpreter.Running = false;
                        }

                        interpreter.Position = targetIndexVJ;
                    }
                    else if (node is NodeCall nc)
                    {
                        var targetIndexC = scene.Nodes.IndexOf(nc.Target);
                        if (targetIndexC != -1) report.AddMessage(ProjectReportMessage.MessageSeverity.Info, "Local call detected, aborting analysis of this branch.", nc);
                        else report.AddMessage(ProjectReportMessage.MessageSeverity.Info, "Intermodular call detected, aborting analysis of this branch.", nc);
                    }
                    else if (node is NodeRet)
                    {
                        interpreter.Running = false;
                    }
                    else if (node is NodeResponseDialogue nrd)
                    {
                        var targetIndex = scene.Nodes.IndexOf(nrd.ResponseMap[0].Target);
                        if (targetIndex == -1)
                        {
                            report.AddMessage(ProjectReportMessage.MessageSeverity.Info, "Detected jump outside of current scope, aborting analysis of this branch.", nrd);
                            interpreter.Running = false;
                        }
                        interpreter.Position = targetIndex;

                        foreach (var response in nrd.ResponseMap.Skip(1))
                        {
                            var newState = new InterpreterState
                            {
                                Interpreter = (BranchingInterpreterVisitor)interpreter.Interpreter.Clone(),
                                Position = scene.Nodes.IndexOf(response.Target),
                                Running = true
                            };

                            if (newState.Position == -1)
                            {
                                report.AddMessage(ProjectReportMessage.MessageSeverity.Info, "Detected jump outside of current scope, aborting analysis of this branch.", nrd);
                            }
                            else newStates.Add(newState);
                        }
                    }
                }

                _interpreters.AddRange(newStates);
            }
        }
    }
}
