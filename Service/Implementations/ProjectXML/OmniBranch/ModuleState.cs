using AngelNode.Model;
using AngelNode.Model.Node;
using System.Collections.Generic;

namespace AngelNode.Service.Implementations.ProjectXML.OmniBranch
{
    public class ModuleState
    {
        /// <summary>
        /// Side, null = Empty
        /// Side, Character = Character
        /// No key = Not used
        /// </summary>
        public Dictionary<NodeMovement.MovementDirectionEnum, Character> Characters = new Dictionary<NodeMovement.MovementDirectionEnum, Character>();

        /// <summary>
        /// Variable, bool = Value
        /// No key = Not used
        /// </summary>
        public Dictionary<Variable, int> Variables = new Dictionary<Variable, int>();

        public ModuleState Clone()
        {
            return new ModuleState
            {
                Characters = new Dictionary<NodeMovement.MovementDirectionEnum, Character>(Characters),
                Variables = new Dictionary<Variable, int>(Variables)
            };
        }

        /// <summary>
        /// Compares whether <paramref name="state"/> is a subset of current state
        /// this.Matches(other) = this contains all the data other does and potentially more
        /// </summary>
        /// <param name="state">Compared state</param>
        /// <returns>Whether <paramref name="state"/> is subset of current state</returns>
        public bool Matches(ModuleState state)
        {
            foreach (var characterPair in state.Characters)
            {
                if (!Characters.ContainsKey(characterPair.Key)) return false;
                if (Characters[characterPair.Key] != characterPair.Value) return false;
            }

            foreach (var variablePair in state.Variables)
            {
                if (!Variables.ContainsKey(variablePair.Key)) return false;
                if (Variables[variablePair.Key] != variablePair.Value) return false;
            }

            return true;
        }

        /// <summary>
        /// Merges state of <paramref name="state"/> into current state
        /// </summary>
        /// <param name="state">State to be merged</param>
        public void Merge(ModuleState state)
        {
            foreach (var characterPair in state.Characters) Characters[characterPair.Key] = characterPair.Value;
            foreach (var variablePair in state.Variables) Variables[variablePair.Key] = variablePair.Value;
        }
    }
}
