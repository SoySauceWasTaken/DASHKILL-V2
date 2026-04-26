namespace Quantum
{
    /// <summary>
    /// Handles hitbox logic including drawing active hitboxes using Quantum's Draw API
    /// </summary>
    public unsafe class HitBoxSystem : SystemMainThreadFilter<HitBoxSystem.Filter>, ISignalOnHitboxSetActive
    {
        public struct Filter
        {
            public EntityRef Entity;
            public HitBox* HitBox;
            public Transform2D* Transform;
            public CharacterMaster* Master;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            // Skip if hitbox is not active
            if (!filter.HitBox->IsActive) return;

            // Get the hitbox config
            var config = frame.FindAsset<HitBoxConfig>(filter.HitBox->CurrentHitBox.Id);
            if (config == null) return;

            // Get facing direction (from MovementData)
            int facing = filter.Master->MovementData.FacingDirection;

            // Draw all capsules in the config
            config.DrawHitboxes(frame, filter.Transform->Position, facing);
        }

        public void OnHitboxSetActive(Frame f, EntityRef entity, QBoolean isActive, AssetRef<HitBoxConfig> config)
        {
            HitBox* hitbox = f.Unsafe.GetPointer<HitBox>(entity);

            hitbox->CurrentHitBox = config;
            hitbox->IsActive = !hitbox->IsActive; // auto toggle on and off
        }
    }
}