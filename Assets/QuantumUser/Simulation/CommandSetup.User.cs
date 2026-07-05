namespace Quantum
{
    using System.Collections.Generic;
    using Photon.Deterministic;

    public static partial class DeterministicCommandSetup
    {
        static partial void AddCommandFactoriesUser(ICollection<IDeterministicCommandFactory> factories, RuntimeConfig gameConfig, SimulationConfig simulationConfig)
        {
            // Every command that could run in the sim must be added here

            factories.Add(new CommandSelectCharacter());
            factories.Add(new CommandPlayerReady());
            
        }
    }
}