namespace Quantum.Addons.Animator
{
  using UnityEngine;

  [DisallowMultipleComponent]
  [AddComponentMenu("")] // Not visible in "Add Component"
  public class AnimatorControllerObserver : MonoBehaviour
  {
    private AnimatorMecanim _animatorMecanim;

    /// <summary>
    /// Associates this observer with the given <see cref="AnimatorMecanim"/> instance.
    /// </summary>
    /// <param name="animatorMecanim">The AnimatorMecanim component to notify on disable.</param>
    public void Setup(AnimatorMecanim animatorMecanim)
    {
      _animatorMecanim = animatorMecanim;
    }

    /// <summary>
    /// Unity callback invoked when the component is disabled.  
    /// Notifies the linked <see cref="AnimatorMecanim"/> to reset its cached data.
    /// </summary>
    private void OnDisable()
    {
      _animatorMecanim.SetPendingReset();
    }

    /// <summary>
    /// Unity callback invoked when the component is reset in the Inspector.  
    /// Ensures this component cannot be manually added by immediately destroying itself.
    /// </summary>
    private void Reset()
    {
      DestroyImmediate(this); // auto-remove if manually added
    }
  }
}