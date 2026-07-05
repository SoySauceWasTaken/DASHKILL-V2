using UnityEngine;

namespace Quantum.Menu
{
    /// <summary>
    /// ScriptableObject that holds all data for a single character.
    /// Create assets for each character in your game.
    /// </summary>
    [CreateAssetMenu(fileName = "Character_", menuName = "Quantum/Character Data", order = 1)]
    public class CharacterData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string characterName = "New Character";
        [SerializeField] private string characterDescription = "";
        [SerializeField] private int characterIndex = -1; // Auto-assigned or set manually

        [Header("Visuals")]
        [SerializeField] private Sprite portraitSprite;
        [SerializeField] private Sprite characterIcon;
        [SerializeField] private GameObject characterPreviewPrefab;

        [Header("Gameplay")]
        [SerializeField] private AssetRef<EntityPrototype> characterPrototype; // Quantum entity prototype

        [Header("Metadata")]
        [SerializeField] private bool isUnlockedByDefault = true;
        [SerializeField] private bool isLocked = false;

        // Public getters
        public string CharacterName => characterName;
        public string CharacterDescription => characterDescription;
        public int CharacterIndex => characterIndex;
        public Sprite PortraitSprite => portraitSprite;
        public Sprite CharacterIcon => characterIcon;
        public GameObject CharacterPreviewPrefab => characterPreviewPrefab;
        public AssetRef<EntityPrototype> CharacterPrototype => characterPrototype;
        public bool IsUnlockedByDefault => isUnlockedByDefault;
        public bool IsLocked => isLocked;

#if UNITY_EDITOR
        // Auto-assign index when created (optional)
        private void OnValidate()
        {
            if (characterIndex < 0)
            {
                // Auto-assign based on asset name or order
                // You can also manually set this in the inspector
            }
        }
#endif
    }
}