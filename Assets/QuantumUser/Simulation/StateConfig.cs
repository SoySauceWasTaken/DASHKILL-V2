namespace Quantum
{
    /// <summary>
    /// Base class for all state configurations.
    /// Contains static data and behavior methods for states.
    /// </summary>
    public abstract unsafe class StateConfig : AssetObject
    {

        /// IMPORTANT!!! This Method's name means "Can transition to state X that's handled by the *SAME* StateMachine's DetermineDesiredState()"
        public virtual bool CanTransitionTo(Frame frame, CharacterMaster* master, StateType currentState)
        {
            // By default, always allow transition
            return true;
        }

        /// IMPORTANT!!!! This method is CanExit to ANY OTHER STATE (NO MATTER the priority or StateMachine.)
        public virtual bool CanExit(Frame frame, EntityRef entity, StateFilter* filter)
        {
            // By default, states never auto-exit
            return false;
        }

        public virtual void EnterState(Frame frame, EntityRef entity, StateFilter* filter)
        {
            // Store this config's reference in the master (TODO: Move this line to CharacterMasterSystem.cs)
            filter->master->CurrentStateConfig = this;
        }

        public virtual void UpdateState(Frame frame, EntityRef entity, StateFilter* filter)
        {

        }

        public virtual void ExitState(Frame frame, EntityRef entity, StateFilter* filter)
        {

        }
    }
}