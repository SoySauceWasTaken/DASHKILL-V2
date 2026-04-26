namespace Quantum
{
    using Photon.Deterministic;

    public unsafe class DamageSystem : SystemMainThread, ISignalOnDamageDealt, ISignalOnEntityDied
    {
        public void OnDamageDealt(Frame frame, EntityRef target, FP amount, EntityRef source)
        {
            if (!frame.TryGet(target, out Health health)) return;

            health.Current -= amount;

            if (health.Current <= FP._0)
            {
                health.Current = FP._0;
                frame.Signals.OnEntityDied(target, source);
            }

            // Still useful for view feedback
            //frame.Events.OnDamageTaken(target, amount, health.Current);
        }

        public void OnEntityDied(Frame frame, EntityRef entity, EntityRef killer)
        {
            // Handle death: disable KCC, trigger death state, etc.
            Log.Debug($"Entity {entity} died!");
        }

        public override void Update(Frame f)
        {
        }
    }
}