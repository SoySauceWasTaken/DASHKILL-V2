namespace Quantum
{
    using Photon.Deterministic;

    public unsafe class RunStateConfig : StateConfig
    {
        public string AnimationName = "Run";

        // Run-specific data (tunable in Inspector!)
        public FP MoveSpeed = 5;
        public FP Acceleration = 10;
        public FP TurnSpeed = 15;  // How quickly direction changes

        public override bool CanTransitionTo(Frame frame, StateComponent* currentState)
        {
            return true;
        }

        public override unsafe void EnterState(Frame frame, CharacterMaster* master, KCC2D* kcc, AnimatorComponent* animator)
        {
            base.EnterState(frame, master, kcc, animator);

            // Play run animation
            animator->FadeTo(frame, 1748754976);
        }

        public override unsafe void UpdateState(Frame frame, CharacterMaster* master, KCC2D* kcc, AnimatorComponent* animator)
        {
            base.UpdateState(frame, master, kcc, animator);
        }
    }
}