using AngelNode.Model;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using AngelNode.Model.Project;
using System.Linq;

namespace AngelNode.Service.Implementations.ProjectXML.OmniBranch
{
    public class OmniInterpreter
    {
        public ConcurrentDictionary<Scene, Module> Modules { get; } = new ConcurrentDictionary<Scene, Module>();

        public IEnumerable<Pose> UsedPoses => Modules.Values.SelectMany(m => m.UsedPoses).Distinct();
        public IEnumerable<Variable> UsedVariables => Modules.Values.SelectMany(m => m.UsedVariables).Distinct();
        public IEnumerable<Character> UsedCharacters => Modules.Values.SelectMany(m => m.UsedCharacters).Distinct();

        private readonly ProjectReport _report;
        private readonly ProjectReport _outputReport = new ProjectReport();

        public OmniInterpreter(ProjectReport report)
        {
            _report = report;
        }

        public void Interpret(IList<Scene> scenes)
        {
            var tasks = new List<Task>();
            foreach(var scene in scenes) tasks.Add(InterpretScene(scene));

            Task.WaitAll(tasks.ToArray());

            foreach (var message in _outputReport.Unique()) 
            {
                if (message == null) continue; // TODO: WTF
                _report.Messages.Add(message);
            }
        }

        private async Task InterpretScene(Scene scene)
        {
            var sceneModule = await Module.FromScene(scene, Modules, _outputReport);
            Modules.TryAdd(scene, sceneModule);
        }
    }
}
