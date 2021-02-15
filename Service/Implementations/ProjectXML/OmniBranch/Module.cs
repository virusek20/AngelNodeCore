using AngelNode.Model;
using AngelNode.Model.Project;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AngelNode.Service.Implementations.ProjectXML.OmniBranch
{
    public class Module
    {
        public ReadOnlyDictionary<ModuleState, HashSet<ModuleState>> States { get; private set; }

        public IEnumerable<Pose> UsedPoses => _interpreters.SelectMany(mi => mi.UsedPoses).Distinct();
        public IEnumerable<Variable> UsedVariables => _interpreters.SelectMany(mi => mi.UsedVariables).Distinct();
        public IEnumerable<Character> UsedCharacters => _interpreters.SelectMany(mi => mi.UsedCharacters).Distinct();

        private readonly Dictionary<ModuleState, HashSet<ModuleState>> _states = new Dictionary<ModuleState, HashSet<ModuleState>>();
        private readonly ConcurrentBag<ModuleInterpreter> _interpreters = new ConcurrentBag<ModuleInterpreter>();
        private readonly ConcurrentBag<Task> _interpreterTasks = new ConcurrentBag<Task>();

        private Module() { }

        public static async Task<Module> FromScene(Scene analysisTarget, ConcurrentDictionary<Scene, Module> modules, ProjectReport report)
        {
            Module module = new Module();
            module.States = new ReadOnlyDictionary<ModuleState, HashSet<ModuleState>>(module._states);
            var startInterpreter = new ModuleInterpreter(analysisTarget, modules, module, report);
            module.AddInterpreter(startInterpreter);

            await Task.WhenAll(module._interpreterTasks);
            module.ReduceStates();

            return module;
        }

        public void AddInterpreter(ModuleInterpreter interpreter)
        {
            _interpreters.Add(interpreter);
            _interpreterTasks.Add(Task.Run(async () => await interpreter.Interpret()));
        }

        public void ReduceStates()
        {
            foreach (var interpreter in _interpreters)
            {
                var inputState = _states.FirstOrDefault(isa => isa.Key.Matches(interpreter.InputState)).Value;

                if (inputState == null) 
                {
                    inputState = new HashSet<ModuleState>();
                    _states[interpreter.InputState] = inputState;
                }
                else
                {
                    var matchingOutput = inputState.FirstOrDefault(isa => isa.Matches(interpreter.OutputState));
                    if (matchingOutput != null) continue;
                }

                inputState.Add(interpreter.OutputState);
            }
        }
    }
}
