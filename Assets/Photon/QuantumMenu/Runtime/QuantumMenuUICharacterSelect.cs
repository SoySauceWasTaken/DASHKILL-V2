using System.Collections.Generic;
using UnityEngine;

namespace Quantum.Menu
{
    using Photon.Deterministic;
    using System.Threading.Tasks;
    using UnityEngine.UI;

    public partial class QuantumMenuUICharacterSelect : QuantumMenuUIScreen
    {
        [Header("UI References")]
        [SerializeField] private Transform podiumContainer;
        [SerializeField] private GameObject characterGrid;
        [SerializeField] private CharacterButton[] characterButtons;
        [SerializeField] private Button readyButton;

        [Header("Prefabs")]
        [SerializeField] private GameObject podiumPrefab;

        [Header("Character Data")]
        [SerializeField] private CharacterLibrary characterLibrary;

        private QuantumRunner _runner;
        private bool _isReady = false;
        private bool _isTransitioning = false;
        private CharacterData _localSelectedCharacter = null;
        private List<CharacterPodiumView> _podiums = new List<CharacterPodiumView>();


        partial void ShowUser();
        partial void HideUser();

        public override void Awake()
        {
            base.Awake();

            if (characterLibrary == null)
            {
                Debug.LogError("QuantumMenuUICharacterSelect: CharacterLibrary not assigned!");
                return;
            }

            // Get selectable characters from the library
            var selectableCharacters = characterLibrary.GetSelectableCharacters();

            // Setup character buttons from the library data
            for (int i = 0; i < characterButtons.Length && i < selectableCharacters.Count; i++)
            {
                var button = characterButtons[i];
                var characterData = selectableCharacters[i];

                // Initialize the button with the character data
                button.Initialize(characterData);

                // Subscribe to the button's selection event
                button.OnCharacterSelected += OnCharacterButtonSelected;
            }

            // Hide any extra buttons beyond our character count
            for (int i = selectableCharacters.Count; i < characterButtons.Length; i++)
            {
                characterButtons[i].gameObject.SetActive(false);
            }

            if (readyButton != null)
            {
                readyButton.onClick.AddListener(OnReadyPressed);
                //readyButton.interactable = false;
            }
        }

        private void SubscribeToEvents()
        {
            QuantumEvent.Subscribe<EventCharacterSelected>(this, OnCharacterSelectedEvent);
            QuantumEvent.Subscribe<EventPlayerReady>(this, OnPlayerReadyEvent);
            QuantumEvent.Subscribe<EventAllPlayersReady>(this, OnAllPlayersReadyEvent);
        }

        private void UnsubscribeFromEvents()
        {
            QuantumEvent.UnsubscribeListener<EventCharacterSelected>(this);
            QuantumEvent.UnsubscribeListener<EventPlayerReady>(this);
            QuantumEvent.UnsubscribeListener<EventAllPlayersReady>(this);
        }

        private void OnCharacterButtonSelected(CharacterData characterData, CharacterButton button)
        {
            // Deselect all other buttons
            foreach (var btn in characterButtons)
            {
                if (btn != button && btn.IsSelected && btn.gameObject.activeSelf)
                {
                    btn.Deselect();
                }
            }

            // Store the selection locally
            _localSelectedCharacter = characterData;

            // 1. UPDATE LOCAL UI immediately (responsive feedback)
            // Get the local player's slot
            if (_runner != null && _runner.Game != null)
            {
                var localSlots = _runner.Game.GetLocalPlayerSlots();
                if (localSlots.Count > 0)
                {
                    int localSlot = localSlots[0]; // First local player
                    if (localSlot < _podiums.Count)
                    {
                        _podiums[localSlot].SetCharacter(characterData.CharacterIndex, characterData);
                    }

                    // 2. UPDATE PLAYER SELECTION DATA (simulation-agnostic)
                    PlayerSelectionData.UpdatePlayerSelection(localSlot, characterData.CharacterIndex);
                }
            }

            // 3. Send Quantum command
            if (_runner != null && _runner.Game != null)
            {
                var command = new CommandSelectCharacter
                {
                    CharacterIndex = characterData.CharacterIndex
                };
                _runner.Game.SendCommand(command);

                // Enable ready button
                readyButton.interactable = true;
            }
        }

