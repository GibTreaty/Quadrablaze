using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Quadrablaze.GameModes {
    [CreateAssetMenu(menuName = "Quadrablaze/Modes/Game Mode Database")]
    public class GameModeDatabase : ScriptableObject {

        [SerializeField]
        List<ScriptableGameMode> _entries = new List<ScriptableGameMode>();

        public int Count => _entries.Count;

        public List<ScriptableGameMode> Entries => _entries;

        public ScriptableGameMode this[int index] {
            get => _entries[index];
        }
        public ScriptableGameMode this[string name] {
            get => _entries.FirstOrDefault(s => s.name == name);
        }
    }
}