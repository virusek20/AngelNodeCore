using AngelNode.Model;
using AngelNode.Model.Node;
using AngelNode.Service.Interface;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AngelNode.Service.Implementations.ResourceUsage
{
    public class ResourceUsageExportService : IExportService
    {
        private readonly IProjectService _projectService;

        public string DefaultExtension => ".txt";

        public string FileFilter => "Text files|*.txt";

        private readonly HashSet<string> _background = new HashSet<string>();
        private readonly HashSet<string> _cgs = new HashSet<string>();
        private readonly Dictionary<string, HashSet<string>> _pose = new Dictionary<string, HashSet<string>>();
        private readonly HashSet<string> _sfx = new HashSet<string>();
        private readonly HashSet<string> _music = new HashSet<string>();

        public ResourceUsageExportService()
        {
            _projectService = SimpleIoc.Default.GetInstance<IProjectService>();
        }

        public void Export(string path)
        {
            foreach (var scene in _projectService.Scenes) ScanScene(scene);

            StreamWriter writer = File.CreateText(path);
            writer.WriteLine("--- Resource usage report ---");
            writer.WriteLine("Used backgrounds:");
            foreach (var background in _background.ToList().OrderBy(b => b)) writer.WriteLine(background);

            writer.WriteLine("\nUsed CGs:");
            foreach (var cg in _cgs.ToList().OrderBy(b => b)) writer.WriteLine(cg);

            writer.WriteLine("\nUsed poses:");
            foreach (var character in _pose.ToList().OrderBy(b => b.Key))
            {
                writer.WriteLine($"{character.Key}:");
                foreach (var pose in character.Value.ToList().OrderBy(p => p))
                {
                    writer.WriteLine($"    {pose}");
                }
            }

            writer.WriteLine("\nUsed SFX:");
            foreach (var sfx in _sfx.ToList().OrderBy(b => b)) writer.WriteLine(sfx);

            writer.WriteLine("\nUsed music:");
            foreach (var music in _music.ToList().OrderBy(b => b)) writer.WriteLine(music);
            writer.Close();
        }

        private void ScanScene(Scene scene)
        {
            foreach (var node in scene.Nodes)
            {
                if (node is NodeChangeBackground ncb && ncb.Background != null)
                {
                    if (ncb.Background.Path.Contains("cg\\")) _cgs.Add(ncb.Background.Name);
                    else _background.Add(ncb.Background.Name);
                }
                if (node is NodePlaySound nps && nps.Sound != null)
                {
                    if (nps.SoundType == NodePlaySound.SoundTypeEnum.Music) _music.Add(nps.Sound.Name);
                    else if (nps.SoundType == NodePlaySound.SoundTypeEnum.SFX) _sfx.Add(nps.Sound.Name);
                    else throw new ArgumentOutOfRangeException();
                }
                if (node is NodeChangePose ncp)
                {
                    if (ncp.Character == null || ncp.Pose == null) continue;

                    if (!_pose.ContainsKey(ncp.Character.Name)) _pose.Add(ncp.Character.Name, new HashSet<string>());

                    _pose[ncp.Character.Name].Add(ncp.Pose.Name);
                    _pose[ncp.Character.Name].Add(ncp.Character.DefaultPose.Name);
                }
            }
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
