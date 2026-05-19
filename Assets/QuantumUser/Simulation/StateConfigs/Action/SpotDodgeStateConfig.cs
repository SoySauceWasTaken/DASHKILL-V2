using Photon.Deterministic;
using Quantum;
using UnityEngine;

public unsafe class SpotDodgeStateConfig : StateConfig
{
    public int AnimationId;

    [Header("Dodge Settings")]
    public FP Duration = FP._0_50;  // How long dodge lasts (invincibility frames)
    public FP Cooldown = FP._0_50;  // How long before can dodge again

    public override unsafe void EnterState(Frame frame, EntityRef entity, StateFilter* filter)
    {
        // Play dodge animation
        filter->animator->FadeTo(frame, AnimationId, FP._0, FP._0, FP._0, true, false);

        // Stop all movement during spot dodge
        //filter->kcc->_kinematicVelocity = FPVector2.Zero;

        // Set invincible (if you have invincibility system)
        filter->health->IsInvincible = true;

        // Optional: Set cooldown on action state machine
        

        //Log.Debug($"[SpotDodge] Entity {entity} entered spot dodge for {Duration} seconds");
    }

    public override void UpdateState(Frame frame, EntityRef entity, StateFilter* filter)
    {
        // Keep velocity zero during dodge
    }

    public override bool CanExit(Frame frame, EntityRef entity, StateFilter* filter)
    {
        // Exit when timer reaches duration
        return filter->master->StateTimer >= Duration;
    }

    public override void ExitState(Frame frame, EntityRef entity, StateFilter* filter)
    {
        filter->health->IsInvincible = false;
        filter->actionSM->SpotDodgeCooldown = Cooldown; // Activate Cooldown
    }
}