        public override async void Show()
        {
            base.Show();

            // Start the local simulation first

            bool success = await StartLocalQuantumSimulation();
            if (!success)
            {
                Controller.Popup("Failed to start local game");
                return;
            }

            _runner = QuantumRunner.Default;
            if (_runner == null || _runner.Game == null)
            {
                Debug.LogError("QuantumMenuUICharacterSelect: No Quantum runner found!");
                return;
            }

            _isReady = false;
            _isTransitioning = false;
            _localSelectedCharacter = null;
            //readyButton.interactable = false;

            // Reset all character buttons
            foreach (var btn in characterButtons)
            {
                if (btn.gameObject.activeSelf)
                    btn.Deselect();
            }

            SubscribeToEvents();
            CreatePodiums();

            if (characterGrid != null)
            {
                characterGrid.SetActive(true);
            }

            ShowUser();
        }

        public override void Hide()
        {
            base.Hide();

            // Unsubscribe from button events
            foreach (var btn in characterButtons)
            {
                btn.OnCharacterSelected -= OnCharacterButtonSelected;
            }

            UnsubscribeFromEvents();
            ClearPodiums();
            HideUser();
        }

        private void CreatePodiums()
        {
            ClearPodiums();

            var frame = _runner.Game.Frames.Verified;
            int playerCount = frame.MaxPlayerCount;

            // Create a podium for each player slot
            for (int i = 0; i < playerCount; i++)
            {
                if (podiumPrefab != null && podiumContainer != null)
                {
                    var podiumObj = Instantiate(podiumPrefab, podiumContainer);
                    var podiumView = podiumObj.GetComponent<CharacterPodiumView>();

                    if (podiumView != null)
                    {
                        podiumView.SetPlayerSlot(i);
                        _podiums.Add(podiumView);
                    }
                }
            }

            // Update podiums with existing state from Quantum
            UpdatePodiumsFromFrame();
        }

        private void ClearPodiums()
        {
            foreach (var podium in _podiums)
            {
                if (podium != null)
                {
                    Destroy(podium.gameObject);
                }
            }
            _podiums.Clear();
        }

        private void UpdatePodiumsFromFrame()
        {
            var frame = _runner.Game.Frames.Verified;

            foreach (var characterPair in frame.GetComponentIterator<CharacterSelect>())
            {
                var characterSelect = characterPair.Component;
                var entity = characterPair.Entity;

                if (frame.TryGet<PlayerLink>(entity, out var playerLink))
                {
                    int slot = playerLink.Player._index;
                    if (slot < _podiums.Count)
                    {
                        int charIndex = characterSelect.SelectedCharacter;
                        CharacterData data = characterLibrary.GetCharacter(charIndex);

                        // Update podium with character data
                        _podiums[slot].SetCharacter(charIndex, data);
                        _podiums[slot].SetReady(characterSelect.IsReady);
                    }
                }
            }
        }

        private void OnReadyPressed()
        {
            if (_isReady || _isTransitioning) return;

            if (_localSelectedCharacter == null)
            {
                Controller.Popup("Please select a character first!");
                return;
            }

            // Send Quantum command to mark ready
            if (_runner != null && _runner.Game != null)
            {
                var command = new CommandPlayerReady
                {
                    IsReady = true
                };
                _runner.Game.SendCommand(command);

                _isReady = true;
                //readyButton.interactable = false;
            }
        }

        private void OnPlayerReadyEvent(EventPlayerReady e)
        {
            int slot = e.Player._index;
            if (slot < _podiums.Count)
            {
                _podiums[slot].SetReady(e.IsReady);
            }

            if (_runner != null && _runner.Game != null && _runner.Game.PlayerIsLocal(e.Player))
            {
                _isReady = e.IsReady;
                Log.Debug("At least this works");
                //if (e.IsReady)
                //{
                //    readyButton.interactable = false;
                //    if (statusText != null)
                //    {
                //        statusText.text = "Waiting for opponent...";
                //    }
                //    if (waitingPanel != null)
                //    {
                //        waitingPanel.SetActive(true);
                //    }
                //}
            }
        }

