using System.Xml;
using TMPro;
using UnityEngine;

namespace Quantum.Menu
{
    using UnityEngine.UI;

    /// <summary>
    /// Individual character button in the grid.
    /// Handles UI interaction and invokes events when selected.
    /// </summary>
    public class CharacterButton : MonoBehaviour
    {
        [Header("Button Data")]
        [SerializeField] private int characterIndex;
        [SerializeField] private string characterName;
        [SerializeField] private Sprite characterPortrait;
        [SerializeField] private bool isLocked = false;

        // Unity event that other scripts can subscribe to
        public System.Action<CharacterData, CharacterButton> OnCharacterSelected;

        private Button button;
        private CharacterData characterData;
        private bool isSelected = false;

        private void Awake()
        {
            button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClicked);
            }
        }

        /// <summary>
        /// Initializes the button with character data.
        /// </summary>
        public void Initialize(CharacterData data)
        {
            characterData = data;

            if (characterData == null)
            {
                Debug.LogWarning("CharacterButton: characterData is null!");
                return;
            }
        }

        private void OnButtonClicked()
        {
            if (isLocked) return;

            // In character select, clicking selects the character
            isSelected = true;

            // Invoke the event so the parent screen can handle it
            Debug.Log("(View Layer): Button clickity click");
            OnCharacterSelected?.Invoke(characterData, this);
        }

        public void Select()
        {
            if (characterData == null || characterData.IsLocked) return;
            isSelected = true;
        }

        public void Deselect()
        {
            isSelected = false;
        }

        // Public getters
        public int CharacterIndex => characterIndex;
        public string CharacterName => characterName;
        public Sprite CharacterPortrait => characterPortrait;
        public bool IsLocked => isLocked;
        public bool IsSelected => isSelected;
    }
}