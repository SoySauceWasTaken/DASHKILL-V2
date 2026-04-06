namespace Quantum
{
    using Photon.Deterministic;

    public unsafe class IdleStateConfig : StateConfig
    {
        public int AnimationId = -1967403633;

        public override bool CanTransitionTo(Frame frame, StateComponent* currentState)
        {
            return true;
        }

        public override unsafe void EnterState(Frame frame, CharacterMaster* master, KCC2D* kcc, AnimatorComponent* animator)
        {
            base.EnterState(frame, master, kcc, animator);

            // Play idle animation
            animator->FadeTo(frame, AnimationId, FP._0, FP._0, FP._0, true, false);
        }

        public override unsafe void UpdateState(Frame frame, CharacterMaster* master, KCC2D* kcc, AnimatorComponent* animator)
        {
            base.UpdateState(frame, master, kcc, animator);

            var kccConfig = frame.FindAsset(kcc->Config);

            IntegrateForces(frame, master, kcc, kccConfig);
        }

        private void IntegrateForces(Frame f, CharacterMaster* master, KCC2D* KCC, KCC2DConfig kccConfig)
        {
            // TODO: Give this class its own Deceleration settings and extract them out of KCC2DConfig
            if (KCC->State == KCCState.GROUNDED)
            {
                KCC->_kinematicVelocity *= FPMath.Clamp01(1 - kccConfig.Deceleration * f.DeltaTime);
            }
            else
            {
                KCC->KinematicHorizontalSpeed *= FPMath.Clamp01(1 - kccConfig.DecelerationOnAir * f.DeltaTime);
            }
        }
    }
}