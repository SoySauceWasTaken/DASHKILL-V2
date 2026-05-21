using UnityEngine.Serialization;

namespace Quantum.Addons.Animator
{
  using Quantum;
  using System;
  using UnityEngine;
  using UE = UnityEngine;

  public unsafe class AnimatorMecanim : QuantumEntityViewComponent, IAnimatorEntityViewComponent
  {
    /// <summary>
    /// If true, a "frame rate" will be utilized.  Can be used to give animations a "stop motion" type feel.
    /// </summary>
    public bool UtilizeFrameRate;

    /// <summary>
    /// The frame rate at which animations will be played back
    /// </summary>
    public float FrameRate = 60;

    /// <summary>
    /// Controls if the animations are updated when the entity is culled.
    /// </summary>
    public bool AnimateWhenCulled = false;

    /// <summary>
    /// The Unity Animator Component reference.
    /// </summary>
    private UE.Animator _animator;

    /// <summary>
    /// The previous Animation state
    /// </summary>
    private int[] _previousAnimationState;

    /// <summary>
    /// The previous animation time
    /// </summary>
    private float[] _previousAnimationTime;

    /// <summary>
    /// The previous layerWeights;
    /// </summary>
    private float?[] _previousLayerWeights;

    /// <summary>
    /// Tracks whether the Animator GameObject was previously disabled, 
    /// used to determine if cached data needs to be reset. 
    /// </summary>
    private bool _pendingReset;

    /// <summary>
    /// Unity lifecycle method called when the script instance is being loaded.  
    /// Disable the Unity Animator component,
    /// and attaches an <see cref="AnimatorControllerObserver"/> for monitoring.
    /// </summary>
    void Awake()
    {
      _animator = GetComponentInChildren<UE.Animator>();

      // The Animator is disabled so it can be updated it manually.
      _animator.enabled = false;

      _animator.gameObject.AddComponent<AnimatorControllerObserver>().Setup(this);
    }

    /// <summary>
    /// Initializes the AnimatorComponent state tracking arrays based on the provided Animator graph.
    /// </summary>
    /// <param name="frame">The current Quantum frame.</param>
    /// <param name="animator">Pointer to the Quantum Animator component.</param>
    public void Init(Frame frame, AnimatorComponent* animator)
    {
      var asset = QuantumUnityDB.GetGlobalAsset<AnimatorGraph>(animator->AnimatorGraph.Id);
      if (asset)
      {
        _previousAnimationState = new int[asset.Layers.Length];
        _previousAnimationTime = new float[asset.Layers.Length];
        _previousLayerWeights = new float?[asset.Layers.Length];
      }
    }

    /// <summary>
    /// Called when the entity is activated.  
    /// Resets cached animation state and timing data.
    /// </summary>
    /// <param name="frame">The current Quantum frame.</param>
    public override void OnActivate(Frame frame)
    {
      ResetData();
    }

    /// <summary>
    /// Flags the Animator as requiring a reset, which will be processed  
    /// on the next update to restore animation state consistency.
    /// </summary>
    public void SetPendingReset()
    {
      _pendingReset = true;
    }

    /// <summary>
    /// Resets cached animation state and time data to ensure the Animator 
    /// can restart correctly after the GameObject is re-enabled.
    /// </summary>
    private void ResetData()
    {
      if (_previousAnimationState != null)
      {
        for (int i = 0; i < _previousAnimationState.Length; i++)
        {
          _previousAnimationState[i] = 0;
          _previousAnimationTime[i] = 0;
        }
      }

      if (_animator.gameObject.activeInHierarchy)
      {
        _pendingReset = false;
      }
    }

