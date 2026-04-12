using System.Diagnostics;

namespace Quantum.Addons.Animator
{
  using System;
  using UnityEngine;
  using Photon.Deterministic;

  /// <inheritdoc cref="AnimatorEventAsset"/>
  [Serializable]
  public abstract unsafe class AnimatorTimeWindowEventAsset : AnimatorEventAsset, IAnimatorEventAsset
  {
    /// <inheritdoc cref="IAnimatorEventAsset.OnBake"/>
    public new virtual AnimatorEvent OnBake(AnimationClip unityAnimationClip,
      AnimationEvent unityAnimationEvent)
    {
      //This allows multiple TimeWindow events of the same type per clip
      for (int i = 0; i < unityAnimationClip.events.Length; i++)
      {
        if (unityAnimationClip.events[i].objectReferenceParameter == unityAnimationEvent.objectReferenceParameter
            && unityAnimationClip.events[i].time == unityAnimationEvent.time)
        {
          if (i != 0 && i % 2 != 0)
          {
            return null;    
          }
        }
      }
      
      var quantumTimeWindowAnimatorEvent = new AnimatorTimeWindowEvent();
      quantumTimeWindowAnimatorEvent.AssetRef = Guid;
      quantumTimeWindowAnimatorEvent.Time = FP.FromFloat_UNSAFE(unityAnimationEvent.time);

      quantumTimeWindowAnimatorEvent.EndTime = -1;
      EndTime = quantumTimeWindowAnimatorEvent.EndTime;
      StartTime = quantumTimeWindowAnimatorEvent.Time;

      // Search if the Time-Windowed AnimEvent has a "twin" that marks the end of the time window.
      bool hasPair = false;
      foreach (var unityEvent in unityAnimationClip.events)
      {
        if (unityEvent.objectReferenceParameter.GetType() != GetType())
        {
          continue;
        }

        if (unityEvent.time == unityAnimationEvent.time)
        {
          continue;
        }

        hasPair = true;

        if (unityEvent.time < unityAnimationEvent.time)
        {
          continue;
        }

        quantumTimeWindowAnimatorEvent.EndTime = FP.FromFloat_UNSAFE(unityEvent.time);
        EndTime = quantumTimeWindowAnimatorEvent.EndTime;
        return quantumTimeWindowAnimatorEvent;
      }

      if (hasPair == false)
      {
        Debug.LogWarning(
          $"[QuantumAnimator] QuantumAnimatorTimeWindowEventAsset not setup correctly on clip: {unityAnimationClip.name}. ");
      }
      return null;
    }

    /// <summary>
    /// Called the first time the event Evaluate is valid.
    /// </summary>
    /// <param name="f">The Quantum Frame.</param>
    /// <param name="animator">The AnimatorComponent being executed.</param>
    public abstract void OnEnter(Frame f, AnimatorComponent* animatorComponent, LayerData* layerData);

    /// <summary>
    /// Called the last time the event Evaluate is valid.
    /// </summary>
    /// <param name="f">The Quantum Frame.</param>
    /// <param name="animator">The AnimatorComponent being executed.</param>
    public abstract void OnExit(Frame f, AnimatorComponent* animatorComponent, LayerData* layerData);

    [HideInInspector]
    public FP EndTime;
    [HideInInspector]
    public FP StartTime;
  }
}