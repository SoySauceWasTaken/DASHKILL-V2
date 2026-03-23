using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace Quantum
{
    public class UnityInputActionHandler : MonoBehaviour
    {

        [Header("Input State - Read by Quantum")]
        [HideInInspector] public Vector2 MoveInput;          // Left/Right/Up/Down
        [HideInInspector] public bool JumpInput;
        [HideInInspector] public bool DodgeInput;
        [HideInInspector] public bool LightAttackInput;
        [HideInInspector] public bool HeavyAttackInput;
        [HideInInspector] public bool ThrowInput;
        [HideInInspector] public bool PickUpInput;
        [HideInInspector] public bool Taunt1Input;
        [HideInInspector] public bool Taunt2Input;
        [HideInInspector] public bool Taunt3Input;
        [HideInInspector] public bool Taunt4Input;

        [Header("Input Action References")]
        [SerializeField] private InputActionAsset inputActions;

        // Input Action references
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction dodgeAction;
        private InputAction lightAttackAction;
        private InputAction heavyAttackAction;
        private InputAction throwAction;
        private InputAction pickUpAction;
        private InputAction taunt1Action;
        private InputAction taunt2Action;
        private InputAction taunt3Action;
        private InputAction taunt4Action;

        // Conditional check (you can expand this based on your game state)
        [HideInInspector] public bool CanReceiveInput = true;

        // Events for other systems to hook into (optional)
        public static Action OnLocalPlayerSpawned;
        public Action OnAnyInputPressed;

        private void Awake()
        {
            // Find and store all input actions
            if (inputActions != null)
            {
                moveAction = inputActions.FindAction("Move");
                jumpAction = inputActions.FindAction("Jump");
                dodgeAction = inputActions.FindAction("Dodge");
                lightAttackAction = inputActions.FindAction("LightAttack");
                heavyAttackAction = inputActions.FindAction("HeavyAttack");
                throwAction = inputActions.FindAction("Throw");
                pickUpAction = inputActions.FindAction("PickUp");
                taunt1Action = inputActions.FindAction("Taunt1");
                taunt2Action = inputActions.FindAction("Taunt2");
                taunt3Action = inputActions.FindAction("Taunt3");
                taunt4Action = inputActions.FindAction("Taunt4");


                foreach (var map in inputActions.actionMaps)
                {
                    map.Enable();
                }
                Debug.Log("Input Action Asset enabled!");
            }
            else
            {
                Debug.LogError("Input Action Asset not assigned to UnityInputActionHandler!", this);
            }
        }

        private void OnEnable()
        {
            SubscribeToInputActions();
        }

        private void OnDisable()
        {
            UnsubscribeFromInputActions();
        }

        private void SubscribeToInputActions()
        {
            if (moveAction != null)
            {
                moveAction.started += OnMove;
                moveAction.performed += OnMove;
                moveAction.canceled += OnMove;
            }

            if (jumpAction != null)
            {
                jumpAction.started += OnJump;
                jumpAction.performed += OnJump;
                jumpAction.canceled += OnJump;
            }

            if (dodgeAction != null)
            {
                dodgeAction.started += OnDodge;
                dodgeAction.performed += OnDodge;
                dodgeAction.canceled += OnDodge;
            }

            if (lightAttackAction != null)
            {
                lightAttackAction.started += OnLightAttack;
                lightAttackAction.performed += OnLightAttack;
                lightAttackAction.canceled += OnLightAttack;
            }

            if (heavyAttackAction != null)
            {
                heavyAttackAction.started += OnHeavyAttack;
                heavyAttackAction.performed += OnHeavyAttack;
                heavyAttackAction.canceled += OnHeavyAttack;
            }

            if (throwAction != null)
            {
                throwAction.started += OnThrow;
                throwAction.performed += OnThrow;
                throwAction.canceled += OnThrow;
            }

            if (pickUpAction != null)
            {
                pickUpAction.started += OnPickUp;
                pickUpAction.performed += OnPickUp;
                pickUpAction.canceled += OnPickUp;
            }

            if (taunt1Action != null)
            {
                taunt1Action.started += OnTaunt1;
                taunt1Action.performed += OnTaunt1;
                taunt1Action.canceled += OnTaunt1;
            }

            if (taunt2Action != null)
            {
                taunt2Action.started += OnTaunt2;
                taunt2Action.performed += OnTaunt2;
                taunt2Action.canceled += OnTaunt2;
            }

            if (taunt3Action != null)
            {
                taunt3Action.started += OnTaunt3;
                taunt3Action.performed += OnTaunt3;
                taunt3Action.canceled += OnTaunt3;
            }

            if (taunt4Action != null)
            {
                taunt4Action.started += OnTaunt4;
                taunt4Action.performed += OnTaunt4;
                taunt4Action.canceled += OnTaunt4;
            }
        }

        private void UnsubscribeFromInputActions()
        {
            if (moveAction != null)
            {
                moveAction.started -= OnMove;
                moveAction.performed -= OnMove;
                moveAction.canceled -= OnMove;
            }

            if (jumpAction != null)
            {
                jumpAction.started -= OnJump;
                jumpAction.performed -= OnJump;
                jumpAction.canceled -= OnJump;
            }

            if (dodgeAction != null)
            {
                dodgeAction.started -= OnDodge;
                dodgeAction.performed -= OnDodge;
                dodgeAction.canceled -= OnDodge;
            }

            if (lightAttackAction != null)
            {
                lightAttackAction.started -= OnLightAttack;
                lightAttackAction.performed -= OnLightAttack;
                lightAttackAction.canceled -= OnLightAttack;
            }

            if (heavyAttackAction != null)
            {
                heavyAttackAction.started -= OnHeavyAttack;
                heavyAttackAction.performed -= OnHeavyAttack;
                heavyAttackAction.canceled -= OnHeavyAttack;
            }

            if (throwAction != null)
            {
                throwAction.started -= OnThrow;
                throwAction.performed -= OnThrow;
                throwAction.canceled -= OnThrow;
            }

            if (pickUpAction != null)
            {
                pickUpAction.started -= OnPickUp;
                pickUpAction.performed -= OnPickUp;
                pickUpAction.canceled -= OnPickUp;
            }

            if (taunt1Action != null)
            {
                taunt1Action.started -= OnTaunt1;
                taunt1Action.performed -= OnTaunt1;
                taunt1Action.canceled -= OnTaunt1;
            }

            if (taunt2Action != null)
            {
                taunt2Action.started -= OnTaunt2;
                taunt2Action.performed -= OnTaunt2;
                taunt2Action.canceled -= OnTaunt2;
            }

            if (taunt3Action != null)
            {
                taunt3Action.started -= OnTaunt3;
                taunt3Action.performed -= OnTaunt3;
                taunt3Action.canceled -= OnTaunt3;
            }

            if (taunt4Action != null)
            {
                taunt4Action.started -= OnTaunt4;
                taunt4Action.performed -= OnTaunt4;
                taunt4Action.canceled -= OnTaunt4;
            }
        }

        // ========== INPUT HANDLERS ==========

        public void OnMove(InputAction.CallbackContext context)
        {
            if (context.started || context.performed)
            {
                if (CanReceiveInput)
                    MoveInput = context.ReadValue<Vector2>();

                Debug.Log("Reading input: " + MoveInput);
            }
            else if (context.canceled)
            {
                MoveInput = Vector2.zero;
            }
            OnAnyInputPressed?.Invoke();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.started) { if (CanReceiveInput) JumpInput = true; }
            else if (context.canceled) JumpInput = false;
            if (context.started) OnAnyInputPressed?.Invoke();
        }

        public void OnDodge(InputAction.CallbackContext context)
        {
            if (context.started) { if (CanReceiveInput) DodgeInput = true; }
            else if (context.canceled) DodgeInput = false;
            if (context.started) OnAnyInputPressed?.Invoke();
        }

        public void OnLightAttack(InputAction.CallbackContext context)
        {
            if (context.started) { if (CanReceiveInput) LightAttackInput = true; }
            else if (context.canceled) LightAttackInput = false;
            if (context.started) OnAnyInputPressed?.Invoke();
        }

        public void OnHeavyAttack(InputAction.CallbackContext context)
        {
            if (context.started) { if (CanReceiveInput) HeavyAttackInput = true; }
            else if (context.canceled) HeavyAttackInput = false;
            if (context.started) OnAnyInputPressed?.Invoke();
        }

        public void OnThrow(InputAction.CallbackContext context)
        {
            if (context.started) { if (CanReceiveInput) ThrowInput = true; }
            else if (context.canceled) ThrowInput = false;
            if (context.started) OnAnyInputPressed?.Invoke();
        }

        public void OnPickUp(InputAction.CallbackContext context)
        {
            if (context.started) { if (CanReceiveInput) PickUpInput = true; }
            else if (context.canceled) PickUpInput = false;
            if (context.started) OnAnyInputPressed?.Invoke();
        }

        public void OnTaunt1(InputAction.CallbackContext context)
        {
            if (context.started) { if (CanReceiveInput) Taunt1Input = true; }
            else if (context.canceled) Taunt1Input = false;
            if (context.started) OnAnyInputPressed?.Invoke();
        }

        public void OnTaunt2(InputAction.CallbackContext context)
        {
            if (context.started) { if (CanReceiveInput) Taunt2Input = true; }
            else if (context.canceled) Taunt2Input = false;
            if (context.started) OnAnyInputPressed?.Invoke();
        }

        public void OnTaunt3(InputAction.CallbackContext context)
        {
            if (context.started) { if (CanReceiveInput) Taunt3Input = true; }
            else if (context.canceled) Taunt3Input = false;
            if (context.started) OnAnyInputPressed?.Invoke();
        }

        public void OnTaunt4(InputAction.CallbackContext context)
        {
            if (context.started) { if (CanReceiveInput) Taunt4Input = true; }
            else if (context.canceled) Taunt4Input = false;
            if (context.started) OnAnyInputPressed?.Invoke();
        }

        // ========== HELPER METHODS ==========

        /// <summary>
        /// Call this to reset all input states (useful during round resets, respawns, etc.)
        /// </summary>
        public void ResetAllInputs()
        {
            MoveInput = Vector2.zero;
            JumpInput = false;
            DodgeInput = false;
            LightAttackInput = false;
            HeavyAttackInput = false;
            ThrowInput = false;
            PickUpInput = false;
            Taunt1Input = false;
            Taunt2Input = false;
            Taunt3Input = false;
            Taunt4Input = false;
        }

        /// <summary>
        /// Temporarily block/unblock input receipt
        /// </summary>
        public void SetCanReceiveInput(bool canReceive)
        {
            CanReceiveInput = canReceive;
            if (!canReceive)
            {
                // Clear all inputs when disabling
                ResetAllInputs();
            }
        }
    }
}