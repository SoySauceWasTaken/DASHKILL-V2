namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine;
    using UnityEngine.Windows;

    public unsafe class MidAirStateConfig : StateConfig
    {
        [Space(5)]
        [Header("Animation")]
        public int AnimationId = 0;

        [Space(5)]
        [Header("Horizontal Movement")]
        public FP Acceleration = 10;
        public FP MaxBaseSpeed = 4;
        public FP FlipDirectionMultiplier = 1;

        [Space(5)]
        [Header("Falling")]
        public float MaxFallSpeed = 1;

        public override bool CanTransitionTo(Frame frame, StateComponent* currentState)
        {
            return true;
        }

        public override unsafe void EnterState(Frame frame, CharacterMaster* master, KCC2D* kcc, AnimatorComponent* animator)
        {
            base.EnterState(frame, master, kcc, animator);

            // Play run animation
            //animator->FadeTo(frame, AnimationId, FP._0, FP._0, FP._0, true, false);
        }

        public override unsafe void UpdateState(Frame frame, CharacterMaster* master, KCC2D* kcc, AnimatorComponent* animator)
        {
            base.UpdateState(frame, master, kcc, animator);

            var kccConfig = frame.FindAsset(kcc->Config);

            IntegrateForces(frame, master, kcc, kccConfig);

            QuantumDemoInputPlatformer2D input = master->Input;

            if (input.Direction.X > 0)
                master->MovementData.FacingDirection = 1;
            else if (input.Direction.X < 0)
                master->MovementData.FacingDirection = -1;
        }

        private void IntegrateForces(Frame f, CharacterMaster* master, KCC2D* KCC, KCC2DConfig kccConfig)
        {
            // TODO: HANDLE EXTERNAL VELOCITY + ACCELARATION

            var sideMovement = SideMovement(KCC, master);

            Accelerate(f, KCC, sideMovement);

            ClampVelocity(f, KCC);
        }

        private void Accelerate(Frame f, KCC2D* KCC, FP sideMovement)
        {
            KCC->ApplyKinematicAcceleration(f, new FPVector2(Acceleration * sideMovement, 0) );
        }

        private void ClampVelocity(Frame f, KCC2D* kcc)
        {
            if (FPMath.Abs(kcc->_kinematicVelocity.X) > MaxBaseSpeed)
            {
                kcc->_kinematicVelocity.X = FPMath.Sign(kcc->_kinematicVelocity.X) * MaxBaseSpeed;
            }
        }

        private FP SideMovement(KCC2D* KCC, CharacterMaster* master)
        {
            FP sideMovement = master->Input.Direction.X;  // Direct from Direction vector


            var oppositeDirection = KCC->CombinedVelocity.X * sideMovement < 0; // Checks if the player is trying to turn

            if (oppositeDirection)
            {
                sideMovement *= FlipDirectionMultiplier;
            }

            return sideMovement;
        }        
    }
}