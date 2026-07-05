// CommandPlayerReady.cs
namespace Quantum
{
    using Photon.Deterministic;

    public class CommandPlayerReady : DeterministicCommand
    {
        public bool IsReady = true;

        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref IsReady);
        }

        public void Execute(Frame frame, EntityRef entity)
        {
            if (frame.TryGet<CharacterSelect>(entity, out var characterSelect))
            {
                characterSelect.IsReady = IsReady;
                frame.Set(entity, characterSelect);

                if (frame.TryGet<PlayerLink>(entity, out var playerLink))
                {
                    frame.Events.PlayerReady(playerLink.Player, IsReady);
                }
            }
        }
    }
}