namespace DashKill
{
    using Quantum;
    using UnityEngine;

    /// <summary>
    /// Provides a custom context for Quantum views, storing local player-specific data.
    /// </summary>
    public class GameViewContext : QuantumMonoBehaviour, IQuantumViewContext
    {
        public Camera Camera;
    }
}