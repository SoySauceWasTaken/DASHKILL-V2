// CommandSelectCharacter.cs
namespace Quantum
{
    using Photon.Deterministic;

    public class CommandSelectCharacter : DeterministicCommand
    {
        public int CharacterIndex;

        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref CharacterIndex);
        }

        public void Execute(Frame frame, EntityRef entity)
        {
            // Find the CharacterSelect component on this entity
            if (frame.TryGet<CharacterSelect>(entity, out var characterSelect))
            {
                characterSelect.SelectedCharacter = CharacterIndex;
                frame.Set(entity, characterSelect);

                Log.Debug($"CharacterSelectSystem: Processing command for player {entity.Index} with character {CharacterIndex}");

                // Fire event for UI to update podiums
                if (frame.TryGet<PlayerLink>(entity, out var playerLink))
                {
                    frame.Events.CharacterSelected(playerLink.Player, CharacterIndex);
                }
            }
        }
    }
}