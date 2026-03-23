namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine;

    /// <summary>
    /// A Unity script that polls Unity's input system and converts it to Quantum input.
    /// </summary>
    public class QuantumDemoInputPlatformer2DPolling : MonoBehaviour
    {

        [SerializeField] private UnityInputActionHandler inputHandler; // Reference to your Unity-side input handler

        private void OnEnable()
        {
            Debug.Log($"[{gameObject.name}] QuantumDemoInputPlatformer2DPolling enabled, subscribing to CallbackPollInput");
            QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));

            // Try to find input handler if not assigned
            if (inputHandler == null)
            {
                inputHandler = FindObjectOfType<UnityInputActionHandler>();
                if (inputHandler != null)
                    Debug.Log($"[{gameObject.name}] Found UnityInputActionHandler: {inputHandler.gameObject.name}");
                else
                    Debug.LogWarning($"[{gameObject.name}] No UnityInputActionHandler found in scene!");
            }
        }

        /// <summary>
        /// Convert Unity input to Quantum input when polled by the simulation.
        /// </summary>
        public void PollInput(CallbackPollInput callback)
        {
            QuantumDemoInputPlatformer2D qInput = default;

            // Log raw input values before conversion
            Debug.Log($"[{gameObject.name}] Raw MoveInput: ({inputHandler.MoveInput.x:F2}, {inputHandler.MoveInput.y:F2})");
            Debug.Log($"[{gameObject.name}] Button states - Jump:{inputHandler.JumpInput} Dodge:{inputHandler.DodgeInput} Light:{inputHandler.LightAttackInput} Heavy:{inputHandler.HeavyAttackInput}");

            // Movement direction (combined vector)
            qInput.Direction = new FPVector2(
              FPMath.Clamp(FP.FromFloat_UNSAFE(inputHandler.MoveInput.x), -1, 1),
              FPMath.Clamp(FP.FromFloat_UNSAFE(inputHandler.MoveInput.y), -1, 1)
            );            

            // Core action buttons
            qInput.Jump = inputHandler.JumpInput;
            qInput.Dodge = inputHandler.DodgeInput;
            qInput.LightAttack = inputHandler.LightAttackInput;
            qInput.HeavyAttack = inputHandler.HeavyAttackInput;
            qInput.Throw = inputHandler.ThrowInput;
            qInput.PickUp = inputHandler.PickUpInput;

            // Emotes
            qInput.Taunt1 = inputHandler.Taunt1Input;
            qInput.Taunt2 = inputHandler.Taunt2Input;
            qInput.Taunt3 = inputHandler.Taunt3Input;
            qInput.Taunt4 = inputHandler.Taunt4Input;

            // Log converted Quantum values
            Debug.Log($"[{gameObject.name}] Quantum Direction: ({qInput.Direction.X.AsFloat:F2}, {qInput.Direction.Y.AsFloat:F2})");

            // Send the input to Quantum
            callback.SetInput(qInput, DeterministicInputFlags.Repeatable);
        }
    }
}