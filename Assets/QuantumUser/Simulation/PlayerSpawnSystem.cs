namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Handles player spawning when a new player joins the game.
    /// </summary>
    public unsafe class PlayerSpawnSystem : SystemMainThreadFilter<PlayerSpawnSystem.Filter>, ISignalOnPlayerAdded
    {
        public struct Filter
        {
            public EntityRef Entity;
            public PlayerLink* PlayerLink;
        }

        public void OnPlayerAdded(Frame f, PlayerRef player, bool firstTime)
        {
            var playerData = f.GetPlayerData(player);
            Log.Debug($"OnPlayerAdded callback: {playerData.PlayerNickname}, {playerData.PlayerAvatar.Id}");
            var playerEntity = f.Create(playerData.PlayerAvatar);

            if (f.Unsafe.TryGetPointer<PlayerLink>(playerEntity, out var playerLink))
            {
                playerLink->Player = player;
            }
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            // This system doesn't need per-frame logic yet
            // The filter ensures PlayerLink components are accessible if needed later
        }
    }
}