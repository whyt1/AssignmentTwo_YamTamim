using System;
using UnityEngine;

/// <summary>
/// Linked list of cards, using <see cref="SC_Card"/>
/// </summary>
[Serializable]
public abstract class CardContainer : MonoBehaviour
{

    #region Fields

    [SerializeField]
    public static int maxCapacitiy = 57;

    [SerializeField]
    public bool ForceIntoPosition;
    [SerializeField]
    public int count;

    [Header("Nodes")]
    [SerializeField]
    public SC_Card tail;
    [SerializeField]
    public SC_Card head;

    [SerializeField]
    protected ContainerSettings containerSettings;

    #endregion

    #region MonoBehaviour

    void Awake()
    {
        containerSettings = new();
        if (!Enum.TryParse(transform.name, true, out CS.Container)) {
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
            ResetContainer();
        }
    }

    #endregion

    #region Logic

    private void InitVariables()
    {
        head = null;
        tail = null;

        CS.Container = Containers.error;
        CS.stackDirection = Vector2.zero;

        CS.floor = Vector2.zero;
        CS.offsetDistance = 0;

        CS.originRotation = Vector3.zero;
        CS.offsetRotation = Vector3.zero;
    }

    protected void ResetContainer()
    {
        SetNodeBasedOnPrev(Tail);
        PropagateUpdatesToHead(Tail);
    }

    #endregion

    #region Properties

    protected ContainerSettings CS { get => containerSettings; }

    public static int MaxCapacitiy { get => maxCapacitiy; }

    public SC_Card Tail {
        get => tail;
        protected set { tail = value; }
    }
    public SC_Card Head {
        get => head;
        protected set { head = value; }
    }

    public Vector2 StackDirection { get => CS.stackDirection.normalized; }

    #endregion

    #region Methods

    // Adding & Removing Nodes

    /// <summary>
    /// Removes a node from the list, if needed propagate change to the rest of the list
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
            PropagateUpdatesToTail(node);
            PropagateUpdatesToHead(node);
        }
        // remove node
        node.Next = null;
        node.Prev = null;
        count--;
    }

    /// <summary>
    /// Inserts a new card node into the linked list before the node toNext. <para>
    /// </para>
    /// If toNext is null inserts as new head.
    /// </summary>
    protected void InsertBefore(SC_Card node, SC_Card toNext = null)
    {
        if (node == toNext || node == null) {
            Debug.LogError(@$"Failed to insert node! 
                              list: {this}
                              node: {node}
                              toNext: {toNext}");
            return;
        }

        node.Home = CS.Container;
        node.IsFaceUp = CS.KeepCardsFaceUp;
        node.FixPerspective = CS.FixPerspective;

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

        // insert in the middleRange
        else
        {
            SC_Card toPrev = toNext.Prev;
            node.Prev = toPrev;
            node.Next = toNext;
        }
        count++;
    }

    // Position, CurrentRotation & Sorting order updates

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
            node.TargetPosition = CS.floor;
            node.TargetRotation = CS.originRotation;
            node.SortOrder = CS.originSortOrder;
            return;
        }
        node.Index = node.Prev.Index + 1;
        node.TargetPosition = node.Prev.TargetPosition + (CS.offsetDistance * StackDirection);
        node.TargetRotation = node.Prev.TargetRotation + CS.offsetRotation;
        node.SortOrder = node.Prev.SortOrder + CS.offsetSortOrder;
    }

    /// <summary>
    /// iterates over the list, don't use in update! <para>
    /// </para>
    /// Propagate changes in position, rotation and sorting order thorgh the list.
    /// <para>
    /// Used when moving a card in the linked list </para>
    /// Dont forget to <see cref="SetNodeBasedOnPrev(SC_Card)"/> the source itself!
    /// </summary>    
    protected void PropagateUpdatesToHead(SC_Card source)
    {
        if (source == null)
        {
            Debug.LogError($"Failed to Propagate Updates! source card node ({nameof(source)}) is null.");
            return;
        }
        int i = 0;
        // porpagate up, set each node based on its prev
        SC_Card current = source.Next;
        while (current != null && current.Prev != null && i < MaxCapacitiy)
        {
            // if card would go out of bounds, update based on next
            if (CS.ceiling.y - current.TargetPosition.y >= 0)
            {
                SetNodeBasedOnPrev(current);
            }
            else { SetNodeBasedOnNext(current); }
            
            current = current.Next;
            i++;
        }
        if (i == MaxCapacitiy)
        {
            Debug.LogError(@$"Infinte while loop. 
                              Stopped after {MaxCapacitiy} iterations. 
                              Problem with linked list connections.");
        }
    }


    /// <summary>
    /// if node is head set to ceiling 
    /// </summary>
    protected void SetNodeBasedOnNext(SC_Card node) 
    {
        if (node == null || (node.Next == null && node != Head)) {
            Debug.LogError($"Failed to Set Node Parameters! card node ({nameof(node)}) is null.");
            return;
        }
        // Head set to ceiling
        if (node == Head)
        {
            node.Index = count;
            node.TargetPosition = CS.ceiling;
            node.TargetRotation = CS.originRotation + CS.offsetRotation * count;
            node.SortOrder = CS.originSortOrder + CS.offsetSortOrder * count;
            return;
        }
        node.Index = node.Next.Index - 1;  
        node.TargetPosition = node.Next.TargetPosition - (CS.offsetDistance * StackDirection);
        node.TargetRotation = node.Next.TargetRotation - CS.offsetRotation;
        node.SortOrder = node.Next.SortOrder - CS.offsetSortOrder;
    }

    protected void PropagateUpdatesToTail(SC_Card source)
    {
        if (source == null) {
            Debug.LogError($"Failed to Propagate Updates! source card node ({nameof(source)}) is null.");
            return; 
        }
        // porpagate down, set each node based on its next
        int i = 0;
        SC_Card current = source.Prev;
        while (current != null && current.Next != null && i < MaxCapacitiy)
        {
            // if card would go out of bounds, update based on prev
            if (current.TargetPosition.y - CS.floor.y >= 0)
            {
                SetNodeBasedOnNext(current);
            }
            else { SetNodeBasedOnPrev(current); }
            current = current.Prev;
            i++;
        }
        if (i == MaxCapacitiy)
        {
            Debug.LogError(@$"Infinte while loop. 
                              Stopped after {MaxCapacitiy} iterations. 
                              Problem with linked list connections.");
        }
    }


    protected void Clear()
    {
        if (Tail == null) {
            Debug.LogError("Failed to clear deck! tail is null.");
            return;
        }
        int i = 0;
        SC_Card current = Tail, next;
        while(current != null && i < MaxCapacitiy)  
        {
            next = current.Next;
            Destroy(current.gameObject);
            count--;
            current = next;
            i++;
        }
    }

    protected SC_Card GetRandomCard(int i)
    {
        SC_Card random = Tail;
        int randomIndex = UnityEngine.Random.Range(0, i);
        for (int j = 0; j < randomIndex && random != null; j++)
        {
            random = random.Next;
        }
        return random;
    }

    #endregion

}



