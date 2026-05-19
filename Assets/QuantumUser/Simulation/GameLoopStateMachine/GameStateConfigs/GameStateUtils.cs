namespace Quantum
{
    public static unsafe class GameStateUtils
    {
        /// <summary>
        /// Gets the GameStateConfig asset reference for a given GameStateType from a GameStateManager
        /// </summary>
        /// <param name="state">The GameStateType to get the config for</param>
        /// <param name="manager">Pointer to the GameStateMachine component</param>
        /// <returns>AssetRef to the corresponding GameStateConfig</returns>
        public static AssetRef<GameStateConfig> GetConfigRefForState(GameStateType state, GameStateMachine* manager)
        {
            switch (state)
            {
                case GameStateType.LOBBY:
                    return manager->LobbyConfig;
                case GameStateType.LOADING:
                    return manager->LoadingConfig;
                case GameStateType.ROUND_ACTIVE:
                    return manager->RoundActiveConfig;
                case GameStateType.ROUND_END:
                    return manager->RoundEndConfig;
                case GameStateType.MATCH_END:
                    return manager->MatchEndConfig;
                case GameStateType.DEATHBLOW:
                    return manager->CutsceneConfig;
                default:
                    return manager->LobbyConfig;
            }
        }

        /// <summary>
        /// Gets the actual GameStateConfig asset for a given GameStateType
        /// </summary>
        public static GameStateConfig GetConfigForState(Frame frame, GameStateType state, GameStateMachine* manager)
        {
            var configRef = GetConfigRefForState(state, manager);
            return frame.FindAsset<GameStateConfig>(configRef.Id);
        }
    }
}