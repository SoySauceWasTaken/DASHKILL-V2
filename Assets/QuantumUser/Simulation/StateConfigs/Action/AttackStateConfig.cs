namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine;

    public unsafe class AttackStateConfig : StateConfig
    {
        public int AnimationId = 0;

        [Header("Attack Movement")]
        public FP DashSpeed = 15;
        public FP DashDuration = FP._0_25;  // How long dash lasts
        public bool SuspendsGravity = true;

        public override bool CanTransitionTo(Frame frame, CharacterMaster* master, StateType currentState)
        {
            return master->StateTimer >= DashDuration;
        }

        public override bool CanExit(Frame frame, EntityRef entity, StateFilter* filter)
        {
            // Get the AnimatorComponent from the entity
            if (frame.TryGet(entity, out AnimatorComponent animator))
            {

                // Get the current state Id from the base layer
                var layers = frame.ResolveList(animator.Layers);
                if (layers.Count > 0)
                {
                    LayerData* layerData = layers.GetPointer(0);

                    // Get the graph asset
                    var graph = frame.FindAsset<AnimatorGraph>(animator.AnimatorGraph.Id);

                    // Get the state by Id and retrieve its length
                    var currentState = graph.GetState(AnimationId);
                    FP animationLength = currentState.GetLength(frame, layerData);

                    //Log.Debug($"Animation State: Name: {currentState.Name}, Length {animationLength}");

                    // Exit when animation completes
                    return filter->master->StateTimer >= animationLength;
                }
            }

            return false;
        }

        public override unsafe void EnterState(Frame frame, EntityRef entity, StateFilter* filter)
        {
            base.EnterState(frame, entity, filter);

            // Optionally suspend gravity during dash
            if (SuspendsGravity)
            {
                filter->kcc->_gravityModifier = 0;
                filter->kcc->KinematicVerticalSpeed = 0; // Must null out the existing gravity
            }

            // Play dash animation
            filter->animator->FadeTo(frame, AnimationId, FP._0, FP._0, FP._0, true, false);
        }

        public override unsafe void UpdateState(Frame frame, EntityRef entity, StateFilter* filter)
        {
            base.UpdateState(frame, entity, filter);

            //ProcessDash(frame, master, kcc);
        }

        public override unsafe void ExitState(Frame frame, EntityRef entity, StateFilter* filter)
        {
            base.ExitState(frame, entity, filter);

            if (SuspendsGravity)
            {
                filter->kcc->_gravityModifier = 1; // Reactiviate gravity
            }
        }

        //private void ProcessDash(Frame f, CharacterMaster* master, KCC2D* kcc)
        //{
        //    int direction = master->MovementData.FacingDirection;
        //    kcc->AddForce(f, new FPVector2(DashSpeed * direction, kcc->KinematicVerticalSpeed));
        //}
    }
}