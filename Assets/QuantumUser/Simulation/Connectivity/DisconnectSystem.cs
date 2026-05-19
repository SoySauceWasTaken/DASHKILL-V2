namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;

    /// <summary>
    ///   The <c>DisconnectSystem</c> handles player disconnection and character entity destruction.
    /// </summary>
    [Preserve]
    public unsafe class DisconnectSystem : SystemMainThreadFilter<DisconnectSystem.Filter>
    {
        /// <summary>
        /// The <c>Filter</c> struct represents the components required for the system's operations,
        /// including an entity reference, PlayerLink and Status.
        /// </summary>
        public struct Filter
        {
            /// <summary>
            /// The reference to the entity being processed.
            /// </summary>
            public EntityRef Entity;
            /// <summary>
            /// Pointer to the entity's PlayerLink component.
            /// </summary>
            public PlayerLink* PlayerLink;
        }

        /// <summary>
        /// It is called for each entity that matches the required components.
        /// </summary>
        /// <param name="frame">The game frame.</param>
        /// <param name="filter">The filter containing the entity, the PlayerLink and Status.</param>
        public override void Update(Frame frame, ref Filter filter)
        {
            //Prevents from checking on predicted frames
            if (frame.IsPredicted)
            {
                return;
            }

            var flags = frame.GetPlayerInputFlags(filter.PlayerLink->Player);

            if ((flags & DeterministicInputFlags.PlayerNotPresent) == DeterministicInputFlags.PlayerNotPresent)
            {
                frame.Destroy(filter.Entity);
            }
        }
    }
}