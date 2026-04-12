// StateMachineUtils.cs
namespace Quantum
{
    public static unsafe class StateMachineUtils
    {
        /// <summary>
        /// Gets the StateConfig asset reference for a given StateType from the CharacterMaster
        /// </summary>
        /// <param name="state">The StateType to get the config for</param>
        /// <param name="master">Pointer to the CharacterMaster component</param>
        /// <returns>AssetRef to the corresponding StateConfig</returns>
        public static AssetRef<StateConfig> GetConfigRefForState(StateType state, CharacterMaster* master)
        {
            switch (state)
            {
                case StateType.IDLE:
                    return master->IdleConfig;
                case StateType.RUN:
                    return master->RunConfig;
                case StateType.MID_AIR:
                    return master->MidAirConfig;
                case StateType.DASH:
                    return master->DashConfig;
                case StateType.JUMP:
                    return master->JumpConfig;
                case StateType.ATTACK:
                    return master->AttackConfig;

                default:
                    return master->IdleConfig;
            }
        }

        /// <summary>
        /// Gets the actual StateConfig asset for a given StateType
        /// </summary>
        public static StateConfig GetConfigForState(Frame frame, StateType state, CharacterMaster* master)
        {
            var configRef = GetConfigRefForState(state, master);
            return frame.FindAsset<StateConfig>(configRef.Id);
        }

        public static bool CanTransitionTo(Frame frame, StateType state, CharacterMaster* master)
        {
            return GetConfigForState(frame, StateType.RUN, master).CanTransitionTo(frame, master, master->CurrentState);
        }
    }
}