// MovementStateMachineSystem.cs
namespace Quantum
{
    using Photon.Deterministic;

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
            var input = *frame.GetPlayerInput(filter.PlayerLink->Player);

            // 2. Determine what state we WANT based on physics state + input
            StateType desiredState = DetermineDesiredState(frame, ref filter, input);

            // 3. Submit request to CharacterMaster
            filter.Master->MovementRequest.RequestedState = desiredState;
            filter.Master->MovementRequest.Priority = (int)StateMachinePriority.Movement;
            filter.Master->MovementRequest.Requester = filter.Entity;
        }

        private StateType DetermineDesiredState(Frame frame, ref Filter filter, QuantumDemoInputPlatformer2D input)
        {
            // Debug: Log input direction
            Log.Debug($"[MovementStateMachine] DetermineDesiredState - Input Direction.X: {input.Direction.X}");

            // On ground: RUN if moving, otherwise IDLE
            if (input.Direction.X != 0)
            {
                Log.Debug($"[MovementStateMachine] DetermineDesiredState - Returning RUN");
                return StateType.RUN;
            }
            else
            {
                Log.Debug($"[MovementStateMachine] DetermineDesiredState - Returning IDLE");
                return StateType.IDLE;
            }
        }
    }
}