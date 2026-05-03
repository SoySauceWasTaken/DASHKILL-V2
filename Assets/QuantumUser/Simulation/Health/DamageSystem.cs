namespace Quantum
{
    using Photon.Deterministic;

    public unsafe class DamageSystem : SystemMainThread, ISignalOnDamageDealt, ISignalOnEntityDied
    {
        public void OnDamageDealt(Frame f, EntityRef target, AssetRef<HitBoxConfig> HitBoxConfig, EntityRef source)
        {
            if (!f.Unsafe.TryGetPointer(target, out Health* health)) return;

            HitBoxConfig config = f.FindAsset<HitBoxConfig>(HitBoxConfig.Id);

            Log.Debug($"[Damage] Entity {target} hit by {source} for {config.Damage} damage! Health: {health->Current} -> {health->Current - config.Damage}");

            health->Current -= config.Damage;

            if (health->Current <= FP._0)
            {
                health->Current = FP._0;
                f.Signals.OnEntityDied(target, source);
            }
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