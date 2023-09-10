using System;
using UnityEngine;

[Serializable]
public class ContainerSettings
{

    [SerializeField]
    public Containers Container;
    [SerializeField]
    [Tooltip("Only normalized values!")]
    public Vector2 stackDirection;
    [SerializeField]
    public bool KeepCardsFaceUp;
    [SerializeField]
    public bool FixPerspective;

    [Header("Positions")]
    [SerializeField]
    public Vector2 ceiling;
    [SerializeField]
    public Vector2 floor;
    [SerializeField]
    [Range(-1f, 1f)]
    public float offsetDistance;

    [Header("Rotations")]
    [SerializeField]
    public Vector3 originRotation;
    [SerializeField]
    public Vector3 offsetRotation;

    [Header("Sorting Order")]
    [SerializeField]
    public int originSortOrder;
    [SerializeField]
    public int offsetSortOrder;
}
/*
public void RotateIntoDeck(SC_Card node)
{
    node.TargetRotation = 1 / (1 + MathF.Abs(node.Position.x - tempOrigin.x) * nodeFlipRange) * containerSettings.originRotation;
}
*/