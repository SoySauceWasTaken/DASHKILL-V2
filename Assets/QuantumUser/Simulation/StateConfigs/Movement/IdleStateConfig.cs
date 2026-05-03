namespace Quantum
{
    using Photon.Deterministic;

    public unsafe class IdleStateConfig : StateConfig
    {
        public int AnimationId = -1967403633;

        public override bool CanTransitionTo(Frame frame, CharacterMaster* master, StateType currentState)
        {
            return true;
        }

        public override unsafe void EnterState(Frame frame, EntityRef entity, StateFilter* filter)
        {
            base.EnterState(frame, entity, filter);

            // Play idle animation
            filter->animator->FadeTo(frame, AnimationId, FP._0, FP._0, FP._0, true, false);
        }

        public override unsafe void UpdateState(Frame frame, EntityRef entity, StateFilter* filter)
        {
            base.UpdateState(frame, entity, filter);

            var kccConfig = frame.FindAsset(filter->kcc->Config);

            IntegrateForces(frame, filter->master, filter->kcc, kccConfig);
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