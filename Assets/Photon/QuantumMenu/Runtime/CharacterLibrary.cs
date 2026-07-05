using System.Collections.Generic;
using UnityEngine;

namespace Quantum.Menu
{
    /// <summary>
    /// ScriptableObject that holds the entire character roster.
    /// Assign this to your QuantumMenuUICharacterSelect.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterLibrary", menuName = "Quantum/Character Library", order = 1)]
    public class CharacterLibrary : ScriptableObject
    {
        [Header("Character Roster")]
        [SerializeField] private List<CharacterData> allCharacters = new List<CharacterData>();

        [Header("Defaults")]
        [SerializeField] private CharacterData defaultCharacter;
        [SerializeField] private CharacterData randomCharacter; // Optional: represents "Random" pick
        [SerializeField] private AssetRef<EntityPrototype> defaultSelectionPrototype;

        // Public getters
        public IReadOnlyList<CharacterData> AllCharacters => allCharacters;
        public CharacterData DefaultCharacter => defaultCharacter;
        public CharacterData RandomCharacter => randomCharacter;
        public AssetRef<EntityPrototype> DefaultSelectionPrototype => defaultSelectionPrototype;
        public int CharacterCount => allCharacters.Count;

        /// <summary>
        /// Gets a character by index.
        /// </summary>
        public CharacterData GetCharacter(int index)
        {
            if (index >= 0 && index < allCharacters.Count)
            {
                return allCharacters[index];
            }
            return null;
        }

        /// <summary>
        /// Gets a character by name.
        /// </summary>
        public CharacterData GetCharacter(string name)
        {
            return allCharacters.Find(c => c.CharacterName == name);
        }

        /// <summary>
        /// Gets all unlocked characters (excluding locked ones).
        /// </summary>
        public List<CharacterData> GetUnlockedCharacters()
        {
            var unlocked = new List<CharacterData>();
            foreach (var character in allCharacters)
            {
                if (!character.IsLocked)
                {
                    unlocked.Add(character);
                }
            }
            return unlocked;
        }

        /// <summary>
        /// Gets characters that are selectable (unlocked and not hidden).
        /// </summary>
        public List<CharacterData> GetSelectableCharacters()
        {
            // You can add additional filters here (e.g., unlock requirements)
            return GetUnlockedCharacters();
        }
    }
}