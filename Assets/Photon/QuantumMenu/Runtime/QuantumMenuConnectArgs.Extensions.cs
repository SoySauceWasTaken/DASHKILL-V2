namespace Quantum.Menu
{
    /// <summary>
    /// Extends Photon's QuantumMenuConnectArgs with custom flags for our game.
    /// This is a partial class that merges with Photon's original definition.
    /// </summary>
    public partial class QuantumMenuConnectArgs
    {
        /// <summary>
        /// When true, the menu will load QuantumMenuUICharacterSelect after a successful connection.
        /// When false, it loads QuantumMenuUIGameplay (the default gameplay screen).
        /// </summary>
        public bool IsCharacterSelectMode = false;
    }
}