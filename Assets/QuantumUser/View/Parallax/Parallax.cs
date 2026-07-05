using DashKill;
using Quantum;
using UnityEngine;

public class Parallax : QuantumEntityViewComponent<GameViewContext>
{
    public Camera cam;

    public Transform subject; // to be followed

    Vector2 startPosition;
    float startZ;

    Vector2 travel => (Vector2)cam.transform.position - startPosition;
    Vector2 parallaxFactor;

    public override void OnActivate(Frame frame)
    {
        startPosition = transform.position;
        startZ = transform.position.z;
    }

    public override void OnUpdateView()
    {
        transform.position = startPosition + travel;
    }
}