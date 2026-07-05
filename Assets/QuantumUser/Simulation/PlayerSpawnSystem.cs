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


        // This refers to the HUMAN/Connection player. NOT the in-game character
        public void OnPlayerAdded(Frame f, PlayerRef player, bool firstTime)
        {
            var data = f.GetPlayerData(player);
            var prototype = f.FindAsset(data.SelectionPrototype);
            var entity = f.Create(prototype);

            // Override the PlayerLink with the correct player
            if (f.TryGet<PlayerLink>(entity, out var playerLink))
            {
                playerLink.Player = player;
                f.Set(entity, playerLink);
            }


            // DO NOT SPAWN A "game character" here.


            //var playerData = f.GetPlayerData(player);
            //Log.Debug($"OnPlayerAdded callback: {playerData.PlayerNickname}, {playerData.PlayerAvatar.Id}");
            //var playerEntity = f.Create(playerData.PlayerAvatar);

            //if (f.Unsafe.TryGetPointer<PlayerLink>(playerEntity, out var playerLink))
            //{
            //    playerLink->Player = player;
            //}
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            // This system doesn't need per-frame logic yet
            // The filter ensures PlayerLink components are accessible if needed later
        }
    }
}