    /// <summary>
    /// Main update loop for driving Mecanim animations.  
    /// Resolves the current animation states and transitions from Quantum simulation,  
    /// updates Unity’s Animator component accordingly, and synchronizes parameters.
    /// </summary>
    /// <param name="frames">The Quantum frames container (Predicted and Verified).</param>
    /// <param name="animator">Pointer to the Quantum Animator component.</param>
    public void Animate(Frame frame, AnimatorComponent* animator)
    {
      if (frame.IsPredictionCulled(EntityRef) && AnimateWhenCulled == false) return;

      if (_pendingReset) ResetData();

      var asset = QuantumUnityDB.GetGlobalAsset<AnimatorGraph>(animator->AnimatorGraph.Id);
      if (asset)
      {
        var layers = frame.ResolveList(animator->Layers);
        for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
        {
          var layerData = layers.GetPointer(layerIndex);

          // Sets the layers weight if a change has been detected.
          if (_previousLayerWeights[layerIndex] != layerData->Weight.AsFloat)
          {
            _animator.SetLayerWeight(layerIndex, layerData->Weight.AsFloat);
            _previousLayerWeights[layerIndex] = layerData->Weight.AsFloat;
          }

          // If ToStateId is not 0, this means we are in the middle of a transition.
          if (layerData->ToStateId != 0)
          {
            // If the animator is not playing the to state id, this means this animation hasn't started yet.
            if (_previousAnimationState[layerIndex] != layerData->ToStateId)
            {
              // The new animation is cross faded to
              _animator.CrossFadeInFixedTime(layerData->ToStateId,
                (layerData->TransitionDuration - layerData->TransitionTime).AsFloat, layerIndex,
                layerData->ToStateTime.AsFloat);

              // The Animator is updated using a delta of 0 to make sure the correct animation transition is rendered
              _animator.Update(0);

              // The previous animation state and transition time are updated
              _previousAnimationState[layerIndex] = layerData->ToStateId;

              // We update the previous animation time by the transition time
              _previousAnimationTime[layerIndex] = layerData->ToStateTime.AsFloat;
            }
            else
            {
              // Make sure the animator gets updated once per layer
              if (layerIndex == asset.Layers.Length - 1)
              {
                UpdateAnimator(layerIndex, layerData->ToStateTime.AsFloat, layerData->ToLength.AsFloat,
                  (layerData->TransitionDuration - layerData->TransitionTime).AsFloat);
              }
            }
          }
          else if (layerData->CurrentStateId != _previousAnimationState[layerIndex])
          {
            _animator.PlayInFixedTime(layerData->CurrentStateId, layerIndex, layerData->Time.AsFloat);
            _animator.Update(0);

            _previousAnimationState[layerIndex] = layerData->CurrentStateId;
            _previousAnimationTime[layerIndex] = layerData->Time.AsFloat;
          }
          else
          {
            // Make sure the animator gets updated once per layer
            if (layerIndex == asset.Layers.Length - 1)
            {
              UpdateAnimator(layerIndex, layerData->Time.AsFloat, layerData->Length.AsFloat);
            }
          }
        }

        UpdateParameters(frame, asset, animator);
      }
    }


    /// <summary>
    /// Updates the parameters / variables for the Animators
    /// </summary>
    /// <param name="frame">The quantum frame</param>
    /// <param name="animatorGraph">The Animator graph asset</param>
    /// <param name="animatorComponent"></param>
    private void UpdateParameters(Frame frame, AnimatorGraph animatorGraph, AnimatorComponent* animatorComponent)
    {
      var variableList = frame.ResolveList(animatorComponent->AnimatorVariables);

      for (int i = 0; i < animatorGraph.Variables.Length; i++)
      {
        switch (animatorGraph.Variables[i].Type)
        {
          case AnimatorVariable.VariableType.Bool:
            _animator.SetBool(animatorGraph.Variables[i].Name, *variableList[i].BooleanValue);
            break;
          case AnimatorVariable.VariableType.Trigger:
            // Do nothing, this prevents the trigger from being called twice.
            break;
          case AnimatorVariable.VariableType.Int:
            _animator.SetInteger(animatorGraph.Variables[i].Name, *variableList[i].IntegerValue);
            break;
          case AnimatorVariable.VariableType.FP:
            _animator.SetFloat(animatorGraph.Variables[i].Name, (*variableList[i].FPValue).AsFloat);
            break;
        }
      }
    }

    /// <summary>
    /// Advances the Animator by calculating and applying the correct delta time 
    /// for the given animation layer. Supports both continuous time and frame-rate-based 
    /// playback, handles negative deltas caused by blend tree flickers, and ensures 
    /// transitions remain visually consistent.
    /// </summary>
    /// <param name="layerIndex">The index of the animation layer being updated.</param>
    /// <param name="time">The current normalized time of the animation state.</param>
    /// <param name="length">The total length (duration) of the animation state.</param>
    /// <param name="transitionDifference">
    /// Optional. The remaining transition duration, used to reapply the state 
    /// if a negative delta occurs during transitions.
    /// </param>
    void UpdateAnimator(int layerIndex, float time, float length, float? transitionDifference = null)
    {
      float delta = 0;
      if (!UtilizeFrameRate)
      {
        delta = time - _previousAnimationTime[layerIndex];
        _previousAnimationTime[layerIndex] = time;
      }
      else
      {
        float inFrame = Mathf.Round(time * FrameRate) / FrameRate;
        delta = inFrame - _previousAnimationTime[layerIndex];
        _previousAnimationTime[layerIndex] = inFrame;
      }

      // Preventing negative value due to flicker on BlendTree states
      if (delta < 0)
      {
        if (transitionDifference.HasValue)
        {
          _animator.CrossFadeInFixedTime(_previousAnimationState[layerIndex], transitionDifference.Value, layerIndex,
            time % length);
          delta = 0f;
        }
        else
        {
          delta = length - Math.Abs(delta);
        }
      }

      _animator.Update(delta);
    }
  }
}