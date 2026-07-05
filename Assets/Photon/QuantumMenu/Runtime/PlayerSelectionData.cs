using System.Collections.Generic;

namespace Quantum.Menu
{
    /// <summary>
    /// Simulation-agnostic storage for player character selections.
    /// Persists across local and online simulation lifetimes.
    /// </summary>
    public static class PlayerSelectionData
    {
        private static Dictionary<int, int> _playerSelections = new Dictionary<int, int>();

        /// <summary>
        /// Gets or sets the character index for a specific player slot.
        /// </summary>
        public static int GetPlayerSelection(int playerSlot)
        {
            return _playerSelections.TryGetValue(playerSlot, out int index) ? index : -1;
        }

        /// <summary>
        /// Updates the character selection for a specific player slot.
        /// </summary>
        public static void UpdatePlayerSelection(int playerSlot, int characterIndex)
        {
            _playerSelections[playerSlot] = characterIndex;
        }

        /// <summary>
        /// Clears all selections (useful when leaving character select).
        /// </summary>
        public static void Clear()
        {
            _playerSelections.Clear();
        }

        /// <summary>
        /// Gets all selections as a read-only dictionary.
        /// </summary>
        public static IReadOnlyDictionary<int, int> GetAllSelections()
        {
            return _playerSelections;
        }

        /// <summary>
        /// Checks if a player has made a selection.
        /// </summary>
        public static bool HasSelection(int playerSlot)
        {
            return _playerSelections.ContainsKey(playerSlot) && _playerSelections[playerSlot] >= 0;
        }

        /// <summary>
        /// Gets the number of players that have made selections.
        /// </summary>
        public static int SelectionCount => _playerSelections.Count;
    }
}