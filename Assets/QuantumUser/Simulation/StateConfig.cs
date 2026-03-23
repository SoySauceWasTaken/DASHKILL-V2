namespace Quantum
{
    /// <summary>
    /// Base class for all state configurations.
    /// Contains static data and behavior methods for states.
    /// </summary>
    public abstract unsafe class StateConfig : AssetObject
    {
        // Every state has a name (for debugging/ID)
        public string StateName;

        public virtual bool CanTransitionTo(Frame frame, StateComponent* currentState)
        {
            // By default, always allow transition
            return true;
        }

        public virtual void EnterState(Frame frame, CharacterMaster* master, KCC2D* kcc, AnimatorComponent* animator)
        {
            // Store this config's reference in the master
            master->CurrentStateConfig = this;
        }

        public virtual void UpdateState(Frame frame, CharacterMaster* master, KCC2D* kcc, AnimatorComponent* animator)
        {

        }

        public virtual void ExitState(Frame frame, CharacterMaster* master, KCC2D* kcc, AnimatorComponent* animator)
        {

        }
    }
}