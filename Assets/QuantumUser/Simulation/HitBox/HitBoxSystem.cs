using Photon.Deterministic;
using Quantum.Physics2D;
using System.Collections.Generic;

namespace Quantum
{
    /// <summary>
    /// Handles hitbox logic including drawing active hitboxes using Quantum's Draw API
    /// </summary>
    public unsafe class HitBoxSystem : SystemMainThreadFilter<HitBoxSystem.Filter>, ISignalOnHitboxSetActive/*, ISignalOnComponentAdded<HitBox>, ISignalOnComponentRemoved<HitBox>*/
    {
        public struct Filter
        {
            public EntityRef Entity;
            public HitBox* HitBox;
            public Transform2D* Transform;
            public CharacterMaster* Master;
            public MovementData* MovementData;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            // Skip if hitbox is not active
            if (!filter.HitBox->IsActive) return;

            // Get the hitbox config
            var config = frame.FindAsset<HitBoxConfig>(filter.HitBox->CurrentHitBox.Id);
            if (config == null) return;

            // Get facing direction (from MovementData)
            int facing = filter.MovementData->FacingDirection;

            // Draw all capsules in the config
            config.DrawHitboxes(frame, filter.Transform->Position, facing);

            // CHECK FOR HITS
            CheckForHits(frame, ref filter, config, facing);
        }

        private Shape2D CreateShapeFromInfo(CapsuleInfo info)
        {
            // Create shape based on type (circle or capsule)
            // This matches how you draw them in your config
            return Shape2D.CreateCapsule(info.Radius, info.Height);
        }

        private void CheckForHits(Frame frame, ref Filter filter, HitBoxConfig config, int facing)
        {
            FPVector2 worldPosition = filter.Transform->Position;

            for (int i = 0; i < config.Capsules.Length; i++)
            {
                var shapeInfo = config.Capsules[i];

                FPVector2 offset = new FPVector2(shapeInfo.Offset.X * facing, shapeInfo.Offset.Y);
                FPVector2 shapeWorldPos = worldPosition + offset;

                Shape2D shape = CreateShapeFromInfo(shapeInfo);

                // Use the same pattern as KCC's FindContacts
                var contacts = FindHitboxContacts(frame, shapeWorldPos, shape, config.HurtBoxMask, filter.Entity);

                foreach (var contact in contacts)
                {
                    if (frame.TryGet(contact.Entity, out HurtBox hurtbox) && hurtbox.IsActive)
                    {
                        if (frame.TryGet(contact.Entity, out Health health))
                        {
                            // CHECK: Has this entity already been hit by this hitbox instance?
                            if (HasEntityBeenHit(frame, filter.HitBox, contact.Entity))
                            {
                                continue;
                            }

                            // Mark as hit BEFORE applying damage (prevents double hits from same frame)
                            AddHitEntity(frame, filter.HitBox, contact.Entity);

                            var hitList = filter.HitBox->HitEntities;

                            // Apply damage
                            Log.Debug($"[HitBoxSystem] OnDamageDealt CALLBACK, APPLYING DAMAGE. Current HitList count {GetHitListCount(frame, filter.HitBox)}, Contacts List count {contacts.Count}. FRAME: {frame.Number}, predicted: {frame.IsPredicted}");

                            //if (hitList.Count > 0)
                            //{
                            //    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                            //    sb.Append($"[HitBoxSystem] Hit list now contains {hitList.Count} entities: ");
                            //    for (int j = 0; j < hitList.Count; j++)
                            //    {
                            //        sb.Append($"{hitList[j]}");
                            //        if (j < hitList.Count - 1) sb.Append(", ");
                            //    }
                            //    Log.Debug(sb.ToString());
                            //}
                            frame.Signals.OnDamageDealt(contact.Entity, config, filter.Entity);
                        }
                    }
                }
            }
        }

        private bool HasEntityBeenHit(Frame frame, HitBox* hitbox, EntityRef entity)
        {
            var hitList = hitbox->HitEntities;            

            for (int i = 0; i < hitList.Length; i++)
            {
                if (hitList[i].Equals(entity))
                {
                    return true;
                }
            }

            return false;
        }

        private void AddHitEntity(Frame frame, HitBox* hitbox, EntityRef entity)
        {
            for (int i = 0; i < hitbox->HitEntities.Length; i++)
            {
                if (hitbox->HitEntities[i].IsValid)
                    continue;

                hitbox->HitEntities[i] = entity;
            }
        }

        private void ClearHitList(Frame frame, HitBox* hitbox)
        {
            for (int i = 0; i < hitbox->HitEntities.Length; i++)
            {
                hitbox->HitEntities[i] = EntityRef.None; // Nullify the list
            }
        }

        private int GetHitListCount(Frame frame, HitBox* hitbox)
        {
            int count = 0;

            for (int i = 0; i < hitbox->HitEntities.Length; i++)
            {
                if (hitbox->HitEntities[i].IsValid)
                {
                    //Log.Debug($"[GetHitListCount] entity {hitbox->HitEntities[i]} is valid (u tell me, IsValid says it is)");
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Finds all valid contacts for a hitbox shape (similar to KCC's FindContacts)
        /// </summary>
        private List<Hit> FindHitboxContacts(Frame frame, FPVector2 position, Shape2D shape, LayerMask mask, EntityRef selfEntity)
        {
            var hits = new List<Hit>();

            // Use the same OverlapShape that KCC uses
            var overlaps = frame.Physics2D.OverlapShape(position, 0, shape, mask, QueryOptions.HitAll);

            for (int i = 0; i < overlaps.Count; i++)
            {
                var hit = overlaps[i];

                // Skip self
                if (hit.Entity == selfEntity) continue;

                // KCC also checks if entity has PhysicsBody (uncomment if needed)
                // if (!frame.Has<PhysicsBody>(hit.Entity)) continue;

                hits.Add(hit);
            }

            return hits;
        }

        public void OnHitboxSetActive(Frame f, EntityRef entity, QBoolean isActive, AssetRef<HitBoxConfig> config)
        {
            HitBox* hitbox = f.Unsafe.GetPointer<HitBox>(entity);

            hitbox->CurrentHitBox = config;

            hitbox->IsActive = isActive;

            // Clear the list
            if (!isActive)
                ClearHitList(f, hitbox);
        }

        //public void OnAdded(Frame frame, EntityRef entity, HitBox* component)
        //{
        //    // Allocate a new list when HitBox component is added to an entity
        //    component->HitEntities = frame.AllocateList<EntityRef>();
        //    Log.Debug($"[HitBoxSetup] Allocated hit list for entity {entity}");
        //}

        //public void OnRemoved(Frame frame, EntityRef entity, HitBox* component)
        //{
        //    // IMPORTANT: Deallocate the list to prevent memory leaks
        //    frame.FreeList(component->HitEntities);
        //    component->HitEntities = default;  // Nullify
        //    Log.Debug($"[HitBoxSetup] Freed hit list for entity {entity}");
        //}
    }
}