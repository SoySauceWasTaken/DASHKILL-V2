namespace Quantum.Addons.Animator
{
    using System.Collections.Generic;
    using UnityEngine;
    using System;

    public unsafe class AnimatorViewUpdater : QuantumSceneViewComponent
    {
        /// <summary>
        /// Stores a mapping between an <see cref="EntityRef"/> and its corresponding
        /// <see cref="IAnimatorEntityViewComponent"/>.
        /// </summary>
        private Dictionary<EntityRef, IAnimatorEntityViewComponent> _animatorEntityViewComponent =
            new Dictionary<EntityRef, IAnimatorEntityViewComponent>();

        /// <summary>
        /// Temporary list of entities that were removed during the frame update.
        /// Used to safely process removals after iteration over tracked entities.
        /// </summary>
        private List<EntityRef> _removedEntities = new List<EntityRef>();

        /// <summary>
        /// Subscription handle for the game initialization callback.
        /// Used to register the view component with the updater once the game starts.
        /// </summary>
        private IDisposable _gameInitSubscription;

        /// <summary>
        /// Called when the view component becomes active.
        /// Clears all cached animator mappings and removed entity tracking,
        /// and disposes any existing initialization subscription.
        /// </summary>
        public override void OnActivate(Frame frame)
        {
            _gameInitSubscription?.Dispose();
            _gameInitSubscription = null;
            _animatorEntityViewComponent.Clear();
            _removedEntities.Clear();
        }

        /// <summary>
        /// Called when the view component is deactivated.
        /// Clears cached state and subscribes to the <see cref="CallbackGameInit"/>
        /// event so the component can be re-added to the updater when the game
        /// is initialized again.
        /// </summary>
        public override void OnDeactivate()
        {
            _animatorEntityViewComponent.Clear();
            _removedEntities.Clear();

            _gameInitSubscription =
                QuantumCallback.SubscribeManual<CallbackGameInit>(this, c => { Updater.AddViewComponent(this); });
        }

        /// <summary>
        /// Updates all Animator entity view components every frame.  
        /// Cleans up references to destroyed entities, ensures each entity has a valid 
        /// <see cref="IAnimatorEntityViewComponent"/>, initializes missing components, 
        /// and drives animation playback for active entities.
        /// </summary>
        public override void OnUpdateView()
        {
            var frame = Game.Frames.Predicted;

            // Remove destroyed entities
            foreach (var kvp in _animatorEntityViewComponent)
            {
                if (frame.Exists(kvp.Key) == false)
                {
                    _removedEntities.Add(kvp.Key);
                }
            }

            for (int i = 0; i < _removedEntities.Count; i++)
            {
                _animatorEntityViewComponent.Remove(_removedEntities[i]);
            }

            // Animate
            foreach (var pair in frame.Unsafe.GetComponentBlockIterator<AnimatorComponent>())
            {
                var animator = pair.Component;
                var entity = pair.Entity;

                var entityView = Updater.GetView(entity);
                if (entityView == null)
                {
                    continue;
                }

                if (_animatorEntityViewComponent.TryGetValue(entity, out var ap) == false)
                {
                    Assert.Check(entityView.GetComponents<IAnimatorEntityViewComponent>().Length == 1,
                        $"[Quantum Animator] The entity {entity} contains multiple {nameof(IAnimatorEntityViewComponent)}. Please make sure there is only one.");
                    var animatorViewComponent = entityView.GetComponent<IAnimatorEntityViewComponent>();
                    if (animatorViewComponent != null)
                    {
                        _animatorEntityViewComponent.Add(entity, animatorViewComponent);
                        animatorViewComponent.Init(frame, animator);
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"[Quantum Animator] Trying to update animations of entity {entity} but it's EntityView does not have a {nameof(IAnimatorEntityViewComponent)}. Please add the component" +
                            $" {nameof(AnimatorMecanim)} or {nameof(AnimatorPlayables)}.");
                    }
                }

                if (ap != null)
                {
                    ap.Animate(frame, animator);
                }
            }
        }
    }
}