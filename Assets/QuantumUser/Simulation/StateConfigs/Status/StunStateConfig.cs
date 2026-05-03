using Photon.Deterministic;
using Quantum;

public unsafe class StunStateConfig : StateConfig
{
    public int AnimationId = 0;

    public override unsafe void EnterState(Frame frame, EntityRef entity, StateFilter* filter)
    {
        base.EnterState(frame, entity, filter);

        filter->status->IsStunned = false;

        // Get the hitbox config that caused this stun
        HitBoxConfig hitBoxConfig = GetHitBoxConfig(frame, filter);

        // 1. Apply knockback to KCC
        ApplyKnockback(frame, filter, hitBoxConfig);

        // 2. Play hit reaction animation
        filter->animator->FadeTo(frame, AnimationId, FP._0, FP._0, FP._0, true, false);

        // 5. Reset state timer (inherited from StateConfig)
        filter->master->StateTimer = FP._0;
    }

    public override bool CanExit(Frame frame, EntityRef entity, StateFilter* filter)
    {
        // Exit stun when timer expires
        HitBoxConfig hitBoxConfig = GetHitBoxConfig(frame, filter);

        return filter->master->StateTimer >= hitBoxConfig.StunDuration;
    }

    public override unsafe void UpdateState(Frame frame, EntityRef entity, StateFilter* filter)
    {
        base.UpdateState(frame, entity, filter);
    }

    private HitBoxConfig GetHitBoxConfig(Frame frame, StateFilter* filter)
    {
        return frame.FindAsset<HitBoxConfig>(filter->status->HitBoxConfig.Id);
    }

    public override unsafe void ExitState(Frame frame, EntityRef entity, StateFilter* filter)
    {
        base.ExitState(frame, entity, filter);
    }

    private void ApplyKnockback(Frame frame, StateFilter* filter, HitBoxConfig hitBoxConfig)
    {
        if (hitBoxConfig.Knockback == FPVector2.Zero) return;

        // Get attacker's position for knockback direction (if needed)
        FPVector2 finalKnockback = hitBoxConfig.Knockback;

        MovementData* movementData = frame.Unsafe.GetPointer<MovementData>(filter->status->Attacker);

        // Apply knockback in the direction the attacker is facing
        // Positive X knockback = away from attacker's facing direction
        finalKnockback = new FPVector2(
            hitBoxConfig.Knockback.X * movementData->FacingDirection,
            hitBoxConfig.Knockback.Y
        );

        Log.Debug($"[StunState] Knockback direction: {movementData->FacingDirection}, final: {finalKnockback}");

        // Apply knockback to KCC
        filter->kcc->_kinematicVelocity = finalKnockback;
    }
}