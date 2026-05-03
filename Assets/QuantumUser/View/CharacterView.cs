using UnityEngine;
using Quantum;

public class CharacterView : QuantumEntityViewComponent
{
    [SerializeField] private Transform modelTransform;  // The visual model child

    private void Update()
    {
        // Get verified frame
        var frame = QuantumRunner.Default.Game.Frames.Verified;
        if (frame == null) return;

        // Try to get MovementData component from the entity
        if (frame.TryGet(EntityRef, out MovementData movementData))
        {
            // Flip based on facing direction
            if (movementData.FacingDirection != 0)
            {
                Vector3 scale = modelTransform.localScale;
                scale.x = Mathf.Abs(scale.x) * Mathf.Sign(movementData.FacingDirection);
                modelTransform.localScale = scale;
            }
        }
    }
}