        private void OnAllPlayersReadyEvent(EventAllPlayersReady e)
        {
            if (_isTransitioning) return;
            _isTransitioning = true;

            Log.Debug("All players are ready! Transitioning to gameplay...");

            //if (statusText != null)
            //{
            //    statusText.text = "All players ready! Starting game...";
            //}

            // For now, just print. Later you'll transition to gameplay.
            // TransitionToGameplay();
        }

        private void OnCharacterSelectedEvent(EventCharacterSelected e)
        {
            // Get the character data
            CharacterData data = characterLibrary.GetCharacter(e.CharacterIndex);

            // 1. UPDATE PODIUM UI (preview)
            int slot = e.Player._index;
            if (slot < _podiums.Count)
            {
                _podiums[slot].SetCharacter(e.CharacterIndex, data);
            }

            // 2. UPDATE PLAYER SELECTION DATA (simulation-agnostic storage)
            PlayerSelectionData.UpdatePlayerSelection(e.Player._index, e.CharacterIndex);

            // 3. Update the local cached selection if this is the local player
            if (QuantumRunner.Default.Game.PlayerIsLocal(e.Player))
            {
                _localSelectedCharacter = data;
                readyButton.interactable = true;
            }
        }

        private async Task<bool> StartLocalQuantumSimulation()
        {
            try
            {
                // Get the map from your RuntimeConfig
                // You can load it from a default map asset or pass it in
                var map = new AssetRef<Map>(QuantumUnityDB.GetGlobalAssetGuid("QuantumUser/Resources/MainMenuMap"));
                var simulationConfig = new AssetRef<SimulationConfig>(QuantumUnityDB.GetGlobalAssetGuid("QuantumUser/Resources/QuantumDefaultConfigs|DefaultConfigSimulation"));
                var systemsConfig = new AssetRef<SystemsConfig>(QuantumUnityDB.GetGlobalAssetGuid("QuantumUser/Resources/QuantumDefaultConfigs|DefaultConfigSystems"));

                // Create RuntimeConfig
                var runtimeConfig = new RuntimeConfig
                {
                    Map = map,
                    Seed = System.DateTime.Now.Millisecond,
                    SimulationConfig = simulationConfig,
                    SystemsConfig = systemsConfig
                };

                // Create session arguments for LOCAL game
                var sessionRunnerArguments = new SessionRunner.Arguments
                {
                    RunnerFactory = QuantumRunnerUnityFactory.DefaultFactory,
                    GameParameters = QuantumRunnerUnityFactory.CreateGameParameters,
                    ClientId = "local_client",
                    RuntimeConfig = runtimeConfig,
                    SessionConfig = QuantumDeterministicSessionConfigAsset.DefaultConfig,
                    GameMode = DeterministicGameMode.Local,
                    PlayerCount = 1, // only local for soloQ
                    StartGameTimeoutInSeconds = 10,
                    Communicator = null, // No network for local
                };

                // Start the simulation
                var runner = (QuantumRunner)await SessionRunner.StartAsync(sessionRunnerArguments);

                // Add local player
                var player = new RuntimePlayer
                {
                    PlayerNickname = ConnectionArgs.Username,
                    SelectionPrototype = characterLibrary.DefaultSelectionPrototype,
                    SelectedCharacterIndex = -1 // We don't have a character selected yet. This is updated ONLY when we've gotten into the gameplay Sim
                };

                runner.Game.AddPlayer(0, player); // IMPORTANT!!! "Player" here means the "human/connection" NOT the avatar/entity (aka the in-game character)

                _runner = runner;
                Debug.Log("Local Quantum simulation started successfully!");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to start local Quantum simulation: {e.Message}");
                return false;
            }
        }
    }
}