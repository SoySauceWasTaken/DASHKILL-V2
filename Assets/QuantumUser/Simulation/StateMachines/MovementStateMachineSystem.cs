// MovementStateMachineSystem.cs
namespace Quantum
{
    public unsafe class MovementStateMachineSystem : SystemMainThreadFilter<MovementStateMachineSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public MovementStateMachine* MovementSM;
            public CharacterMaster* Master;
            public KCC2D* KCC;
            public PlayerLink* PlayerLink;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            // 1. Get input
            var input = filter.Master->Input;

            // 2. Determine what state we WANT based on physics state + input
            StateType desiredState = DetermineDesiredState(frame, ref filter, input);

            // 3. Submit request to CharacterMaster
            filter.Master->MovementRequest.RequestedState = desiredState;
            filter.Master->MovementRequest.Priority = (int)StateMachinePriority.Movement;
            filter.Master->MovementRequest.Requester = filter.Entity;
        }

        private StateType DetermineDesiredState(Frame frame, ref Filter filter, QuantumDemoInputPlatformer2D input)
        {
            // JUMP takes priority over ground movement
            //Log.Debug($"AddForce requested - WasPressed: {input.AddForce.WasPressed}, IsGrounded: {filter.KCC->State == KCCState.GROUNDED}");
            if (input.Jump.WasPressed && filter.KCC->State == KCCState.GROUNDED)
            {
                //Log.Debug("[MovementSM] GOING TO JUMP");
                return StateType.JUMP;
            }

            // In air? Stay in MID_AIR
            if (filter.KCC->State != KCCState.GROUNDED)
            {
                return StateType.MID_AIR;
            }

            // On ground: RUN if moving, otherwise IDLE
            if (input.Direction.X != 0)
            {
                return StateType.RUN;
            }
            else
            {
                return StateType.IDLE;
            }
        }
    }
}