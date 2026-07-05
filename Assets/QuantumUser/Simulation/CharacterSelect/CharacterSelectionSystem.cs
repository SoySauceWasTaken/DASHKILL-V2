// CharacterSelectSystem.cs
namespace Quantum
{
    using Photon.Deterministic;

    public unsafe class CharacterSelectSystem : SystemMainThreadFilter<CharacterSelectSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public PlayerLink* PlayerLink;
            public CharacterSelect* CharacterSelect;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            // Process commands for each player
            for (int playerIndex = 0; playerIndex < frame.MaxPlayerCount; playerIndex++)
            {

                if (frame.TryGetPlayerCommand<CommandSelectCharacter>(playerIndex, out var selectCmd))
                {
                    Log.Debug($"CharacterSelectSystem: Found CommandSelectCharacter for player {playerIndex}, CharacterIndex: {selectCmd.CharacterIndex}");

                    // Find this player's CharacterSelect entity
                    var filterForPlayer = frame.Filter<PlayerLink, CharacterSelect>();
                    while (filterForPlayer.NextUnsafe(out var entity, out var playerLink, out var charSelect))
                    {
                        if (playerLink->Player._index == playerIndex)
                        {
                            Log.Debug($"Executing CommandSelectCharacter for player {playerIndex}");
                            selectCmd.Execute(frame, entity);
                            break;
                        }
                    }
                }

                // Process PlayerReady command
                if (frame.TryGetPlayerCommand<CommandPlayerReady>(playerIndex, out var readyCmd))
                {
                    Log.Debug($"CharacterSelectSystem: Found CommandPlayerReady for player {playerIndex}");

                    var filterForPlayer = frame.Filter<PlayerLink, CharacterSelect>();
                    while (filterForPlayer.NextUnsafe(out var entity, out var playerLink, out var charSelect))
                    {
                        if (playerLink->Player._index == playerIndex)
                        {
                            Log.Debug($"Executing CommandPlayerReady for player {playerIndex}");
                            readyCmd.Execute(frame, entity);
                            break;
                        }
                    }
                }
            }

            // Check if all players are ready
            bool allReady = true;
            int readyCount = 0;
            int totalPlayers = 0;

            var readyFilter = frame.Filter<PlayerLink, CharacterSelect>();
            while (readyFilter.NextUnsafe(out var entity, out var playerLink, out var charSelect))
            {                
                totalPlayers++;
                Log.Debug("inside while loop:" + totalPlayers);
                if (charSelect->IsReady)
                {
                    readyCount++;
                }
                else
                {
                    allReady = false;
                }
            }

            // If all players ready, fire event to transition
            if (allReady && totalPlayers > 0 && totalPlayers == readyCount)
            {
                Log.Debug("We fired the event");
                frame.Events.AllPlayersReady();
            }
        }
    }
}