namespace Quantum
{
    using Photon.Deterministic;

    public unsafe class PlayerSystem : SystemMainThreadFilter<PlayerSystem.Filter>, ISignalOnPlayerAdded
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Transform2D* Transform;
            public KCC2D* KCC;
            public PlayerLink* PlayerLink;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            //if (filter.Status->IsDead == true)
            //{
            //    return;
            //}

            var config = frame.FindAsset(filter.KCC->Config);

            config.Move(frame, filter.Entity, filter.Transform, filter.KCC);
        }

        public void OnPlayerAdded(Frame f, PlayerRef player, bool firstTime)
        {
            var playerData = f.GetPlayerData(player);
            var playerEntity = f.Create(playerData.PlayerAvatar);
            PlayerLink* playerLink = f.Unsafe.GetPointer<PlayerLink>(playerEntity);
            playerLink->Player = player;
        }
    }
}
