namespace Quantum
{
    using Photon.Deterministic;

    public unsafe class GameStateMachineSystem : SystemMainThreadFilter<GameStateMachineSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public GameStateMachine* StateMachine;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            // Process state transition requests
            if (filter.StateMachine->IsTransitioning)
            {
                filter.StateMachine->IsTransitioning = false;
                return; // Transition will happen next frame
            }

            // Get current state config
            var currentConfig = frame.FindAsset<GameStateConfig>(filter.StateMachine->CurrentStateConfig.Id);
            if (currentConfig == null) return;

            // Update current state
            currentConfig.UpdateState(frame, filter.StateMachine);
        }

        public void RequestStateChange(Frame frame, GameStateMachine* stateMachine, GameStateType newState)
        {
            if (stateMachine->IsTransitioning) return;

            // Exit current
            var currentConfig = frame.FindAsset<GameStateConfig>(stateMachine->CurrentStateConfig.Id);
            currentConfig?.ExitState(frame, stateMachine);

            // Get new config
            var newConfigRef = GameStateUtils.GetConfigRefForState(newState, stateMachine);
            var newConfig = frame.FindAsset<GameStateConfig>(newConfigRef.Id);

            // Enter new
            newConfig?.EnterState(frame, stateMachine);
        }
    }
}