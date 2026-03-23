namespace Quantum
{
    using Photon.Deterministic;

    public unsafe class SimpleAnimationSystem : SystemMainThreadFilter<SimpleAnimationSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public KCC2D* KCC;
            public GameplayState* GameplayState;  // Your custom state
            public AnimatorComponent* Animator;
            public PlayerLink* PlayerLink;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            // 1. Get input
            var input = *frame.GetPlayerInput(filter.PlayerLink->Player);

            // 2. Determine animation state based on KCC + GameplayState
            string animState = GetAnimationState(filter.KCC->State, filter.GameplayState->MoveState, filter.GameplayState->ActionState, input);

            // 3. Apply to Animator
            var animatorGraph = frame.FindAsset<AnimatorGraph>(filter.Animator->AnimatorGraph.Id);
            //filter.Animator->TryFadeToState(frame, animState, 0, 0, 0, false, true);
        }



        // THIS IS PRETTY BAD: We're doing some logic that should only be expressed in the Systems


        // Returns the Animation Name that corresponds to the State
        private string GetAnimationState(KCCState kccState, MovementState moveState, ActionState actionState, QuantumDemoInputPlatformer2D input)
        {
            // Combat actions take highest priority
            if (actionState != ActionState.NONE)
            {
                switch (actionState)
                {
                    case ActionState.ATTACKING: return "Attack";
                    case ActionState.DEFLECTING: return "Deflect";
                    case ActionState.HITSTUN: return "HitStun";
                }
            }

            // Movement based on KCC
            switch (kccState)
            {
                case KCCState.GROUNDED:
                    // Check if moving horizontally
                    bool isMoving = FPMath.Abs(input.Direction.X) > FP._0_01;

                    if (!isMoving) return "Idle";

                    // Choose between walk/run based on your GameplayState
                    return moveState == MovementState.RUNNING ? "Run" : "Walk";

                case KCCState.JUMPED:
                case KCCState.DOUBLE_JUMPED:
                    return "Jump";

                case KCCState.FREE_FALLING:
                    return "Fall";

                case KCCState.WALLED:
                    return "WallSlide";

                case KCCState.DASHING:
                    return "Dash";

                default:
                    return "Idle";
            }
        }
    }
}