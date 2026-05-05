using Quantum;
using UnityEngine;

[CreateAssetMenu(menuName = "Quantum/PlayerSpawnConfig", fileName = "PlayerSpawnConfig")]
public class PlayerSpawnConfig : ScriptableObject
{
    public AssetRef<EntityPrototype> Avatar;
}