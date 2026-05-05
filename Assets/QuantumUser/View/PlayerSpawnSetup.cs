using UnityEngine;
using Quantum;

/// <summary>
/// Sets up the RuntimePlayer for the local client when the game starts.
/// Attach this to a GameObject in your Game scene (e.g., QuantumRunner or a dedicated spawn manager).
/// </summary>
public class PlayerSpawnSetup : MonoBehaviour
{
    [Header("Player Configuration")]
    [SerializeField] private AssetRef<EntityPrototype> _playerAvatar;
    [SerializeField] private string _playerName = "ThaiTanic";

    [Header("Debug")]
    [SerializeField] private bool _autoSetupOnStart = true;
    [SerializeField] private bool _logDebug = true;

    private bool _playerAdded = false;

    private void Start()
    {
        if (_autoSetupOnStart)
        {
            SetupRuntimePlayer();
        }
    }

    /// <summary>
    /// Sets up and adds the RuntimePlayer for the local client.
    /// Called automatically or can be called manually.
    /// </summary>
    public void SetupRuntimePlayer()
    {
        if (_playerAdded)
        {
            if (_logDebug) Debug.Log("[PlayerSpawnSetup] Player already added, skipping.");
            return;
        }

        QuantumRunner runner = QuantumRunner.Default;
        if (runner == null)
        {
            if (_logDebug) Debug.LogError("[PlayerSpawnSetup] QuantumRunner.Default is null! Make sure Quantum is initialized.");
            return;
        }

        if (runner.Game.PlayerIsLocal(0))
        {
            if (_logDebug) Debug.Log("[PlayerSpawnSetup] Player 0 already exists, skipping AddPlayer");
            _playerAdded = true;
            return;
        }

        // Create RuntimePlayer with avatar and name
        RuntimePlayer playerData = new RuntimePlayer();
        playerData.PlayerAvatar = _playerAvatar;
        playerData.PlayerNickname = GetPlayerName();

        if (_logDebug) Debug.Log($"[PlayerSpawnSetup] Adding player: Name={playerData.PlayerNickname}, Avatar={playerData.PlayerAvatar.Id}");

        // Add player to the game - THIS triggers OnPlayerAdded in simulation
        runner.Game.AddPlayer(playerData);

        _playerAdded = true;

        if (_logDebug) Debug.Log("[PlayerSpawnSetup] Player added successfully!");
    }

    /// <summary>
    /// Gets the player name from Quantum menu or uses fallback.
    /// </summary>
    private string GetPlayerName()
    {
        // Try to get name from menu
        var menu = FindAnyObjectByType<Quantum.Menu.QuantumMenuUIController>();
        if (menu != null && !string.IsNullOrEmpty(menu.ConnectArgs.Username))
        {
            return menu.ConnectArgs.Username;
        }

        // Fallback to configured name
        return _playerName;
    }

    /// <summary>
    /// Public method to manually set avatar and setup (for character select screens).
    /// </summary>
    //public void SetupWithAvatar(AssetRef<EntityPrototype> avatar, string playerName = null)
    //{
    //    _playerAvatar = avatar;
    //    if (!string.IsNullOrEmpty(playerName))
    //        _playerName = playerName;
    //    SetupRuntimePlayer();
    //}
}