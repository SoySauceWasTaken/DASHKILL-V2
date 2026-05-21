namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine;
    /// <summary>
    /// The <c>StatusData</c> is a data asset for store initial values for character status component.
    /// </summary>
    public class StatusData : AssetObject
    {
        [Tooltip("Time to wait in seconds until broadcasting 'OnPlayerDisconnected' after the player disconnects.")]
        public FP TimeToDisconnect = 1;
    }
}