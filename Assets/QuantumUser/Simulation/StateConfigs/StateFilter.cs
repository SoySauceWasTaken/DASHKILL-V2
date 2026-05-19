using Photon.Deterministic;

namespace Quantum
{
    public unsafe struct StateFilter
    {
        public EntityRef entity;
        public CharacterMaster* master;
        public KCC2D* kcc;
        public StatusStateMachine* status;
        public MovementData* movementData;
        public AnimatorComponent* animator;
        public Transform2D* transform;
        public ActionStateMachine* actionSM;
        public Health* health;
        // Add more as needed
    }

    public static unsafe class StateFilterUtils
    {
        /// <summary>
        /// Creates a StateFilter for the given entity with all component references.
        /// Caller is responsible for disposing/freeing if needed (though structs on stack don't need freeing).
        /// </summary>
        public static StateFilter CreateStateFilter(Frame frame, EntityRef entity)
        {
            StateFilter filter = default;

            filter.entity = entity;

            // Get required components (these should exist on the entity)
            filter.master = frame.Unsafe.GetPointer<CharacterMaster>(entity);
            filter.kcc = frame.Unsafe.GetPointer<KCC2D>(entity);
            filter.status = frame.Unsafe.GetPointer<StatusStateMachine>(entity);
            filter.movementData = frame.Unsafe.GetPointer<MovementData>(entity);
            filter.animator = frame.Unsafe.GetPointer<AnimatorComponent>(entity);
            filter.transform = frame.Unsafe.GetPointer<Transform2D>(entity);
            filter.actionSM = frame.Unsafe.GetPointer<ActionStateMachine>(entity);
            filter.health = frame.Unsafe.GetPointer<Health>(entity);

            return filter;
        }
    }
}