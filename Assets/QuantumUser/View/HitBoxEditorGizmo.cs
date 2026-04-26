using Photon.Deterministic;
using Quantum;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

[ExecuteAlways]
public class HitBoxEditorGizmo : MonoBehaviour
{
    [Header("HitBox Configuration")]
    public HitBoxConfig hitBoxConfig;

    [Header("HurtBox Configuration")]
    public HurtBoxConfig hurtBoxConfig;

    [Header("Runtime Settings")]
    public int FacingDirection = 1;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (UnityEditor.EditorApplication.isPlaying) return;

        if (hitBoxConfig != null)
            hitBoxConfig.DrawGizmo(transform.position, FacingDirection);

        if (hurtBoxConfig != null)
            hurtBoxConfig.DrawGizmo(transform.position, FacingDirection);

    }
#endif
}