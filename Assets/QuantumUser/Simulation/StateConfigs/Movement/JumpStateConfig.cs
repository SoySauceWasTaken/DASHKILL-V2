namespace Quantum
{
    using Photon.Deterministic;

    public unsafe class JumpStateConfig : StateConfig
    {
        public int AnimationId = 0;
        public FP JumpImpulse;

        public override bool CanTransitionTo(Frame frame, CharacterMaster* master, StateType currentState)
        {
            return true;
        }

        public override unsafe void EnterState(Frame frame, CharacterMaster* master, KCC2D* kcc, AnimatorComponent* animator)
        {
            base.EnterState(frame, master, kcc, animator);
            animator->FadeTo(frame, AnimationId, FP._0, FP._0, FP._0, true, false);

            ProcessJump(frame, kcc);
        }

        private void ProcessJump(Frame f, KCC2D* kcc)
        {
            kcc->AddForce(f, new FPVector2(kcc->KinematicHorizontalSpeed, JumpImpulse));
            kcc->SetState(f, KCCState.JUMPED);
        }
    }
}