namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine;

    public unsafe class DashStateConfig : StateConfig
    {
        public int AnimationId = 0;

        [Header("Dash Movement")]
        public FP DashSpeed = 15;
        public FP DashDuration = FP._0_25;  // How long dash lasts
        public bool SuspendsGravity = true;

        public override bool CanTransitionTo(Frame frame, CharacterMaster* master, StateType currentState)
        {
            return master->StateTimer >= DashDuration;
        }

        public override bool CanExit(Frame frame, EntityRef entity, CharacterMaster* master, KCC2D* kcc)
        {
            // Exit when dash duration is complete
            return master->StateTimer >= DashDuration;
        }

        public override unsafe void EnterState(Frame frame, CharacterMaster* master, KCC2D* kcc, AnimatorComponent* animator)
        {
            base.EnterState(frame, master, kcc, animator);

            // Play dash animation
            animator->FadeTo(frame, AnimationId, FP._0, FP._0, FP._0, true, false);

            // Optionally suspend gravity during dash
            if (SuspendsGravity)
            {
                kcc->_gravityModifier = 0;
                kcc->KinematicVerticalSpeed = 0; // Must null out the existing gravity
            }
        }

        public override unsafe void UpdateState(Frame frame, CharacterMaster* master, KCC2D* kcc, AnimatorComponent* animator)
        {
            base.UpdateState(frame, master, kcc, animator);

            ProcessDash(frame, master, kcc);
        }

        public override unsafe void ExitState(Frame frame, CharacterMaster* master, KCC2D* kcc, AnimatorComponent* animator)
        {
            base.ExitState(frame, master, kcc, animator);

            if (SuspendsGravity)
            {
                kcc->_gravityModifier = 1; // Reactiviate gravity
            }
        }

        private void ProcessDash(Frame f, CharacterMaster* master, KCC2D* kcc)
        {
            int direction = master->MovementData.FacingDirection;
            kcc->AddForce(f, new FPVector2(DashSpeed * direction, kcc->KinematicVerticalSpeed));
        }
    }
}