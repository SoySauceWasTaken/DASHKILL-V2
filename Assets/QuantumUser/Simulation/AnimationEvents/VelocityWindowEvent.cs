using Photon.Deterministic;
using Quantum;
using Quantum.Addons.Animator;
using UnityEngine;

/// <summary>
/// Quantum Animator Time-Window Event that smoothly interpolates character velocity
/// from StartVelocity to EndVelocity over the duration of the event window.
/// </summary>
[CreateAssetMenu(menuName = "Quantum/Animator/VelocityWindowEvent", fileName = "NewVelocityWindowEvent")]
public class VelocityWindowEvent : AnimatorTimeWindowEventAsset
{
    [Header("Mode")]
    public VelocityInterpolationMode Mode = VelocityInterpolationMode.Fixed;

    [Header("Velocity Settings")]
    public FPVector2 StartVelocity;
    public FPVector2 EndVelocity;
    public bool UseEndVelocity;
    public bool ZeroVelocityOnEnd;

    //[Header("Lerp Settings (Mode = Lerp only)")]
    


    private unsafe void ProcessVelocity(Frame frame, AnimatorComponent* animatorComponent, LayerData* layerData)
    {
        switch (Mode)
        {
            case VelocityInterpolationMode.Fixed:
                frame.Signals.OnAnimatorSetVelocity(animatorComponent->Self, StartVelocity);
                break;

            case VelocityInterpolationMode.Lerp:
                frame.Signals.OnAnimatorSetVelocity(animatorComponent->Self, StartVelocity); // First we need to set the StartVelocity
                frame.Signals.OnAnimatorVelocityLerp(animatorComponent->Self, StartVelocity, EndVelocity, StartTime, EndTime, layerData->Time);
                break;
        }
    }

    /// <summary>
    /// Called when the animation enters the time window
    /// </summary>
    public override unsafe void OnEnter(Frame frame, AnimatorComponent* animatorComponent, LayerData* layerData)
    {
        //Log.Debug($"[VelocityWindowEvent] StartTime: {layerData->Time}, EndTime: {EndTime}");
        ProcessVelocity(frame, animatorComponent, layerData);
    }

    /// <summary>
    /// Called every frame during the time window
    /// </summary>
    public override unsafe void Execute(Frame frame, AnimatorComponent* animatorComponent, LayerData* layerData)
    {
        ProcessVelocity(frame, animatorComponent, layerData);
    }

    /// <summary>
    /// Called when the animation exits the time window
    /// </summary>
    public override unsafe void OnExit(Frame frame, AnimatorComponent* animatorComponent, LayerData* layerData)
    {
        // Ensure we reach exact EndVelocity at window exit
        if (UseEndVelocity)
            frame.Signals.OnAnimatorSetVelocity(animatorComponent->Self, EndVelocity);
        if (ZeroVelocityOnEnd)
            frame.Signals.OnAnimatorSetVelocity(animatorComponent->Self, FPVector2.Zero);
    }

    public enum VelocityInterpolationMode
    {
        Fixed,      // Constant velocity (uses StartVelocity)
        Lerp        // Smooth interpolation from StartVelocity to EndVelocity over time
    }
}