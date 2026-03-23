namespace Quantum
{
    using Photon.Deterministic;

    public unsafe class IdleStateConfig : StateConfig
    {
        public string AnimationName = "Idle";

        public override bool CanTransitionTo(Frame frame, StateComponent* currentState)
        {
            return true;
        }

        public override unsafe void EnterState(Frame frame, CharacterMaster* master, KCC2D* kcc, AnimatorComponent* animator)
        {
            base.EnterState(frame, master, kcc, animator);

            // Play idle animation
            animator->FadeTo(frame, 2081823275);
        }

        public override unsafe void UpdateState(Frame frame, CharacterMaster* master, KCC2D* kcc, AnimatorComponent* animator)
        {
            base.UpdateState(frame, master, kcc, animator);
        }
    }
}