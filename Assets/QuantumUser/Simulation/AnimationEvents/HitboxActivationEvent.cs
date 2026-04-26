using Quantum;
using Quantum.Addons.Animator;
using UnityEngine;


[CreateAssetMenu(menuName = "Quantum/Animator/HitboxActivationEvent", fileName = "NewHitboxActivationEvent")]
public class HitboxActivationEvent : AnimatorInstantEventAsset
{
    [Header("Hitbox Config")]
    public AssetRef<HitBoxConfig> HitBoxConfig;

    //[Header("Activation")]
    //public bool Activate = true;  // True = activate, False = deactivate

    public override unsafe void Execute(Frame frame, AnimatorComponent* animatorComponent, LayerData* layerData)
    {
        Log.Debug("[HitboxActivationEvent] OnEnter Callback");
        frame.Signals.OnHitboxSetActive(animatorComponent->Self, true, HitBoxConfig);
    }
}