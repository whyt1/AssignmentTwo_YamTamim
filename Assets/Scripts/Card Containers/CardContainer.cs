using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Linked list of cards, using <see cref="SC_Card"/>
/// </summary>
[Serializable]
public abstract class CardContainer : MonoBehaviour
{

    #region Fields
    
    [SerializeField]
    private static int maxCapacitiy = 57;

    [SerializeField]
    protected bool ForceIntoPosition;
    [SerializeField]
    protected int count;

    [Header("Nodes")]
    [SerializeField]
    protected SC_Card head;
    [SerializeField]
    protected SC_Card tail;

    [Header("Container")]
    [SerializeField]
    protected Containers Container;
    [SerializeField]
    [Tooltip("Only normalized values!")]
    protected Vector2 stackDirection;
    [SerializeField]
    protected bool KeepCardsFaceUp;
    [SerializeField]
    protected bool FixPerspective;

    [Header("Positions")]
    [SerializeField]
    protected Vector2 originPosition;
    [SerializeField]
    [Range(-1f, 1f)]
    protected float offsetDistance;

    [Header("Rotations")]
    [SerializeField]
    protected Vector3 originRotation;
    [SerializeField]
    protected Vector3 offsetRotation;

    [Header("Sorting Order")]
    [SerializeField]
    protected int originSortOrder;
    [SerializeField]
    protected int offsetSortOrder;

    #endregion

    #region MonoBehaviour

    void Awake()
    {
        if (!Enum.TryParse(transform.name, true, out Container)) {
            Debug.LogError(@$"Failed to get list Container! Container: {transform.name}");
        }
    }

    void Start()
    {
        InitVariables();
    }

    void FixedUpdate()
    {
        if (ForceIntoPosition)
        {
            SetNodeBasedOnPrev(Tail);
            PropagateUpdates(Tail);
        }
    }

    #endregion

    #region Logic

    private void InitVariables()
    {
        head = null;
        tail = null;

        Container = Containers.error;
        stackDirection = Vector2.zero;

        originPosition = Vector2.zero;
        offsetDistance = 0;

        originRotation = Vector3.zero;
        offsetRotation = Vector3.zero;
    }

    #endregion

    #region Properties

    public static int MaxCapacitiy { get => maxCapacitiy; }

    public SC_Card Tail {
        get => tail;
        protected set { tail = value; }
    }
    public SC_Card Head {
        get => head;
        protected set { head = value; }
    }

    public Vector2 StackDirection { get => stackDirection.normalized; }

    #endregion

    #region Methods

    // Adding & Removing Nodes

    /// <summary>
    /// Removes a node from the list, if needed propgate change to the rest of the list
    /// </summary>
    protected void Remove(SC_Card node, bool propagate)
    {
        // connect prev to next
        if (node.Prev != null) {
            node.Prev.Next = node.Next;
        }
        else {
            Tail = node.Next;
        }
        // connect next to prev
        if (node.Next != null) {
            node.Next.Prev = node.Prev;
        }
        else {
            Head = node.Prev;
        }
        // update list
        if (propagate)
        {
            PropagateUpdates(node);
        }
        // remove node
        node.Next = null;
        node.Prev = null;
        count--;
    }

    /// <summary>
    /// Inserts a new card node into the linked list before the node toNext.
    /// </summary>
    protected void InsertBefore(SC_Card node, SC_Card toNext)
    {
        if (node == toNext || node == null) {
            Debug.LogError(@$"Failed to insert node! 
                              list: {this}
                              node: {node}
                              toNext: {toNext}");
            return;
        }

        node.Home = Container;
        node.IsFaceUp = KeepCardsFaceUp;
        node.FixPerspective = FixPerspective;

        // container is empty
        if (Head == null && Tail == null)
        {
            Tail = node;
            Head = node;
        }

        // new head
        else if (toNext == null)
        {
            Head.Next = node;
            Head = node;
        }

        // new tail
        else if (toNext.Prev == null)
        {
            Tail.Prev = node;
            Tail = node;
        }

        // insert in the middle
        else
        {
            SC_Card toPrev = toNext.Prev;
            node.Prev = toPrev;
            node.Next = toNext;
        }
        count++;
    }

    // Position, Rotation & Sorting order updates

    /// <summary>
    /// Sets card node position, rotation and sorting order based on its prev
    /// <para>
    /// if node is tail set to origin </para>
    /// </summary>
    protected void SetNodeBasedOnPrev(SC_Card node)
    {
        if (node == null || (node.Prev == null && node != Tail)) {
            Debug.LogError($"Failed to Set Node Parameters! card node ({nameof(node)}) is null.");
            return;
        }

        // tail set to origin
        if (node == Tail) {
            node.Index = 0;
            node.TargetPosition = originPosition;
            node.TargetRotation = originRotation;
            node.SortOrder = originSortOrder;
            return;
        }
        node.Index = node.Prev.Index + 1;
        node.TargetPosition = node.Prev.TargetPosition + (offsetDistance * StackDirection);
        node.TargetRotation = node.Prev.TargetRotation + offsetRotation;
        node.SortOrder = node.Prev.SortOrder + offsetSortOrder;
    }

    /// <summary>
    /// Sets card node position, rotation and sorting order based on its next
    /// <para>
    /// if node is head do nothing </para>
    /// </summary>
    protected void SetNodeBasedOnNext(SC_Card node) 
    {
        if (node == null || (node.Next == null)) {
            Debug.LogError($"Failed to Set Node Parameters! card node ({nameof(node)}) is null.");
            return;
        }
        node.Index = node.Next.Index - 1;  
        node.TargetPosition = node.Next.TargetPosition - (offsetDistance * StackDirection);
        node.TargetPosition = node.Next.TargetRotation - offsetRotation;
        node.SortOrder = node.Next.SortOrder - offsetSortOrder;
    }

    /// <summary>
    /// Propagate changes in position, rotation and sorting order thorgh the list.
    /// <para>
    /// Used when moving a card in the linked list </para>
    /// Dont forget to <see cref="SetNodeBasedOnPrev(SC_Card)"/> the source itself!
    /// </summary>
    protected void PropagateUpdates(SC_Card source)
    {
        if (source == null) {
            Debug.LogError($"Failed to Propagate Updates! source card node ({nameof(source)}) is null.");
            return; 
        }
        // porpagate up, set each node based on its prev
        SC_Card current = source.Next;
        while (current != null && current.Prev != null)
        {
            SetNodeBasedOnPrev(current);
            current = current.Next;
        }
        
        // porpagate down, set each node based on its next
        current = source.Prev;
        while (current != null && current.Next != null)
        {
            SetNodeBasedOnNext(current);
            current = current.Prev;
        }
    }

    protected void Clear()
    {
        if (Tail == null) {
            Debug.LogError("Failed to clear deck! tail is null.");
            return;
        }
        SC_Card current = Tail, next;
        while(current != null)  
        {
            next = current.Next;
            Destroy(current.gameObject);
            count--;
            current = next;
        }
    }

    #endregion

}



