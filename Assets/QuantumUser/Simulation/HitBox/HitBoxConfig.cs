using UnityEngine;
using Quantum;
using Quantum.Physics2D;
using Photon.Deterministic;

[CreateAssetMenu(menuName = "Quantum/HitBoxConfig", fileName = "HitBoxConfig")]
public class HitBoxConfig : AssetObject
{
    [Header("Capsules")]
    public CapsuleInfo[] Capsules;

    [Header("Debug (Runtime Only)")]
    public ColorRGBA ColorFinal = ColorRGBA.Red;

#if QUANTUM_UNITY
    [Header("Gizmo Settings (Editor Only)")]
    public Color GizmoColor = Color.red;
    public bool DrawGizmos = true;
#endif

    /// <summary>
    /// Draws all capsules in this config using Quantum's Draw API (Simulation-side)
    /// </summary>
    public void DrawHitboxes(Frame frame, FPVector2 worldPosition, int facing)
    {
        foreach (var capsuleInfo in Capsules)
        {
            // Calculate world position with facing direction
            FPVector2 offset = new FPVector2(capsuleInfo.Offset.X * facing, capsuleInfo.Offset.Y);
            FPVector2 center = worldPosition + offset;

            // Create capsule shape using same method as KCC2D
            var capsuleShape = Shape2D.CreateCapsule(capsuleInfo.Radius, capsuleInfo.Height / 2 - capsuleInfo.Radius);

            // Draw using Quantum's Draw API (same as KCC2D's debug drawing)
            Draw.Capsule(center, capsuleShape.Capsule, color: ColorFinal, wire: false);
        }
    }

#if QUANTUM_UNITY
    /// <summary>
    /// Unity Editor Gizmo drawing (Unity-side, for scene view visualization)
    /// </summary>
    public void DrawGizmo(Vector3 worldPosition, int facing)
    {
        if (!DrawGizmos) return;

        Gizmos.color = GizmoColor;

        foreach (var capsuleInfo in Capsules)
        {
            // Calculate world position with facing direction
            Vector3 offset = new Vector3((float)capsuleInfo.Offset.X * facing, (float)capsuleInfo.Offset.Y, 0);
            Vector3 center = worldPosition + offset;

            float radius = (float)capsuleInfo.Radius;
            float height = (float)capsuleInfo.Height;

            DrawCapsuleGizmo(center, radius, height);
        }
    }

    private void DrawCapsuleGizmo(Vector3 center, float radius, float height)
    {
        // The distance between sphere centers is height - (radius * 2)
        float sphereCenterDistance = Mathf.Max(0, height - (radius * 2f));

        Vector3 topSphere = center + Vector3.up * (sphereCenterDistance / 2f);
        Vector3 bottomSphere = center - Vector3.up * (sphereCenterDistance / 2f);

        // Draw wire spheres at ends
        Gizmos.DrawWireSphere(topSphere, radius);
        Gizmos.DrawWireSphere(bottomSphere, radius);

        // Draw connecting lines only if there's a gap
        if (sphereCenterDistance > 0)
        {
            // Draw lines at the equator of each sphere connecting to the cylinder
            Gizmos.DrawLine(topSphere + Vector3.left * radius, bottomSphere + Vector3.left * radius);
            Gizmos.DrawLine(topSphere + Vector3.right * radius, bottomSphere + Vector3.right * radius);
            Gizmos.DrawLine(topSphere + Vector3.forward * radius, bottomSphere + Vector3.forward * radius);
            Gizmos.DrawLine(topSphere + Vector3.back * radius, bottomSphere + Vector3.back * radius);

            // Also draw the cylinder wireframe
            Vector3 cylinderCenter = center;
            Vector3 cylinderSize = new Vector3(radius * 2f, sphereCenterDistance, radius * 2f);
            Gizmos.DrawWireCube(cylinderCenter, cylinderSize);
        }
    }
#endif
}

[System.Serializable]
public struct CapsuleInfo
{
    public FPVector2 Offset;
    public FP Radius;
    public FP Height;
}