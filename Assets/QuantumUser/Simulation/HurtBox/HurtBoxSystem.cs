using Photon.Deterministic;
using static UnityEngine.LowLevelPhysics2D.PhysicsShape;

namespace Quantum
{
    /// <summary>
    /// Handles hurtbox logic including drawing active hurtboxes using Quantum's Draw API
    /// </summary>
    public unsafe class HurtBoxSystem : SystemMainThreadFilter<HurtBoxSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public HurtBox* HurtBox;
            public Transform2D* Transform;
            public CharacterMaster* Master;
            public MovementData* MovementData;
            public PhysicsCollider2D* Collider;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            // Skip if hitbox is not active
            if (!filter.HurtBox->IsActive) return;

            // Get the hitbox config
            var config = frame.FindAsset<HurtBoxConfig>(filter.HurtBox->CurrentHurtBox.Id);
            if (config == null) return;

            // Get facing direction (from MovementData)
            int facing = filter.MovementData->FacingDirection;

            // Draw all capsules in the config
            config.DrawHurtBoxes(frame, filter.Transform->Position, facing);

            // Update the physics collider
            UpdateHurtboxShapes(frame, ref filter, config, facing);
        }

        /// <summary>
        /// Creates or updates a compound collider based on the hurtbox config
        /// </summary>
        private void UpdateHurtboxShapes(Frame frame, ref Filter filter, HurtBoxConfig config, int facing)
        {
            // Create a compound shape to hold both existing + hurtbox shapes
            var compoundShape = Shape2D.CreatePersistentCompound();

            // SECOND: Add all hurtbox shapes from config
            for (int i = 0; i < config.Capsules.Length; i++)
            {
                var shapeInfo = config.Capsules[i];

                // Apply facing to offset
                FPVector2 offset = new FPVector2(shapeInfo.Offset.X * facing, shapeInfo.Offset.Y);

                FP calculatedExtent = (shapeInfo.Height / 2) - shapeInfo.Radius;

                // Create the hurtbox shape
                Shape2D hurtShape = Shape2D.CreateCapsule(shapeInfo.Radius, calculatedExtent , offset);

                // Add hurtbox shape to compound with offset
                compoundShape.Compound.AddShape(frame, ref hurtShape);
            }

            // Update the collider with the combined compound shape
            filter.Collider->Shape = compoundShape;
        }
    }
}