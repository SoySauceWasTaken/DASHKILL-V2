namespace Quantum
{
    using Photon.Deterministic;
    using System.Collections.Generic;

    /// <summary>
    /// Handles velocity window events from animations.
    /// Smoothly interpolates character velocity from StartVelocity to EndVelocity
    /// over the duration of the animation time window.
    /// </summary>
    public unsafe class VelocityWindowSystem : SystemMainThread, ISignalOnAnimatorSetVelocity, ISignalOnAnimatorVelocityLerp
    {
        public void OnAnimatorSetVelocity(Frame f, EntityRef entity, FPVector2 velocity)
        {
            KCC2D* kcc = f.Unsafe.GetPointer<KCC2D>(entity);
            CharacterMaster* master = f.Unsafe.GetPointer<CharacterMaster>(entity);

            int direction = master->MovementData.FacingDirection;
            kcc->AddForce(f, new FPVector2(velocity.X * direction, velocity.Y));
        }

        public void OnAnimatorVelocityLerp(Frame f, EntityRef entity, FPVector2 startVelocity, FPVector2 endVelocity, FP StartTime, FP EndTime, FP CurrentTime)
        {
            KCC2D* kcc = f.Unsafe.GetPointer<KCC2D>(entity);
            CharacterMaster* master = f.Unsafe.GetPointer<CharacterMaster>(entity);

            // Calculate interpolation factor T (0 to 1 within window)
            FP duration = EndTime - StartTime;
            FP t = FP._0;

            if (duration > FP._0)
            {
                t = (CurrentTime - StartTime) / duration;
                t = FPMath.Clamp01(t);  // Clamp between 0 and 1
            }

            // Apply facing direction to velocities (convert from local to world space)
            int facing = master->MovementData.FacingDirection;
            FPVector2 adjustedStart = new FPVector2(startVelocity.X * facing, startVelocity.Y);
            FPVector2 adjustedEnd = new FPVector2(endVelocity.X * facing, endVelocity.Y);

            // LERP formula: A + (B - A) * T
            FPVector2 newVelocity = adjustedStart + (adjustedEnd - adjustedStart) * t;

            // Apply to KCC
            kcc->_kinematicVelocity = newVelocity;
        }

        public override void Update(Frame f)
        {

        }
    }
}