using AngelNode.Model;
using AngelNode.Model.Node;
using AngelNode.Service.Interface;
using AngelNode.Util;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AngelNode.Service.Implementations.DOT
{
    public class DotExportService : IExportService
    {
        private readonly IProjectService _projectService;
        private readonly OperationConverter _converter = new OperationConverter();

        public string DefaultExtension => ".dot";

        public string FileFilter => "DOT Graphs|*.dot";

        private const bool ShowEntireGraph = false;

        public DotExportService()
        {
            _projectService = SimpleIoc.Default.GetInstance<IProjectService>();
        }

        public void Export(string path)
        {
            var folderName = Path.GetDirectoryName(path);
            var outputName = Path.GetFileNameWithoutExtension(path);
            var extension = Path.GetExtension(path);

            foreach (var scene in _projectService.Scenes.OfType<Scene>()) ExportScene(scene, $"{folderName}\\{outputName}_{scene.Name}{extension}");
        }

        private void ExportScene(Scene scene, string path)
        {
            StreamWriter writer = File.CreateText(path);

            var graphNodes = new List<GraphNode>(scene.Nodes.Count);
            for (int i = 0; i < scene.Nodes.Count; i++) graphNodes.Add(new GraphNode());
            var maxId = graphNodes.Capacity + 1;

            for (int i = 0; i < scene.Nodes.Count; i++)
            {
                var node = scene.Nodes[i];
                graphNodes[i].Node = node;

                switch (node)
                {
                    case NodeJump nj:
                        var targetId = scene.Nodes.IndexOf(nj.Target);
                        graphNodes[i].Text = $"Jump from {i} to {targetId}";
                        graphNodes[i].NextNodes.Add(graphNodes[targetId], null);
                        break;
                    case NodeVariableJump nvj:
                        graphNodes[i].Text = FormatVariableJump(nvj);
                        var targetId1 = scene.Nodes.IndexOf(nvj.Target);
                        var targetId2 = i + 1;

                        if (nvj.Variable.Binary && nvj.Value == 0)
                        {
                            graphNodes[i].NextNodes.Add(graphNodes[targetId1], "false");
                            graphNodes[i].NextNodes.Add(graphNodes[targetId2], "true");
                        }
                        else
                        {
                            graphNodes[i].NextNodes.Add(graphNodes[targetId1], "true");
                            graphNodes[i].NextNodes.Add(graphNodes[targetId2], "false");
                        }

                        break;
                    case NodeResponseDialogue nrd:
                        graphNodes[i].Text = $"{nrd.Character.Name}\n\"{nrd.Dialogue.WordWrap(70)}\"";
                        foreach (var response in nrd.ResponseMap)
                        {
                            var targetIdr = scene.Nodes.IndexOf(response.Target);
                            if (graphNodes[i].NextNodes.ContainsKey(graphNodes[targetIdr])) graphNodes[i].NextNodes[graphNodes[targetIdr]] += $"\n{response.Text}";
                            else graphNodes[i].NextNodes.Add(graphNodes[targetIdr], response.Text);
                        }
                        break;
                    case NodeCall nc:
                        graphNodes[i].Text = nc.Target.Name;
                        graphNodes[i].NextNodes.Add(graphNodes[i + 1], null);
                        break;
                    case NodeSetVariable nsv:
                        graphNodes[i].Text = $"{nsv.Variable.Name} {_converter.Convert(nsv.Type, null, null, null)} {nsv.Value}";
                        graphNodes[i].NextNodes.Add(graphNodes[i + 1], null);
                        break;
                    case NodeDialogue nd:
                        graphNodes[i].Text = $"{nd.Character.Name}\n\"{nd.Text.WordWrap(70)}\"";
                        graphNodes[i].NextNodes.Add(graphNodes[i + 1], null);
                        break;
                    case NodeMovement nm:
                        graphNodes[i].Text = $"Movement\n{nm.Character.Name} {nm.MovementDirection} {nm.MovementType}";
                        graphNodes[i].NextNodes.Add(graphNodes[i + 1], null);
                        break;
                    case NodeChangeBackground ncb:
                        graphNodes[i].Text = $"Background change\n{ncb.Background.Name}";
                        graphNodes[i].NextNodes.Add(graphNodes[i + 1], null);
                        break;
                    case NodeFadeMusic nfm:
                        switch (nfm.AudioFadeType)
                        {
                            case NodeFadeMusic.AudioFadeTypeEnum.All:
                                graphNodes[i].Text = $"Sound fade";
                                break;
                            case NodeFadeMusic.AudioFadeTypeEnum.Music:
                                graphNodes[i].Text = $"Music fade";
                                break;
                            case NodeFadeMusic.AudioFadeTypeEnum.SFX:
                                graphNodes[i].Text = $"SFX fade";
                                break;
                        }
                        
                        graphNodes[i].NextNodes.Add(graphNodes[i + 1], null);
                        break;
                    case NodeChangePose ncp:
                        graphNodes[i].Text = $"Pose change\n{ncp.Character.Name} -> {ncp.Pose.Name}";
                        graphNodes[i].NextNodes.Add(graphNodes[i + 1], null);
                        break;
                    case NodeWait nw:
                        graphNodes[i].Text = $"Wait {nw.Duration}s";
                        graphNodes[i].NextNodes.Add(graphNodes[i + 1], null);
                        break;
                    case NodePlaySound nps:
                        switch (nps.SoundType)
                        {
                            case NodePlaySound.SoundTypeEnum.Music:
                                graphNodes[i].Text = $"Play music\n{nps.Sound.Name}";
                                break;
                            case NodePlaySound.SoundTypeEnum.SFX:
                                graphNodes[i].Text = $"Play sound\n{nps.Sound.Name}";
                                break;
                        }
                        
                        graphNodes[i].NextNodes.Add(graphNodes[i + 1], null);
                        break;
                    case NodeRet _:
                        graphNodes[i].Text = "End\n" + scene.Name;
                        break;
                    case NodeTodo nt:
                        graphNodes[i].Text = $"TODO\n{nt.Note}";
                        graphNodes[i].NextNodes.Add(graphNodes[i + 1], null);
                        break;
                    default:
                        graphNodes[i].Text = node.GetType().Name;
                        if (i != scene.Nodes.Count - 1) graphNodes[i].NextNodes.Add(graphNodes[i + 1], null);
                        break;
                }
            }

            foreach (var node in graphNodes)
            {
                foreach (var target in node.NextNodes)
                {
                    var targetId = graphNodes.IndexOf(target.Key);
                    if (targetId < 0) continue;
                    graphNodes[targetId].PreviousNodes.Add(node);
                }
            }

            foreach (var node in graphNodes)
            {
                node.Critical = ShowEntireGraph ||
                    node.NextNodes.Count >= 2 ||
                    node.PreviousNodes.Count >= 2 ||
                    IsBranching(node.Node) ||
                    node.PreviousNodes.Any(n => IsBranching(n.Node));

                var sign = IsCharacterVariable(node.Node);
                if (sign > 0) node.Style = "style=filled fillcolor=green";
                else if (sign < 0) node.Style = "style=filled fillcolor=red";
            }

            var startNode = graphNodes.First();
            startNode.Critical = true;

            var endNode = graphNodes.Last();
            endNode.Critical = true;
            endNode.Text = "End\n" + scene.Name;

            if (!ShowEntireGraph)
            {
                // Collapse 1-input, 1-output jump nodes
                foreach (var jumpNode in graphNodes.Where(n => n.Node is NodeJump))
                {
                    if (jumpNode.PreviousNodes.Count == 1 && jumpNode.NextNodes.Count == 1)
                    {
                        var prevNode = jumpNode.PreviousNodes.First();
                        var nextNode = jumpNode.NextNodes.First();

                        GraphNode nextCriticalNode = null;
                        if (nextNode.Key.Critical) nextCriticalNode = nextNode.Key;
                        else nextCriticalNode = graphNodes[GetNextNode(graphNodes, nextNode.Key)];

                        jumpNode.Critical = false;
                        prevNode.Critical = true;

                        var oldNext = prevNode.NextNodes[jumpNode];
                        prevNode.NextNodes.Remove(jumpNode);
                        prevNode.NextNodes.Add(nextCriticalNode, oldNext);

                        nextCriticalNode.PreviousNodes.Remove(jumpNode);
                        nextCriticalNode.PreviousNodes.Add(prevNode);
                    }
                }
            }

            writer.WriteLine("digraph {" +
                $"start [label=\"Start\n{scene.Name}\" shape=rarrow margin=0.2];\n" +
                "start -> 0;");

            for (int i = 0; i < graphNodes.Count; i++)
            {
                GraphNode node = graphNodes[i];
                if (!node.Critical) continue;

                if (node.Node is NodeRet) writer.WriteLine($"{i} [label=\"{node.Text?.Replace("\"","\\\"")}\" shape=larrow margin=0.2];");
                else if (node.Node is NodeVariableJump) writer.WriteLine($"{i} [label=\"{node.Text?.Replace("\"", "\\\"")}\" shape=diamond];");
                else if (node.Node is NodeCall) writer.WriteLine($"{i} [label=\"{node.Text?.Replace("\"", "\\\"")}\" shape=rarrow];");
                else if (node.Node is NodeTodo) writer.WriteLine($"{i} [label=\"{node.Text?.Replace("\"", "\\\"")}\" shape=box color=green ];");
                else if (node.Node is NodeAchievement) writer.WriteLine($"{i} [label=\"{node.Text?.Replace("\"", "\\\"")}\" shape=box color=yellow ];");
                else if (node.Node is NodeLua) writer.WriteLine($"{i} [label=\"{node.Text?.Replace("\"", "\\\"")}\\nPotential branch\" shape=box color=blue ];");
                else writer.WriteLine($"{i} [label=\"{node.Text?.Replace("\"", "\\\"")}\" shape=box {node.Style}];");

                if (node.PreviousNodes.Count == 0 && i != 0)
                {
                    if (scene.Nodes.Any(n => n is NodeLua)) Debug.WriteLine($"Node with no incoming edges: {scene.Name} ({i}) - {node.Node} | But scene contains Lua");
                    else Debug.WriteLine($"Node with no incoming edges: {scene.Name} ({i}) - {node.Node}");
                }

                foreach (var nextNode in node.NextNodes)
                {
                    if (node != graphNodes.Last())
                    {
                        if (!nextNode.Key.Critical)
                        {
                            var skipNodeId = GetNextNode(graphNodes, node);
                            writer.WriteLine($"{i} -> {skipNodeId};");
                            continue;
                        };

                        writer.WriteLine($"{i} -> {graphNodes.IndexOf(nextNode.Key)} [ label=\"{nextNode.Value?.Replace("\"", "\\\"")}\" ];");
                    }
                }

                if (node.NextNodes.Count >= 2)
                {
                    writer.Write("{rank = same;");
                    foreach (var nextNode in node.NextNodes)
                    {
                        var nextNodeId = graphNodes.IndexOf(nextNode.Key);
                        writer.Write($"{nextNodeId};");
                    }
                    writer.WriteLine("}");
                }
            }

            writer.WriteLine("}");
            writer.Close();
        }

        private bool IsBranching(INode node)
        {
            return node is NodeJump ||
                   node is NodeResponseDialogue ||
                   node is NodeVariableJump ||
                   node is NodeSetVariable ||
                   node is NodeRet ||
                   node is NodeCall ||
                   node is NodeLua;
        }

        private int IsCharacterVariable(INode node)
        {
            if (node is NodeSetVariable nsv)
            {
                // TODO: List all custom important vars here
                if (nsv.Variable.Name == "Nuri_Eliminated") return -1;

                if (nsv.Variable.HighlightChanges) return Math.Sign(nsv.Value);

                return 0;
            }

            return 0;
        }

        private int GetNextNode(List<GraphNode> nodes, GraphNode node)
        {
            var startIndex = nodes.IndexOf(node) + 1;
            for (int i = startIndex; i < nodes.Count; i++)
            {
                if (nodes[i].Critical) return i;
            }

            throw new ArgumentOutOfRangeException();
        }

        private string FormatVariableJump(NodeVariableJump nvj)
        {
            if (nvj.Variable.Binary) return nvj.Variable.Name;
            else return $"{nvj.Variable.Name} {_converter.Convert(nvj.Type, null, null, null)} {nvj.Value}";
        }

        public int GetNodeNumber(INode node)
        {
            throw new NotImplementedException();
        }

        public void Run(int startNode)
        {
            throw new NotImplementedException();
        }
    }
}
