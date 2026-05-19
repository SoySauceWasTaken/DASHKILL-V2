using Photon.Deterministic;

namespace Quantum
{
    public abstract unsafe class GameStateConfig : AssetObject
    {
        public string StateName;
        public GameStateType StateType;

        public virtual void EnterState(Frame frame, GameStateMachine* stateMachine)
        {
            stateMachine->StateTimer = FP._0;
            stateMachine->CurrentStateConfig = this;
            stateMachine->CurrentState = StateType;

            Log.Debug($"[GameState] Entered {StateName}");
        }

        public virtual void UpdateState(Frame frame, GameStateMachine* stateMachine)
        {
            stateMachine->StateTimer += frame.DeltaTime;
        }

        public virtual void ExitState(Frame frame, GameStateMachine* stateMachine)
        {
            Log.Debug($"[GameState] Exited {StateName}");
        }

        public virtual bool CanTransitionTo(Frame frame, GameStateMachine* stateMachine, GameStateType toState)
        {
            return true;
        }
    }
}