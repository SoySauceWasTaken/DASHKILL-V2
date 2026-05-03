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

        public override bool CanExit(Frame frame, EntityRef entity, StateFilter* filter)
        {
            // Exit when dash duration is complete
            return filter->master->StateTimer >= DashDuration;
        }

        public override unsafe void EnterState(Frame frame, EntityRef entity, StateFilter* filter)
        {
            base.EnterState(frame, entity, filter);

            // Play dash animation
            filter->animator->FadeTo(frame, AnimationId, FP._0, FP._0, FP._0, true, false);

            // Optionally suspend gravity during dash
            if (SuspendsGravity)
            {
                filter->kcc->_gravityModifier = 0;
                filter->kcc->KinematicVerticalSpeed = 0; // Must null out the existing gravity
            }
        }

        public override unsafe void UpdateState(Frame frame, EntityRef entity, StateFilter* filter)
        {
            base.UpdateState(frame, entity, filter);

            ProcessDash(frame, filter->master, filter->movementData, filter->kcc);
        }

        public override unsafe void ExitState(Frame frame, EntityRef entity, StateFilter* filter)
        {
            base.ExitState(frame, entity, filter);

            if (SuspendsGravity)
            {
                filter->kcc->_gravityModifier = 1; // Reactiviate gravity
            }
        }

        private void ProcessDash(Frame f, CharacterMaster* master, MovementData* movementData, KCC2D* kcc)
        {
            int direction = movementData->FacingDirection;
            kcc->AddForce(f, new FPVector2(DashSpeed * direction, kcc->KinematicVerticalSpeed));
        }
    }
}