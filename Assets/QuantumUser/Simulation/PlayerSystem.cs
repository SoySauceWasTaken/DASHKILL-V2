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
            QuantumDemoInputPlatformer2D input = *frame.GetPlayerInput(filter.PlayerLink->Player);

            var config = frame.FindAsset(filter.KCC->Config);
            if (frame.TryGet<PlayerLink>(filter.Entity, out var link))
            {
                //input.Direction
                // Instread of passing input to KCC, we pass it to the Systems
            }

            // DEBUG 4: Verify movement is being processed
            var beforePos = filter.Transform->Position;

            // This calls the KCC's Move method which should use the input direction
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
