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
    [SerializeField]
    public Vector2 OnHoverOffset;

    [Header("Positions")]
    [SerializeField]
    private Vector2 ceilingRelative;
    [SerializeField]
    public Vector2 originRelative;
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

    public Vector2 Origin { get => originRelative * SC_GameData.Instance.screenSize; set => originRelative = value / new Vector2(6.667f, 5); }
    public Vector2 Ceiling { get => ceilingRelative * SC_GameData.Instance.screenSize; set => ceilingRelative = value / new Vector2(6.667f, 5); }
}