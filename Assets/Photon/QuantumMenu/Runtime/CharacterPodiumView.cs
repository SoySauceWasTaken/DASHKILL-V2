using UnityEngine;
using TMPro;

namespace Quantum.Menu
{
    /// <summary>
    /// Individual podium component that displays a player's character selection,
    /// ready status, and name.
    /// </summary>
    public class CharacterPodiumView : QuantumMonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private GameObject characterPreview; // Single preview GameObject per podium
        [SerializeField] private GameObject readyCheckmark;
        [SerializeField] private GameObject loadingSpinner;

        [Header("State")]
        [SerializeField] private int playerSlot = -1;
        [SerializeField] private bool isReady = false;
        [SerializeField] private int selectedCharacterIndex = -1;

        private Animator previewAnimator;

        /// <summary>
        /// The player slot this podium represents (0-based).
        /// </summary>
        public int PlayerSlot => playerSlot;

        /// <summary>
        /// Whether the player is ready.
        /// </summary>
        public bool IsReady => isReady;

        /// <summary>
        /// The selected character index (-1 if none selected).
        /// </summary>
        public int SelectedCharacterIndex => selectedCharacterIndex;

        private void Awake()
        {
            // Cache the animator
            if (characterPreview != null)
            {
                previewAnimator = characterPreview.GetComponent<Animator>();
            }

            // Hide everything initially
            SetReady(false);
            SetCharacter(-1, null);
            SetPlayerName("Waiting...");
        }

        /// <summary>
        /// Sets the player slot and displays appropriate label.
        /// </summary>
        /// <param name="slot">Player slot (0-based)</param>
        public void SetPlayerSlot(int slot)
        {
            playerSlot = slot;
            SetPlayerName($"Player {slot + 1}");
        }

        /// <summary>
        /// Sets the player's name.
        /// </summary>
        /// <param name="name">Player name</param>
        public void SetPlayerName(string name)
        {
            if (playerNameText != null)
            {
                playerNameText.text = name;
            }
        }

        /// <summary>
        /// Sets the character display by playing the appropriate animation.
        /// </summary>
        /// <param name="characterIndex">Character index (-1 for none)</param>
        /// <param name="data">CharacterData containing the character name for animation lookup</param>
        public void SetCharacter(int characterIndex, CharacterData data)
        {
            selectedCharacterIndex = characterIndex;

            if (data != null && characterPreview != null)
            {
                // Show the preview GameObject
                characterPreview.SetActive(true);

                // Play the selection animation
                if (previewAnimator != null)
                {
                    string animationName = $"{data.CharacterName}Preview_Selected";
                    previewAnimator.Play(animationName, 0, 0f);
                }

                if (statusText != null)
                {
                    statusText.text = $"Selected: {data.CharacterName}";
                }
            }
            else
            {
                // Hide the preview when no character is selected
                if (characterPreview != null)
                {
                    characterPreview.SetActive(false);
                }

                if (statusText != null)
                {
                    statusText.text = "Selecting...";
                }
            }
        }

        /// <summary>
        /// Sets the ready status.
        /// </summary>
        /// <param name="ready">Is the player ready?</param>
        public void SetReady(bool ready)
        {
            isReady = ready;

            if (readyCheckmark != null)
            {
                readyCheckmark.SetActive(ready);
            }

            if (loadingSpinner != null)
            {
                loadingSpinner.SetActive(!ready && selectedCharacterIndex >= 0);
            }

            if (statusText != null && selectedCharacterIndex >= 0)
            {
                statusText.text = ready ? "Ready!" : "Selecting...";
            }
        }

        public void ResetPodium()
        {
            SetCharacter(-1, null);
            SetReady(false);
            SetPlayerName("Waiting...");

            if (characterPreview != null)
            {
                characterPreview.SetActive(false);
            }
        }
    }
}