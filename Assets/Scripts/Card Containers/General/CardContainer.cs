using System;
using System.Reflection;
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
    public static int maxCapacitiy = 57;

    public SC_Card draggedCard;

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

    public virtual void InitVariables()
    {
        head = null;
        tail = null;

        CS.Container = Containers.error;
        CS.stackDirection = Vector2.zero;

        CS.Origin = Vector2.zero;
        CS.offsetDistance = 0;

        CS.originRotation = Vector3.zero;
        CS.offsetRotation = Vector3.zero;
    }

    public virtual void ResetContainer()
    {
        SetNodeBasedOnPrev(Tail);
        PropagateUpdatesToHead(Tail);
    }

    /// <summary>
    /// shuffle the Container using the Fisher-Yates SetUpShuffle algorithm
    /// </summary>
    public void Shuffle()
    {
        if (Tail == null || Head == null)
        {
            Debug.LogError("Failed to SetUpShuffle! list is empty? tail or head null");
        }
        SC_Card current, random, next;
        int i = 0;
        current = Head;
        while (current != null && i < MaxCapacitiy)
        {
            next = current.Prev;

            // get a random card from the list
            random = GetRandomCard(count - 1 - i);
            if (random == null) { Debug.LogError("Failed to shuffle, got null on random"); return; }
            // swap current and random cards 
            current.Swap(random);
            // move on to next card
            current = next;
            i++;
        }
    }

    #endregion

    #region Properties

    public ContainerSettings CS { get => containerSettings; }

    public static int MaxCapacitiy { get => maxCapacitiy; }

    public SC_Card Tail {
        get => tail;
        set { tail = value; }
    }
    public SC_Card Head {
        get => head;
        set { head = value; }
    }

    public Vector2 StackDirection { get => CS.stackDirection.normalized; }
    public SC_Card DraggedCard { get => draggedCard; set => draggedCard = value; }

    #endregion

    #region Methods

    // Adding & Removing Nodes

    /// <summary>
    /// Removes a node from the list, if needed propagate change to the rest of the list
    /// </summary>
    public virtual void Remove(SC_Card node)
    {
        if (node == null || node.Home != CS.Container) { 
            Debug.LogError("Failed to Remove card! null or not in container."); 
            return; 
        }
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
        // remove node
        node.Next = null;
        node.Prev = null;
        count--;
        PropagateUpdatesToHead(Tail);
    }

    /// <summary>
    /// Inserts a new card node into the linked list before the node toNext. <para>
    /// </para>
    /// If toNext is null inserts as new head.
    /// </summary>
    public virtual void InsertBefore(SC_Card node, SC_Card toNext = null)
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
        node.OnHoverOffset = CS.OnHoverOffset;

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
        // SetNodeBasedOnPrev(node);
    }

    // Position, CurrentRotation & Sorting order updates

    /// <summary>
    /// Sets card node position, rotation and sorting order based on its prev
    /// <para>
    /// if node is tail set to Origin </para>
    /// </summary>
    public void SetNodeBasedOnPrev(SC_Card node)
    {
        if (node == null) {
            Debug.LogError($"Failed to Set Node Parameters! card {node} is null.");
            return;
        }
        if (node.Prev == null && node != Tail) {
            Debug.LogError($"Failed to Set Node Parameters! card {node} has no prev without being tail.");
            return;
        }

        // tail set to Origin
        if (node == Tail) {
            node.Index = 0;
            node.TargetPosition = CS.Origin;
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
    public void PropagateUpdatesToHead(SC_Card source)
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
            SetNodeBasedOnPrev(current);
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
    /// if node is head set to Ceiling 
    /// </summary>
    public void SetNodeBasedOnNext(SC_Card node) 
    {
        if (node == null || (node.Next == null && node != Head)) {
            Debug.LogError($"Failed to Set Node Parameters! card node is null.");
            return;
        }
        // Head set to Ceiling
        if (node == Head)
        {
            node.Index = count;
            node.TargetPosition = CS.Ceiling;
            node.TargetRotation = CS.originRotation + CS.offsetRotation * count;
            node.SortOrder = CS.originSortOrder + CS.offsetSortOrder * count;
            return;
        }
        node.Index = node.Next.Index - 1;  
        node.TargetPosition = node.Next.TargetPosition - (CS.offsetDistance * StackDirection);
        node.TargetRotation = node.Next.TargetRotation - CS.offsetRotation;
        node.SortOrder = node.Next.SortOrder - CS.offsetSortOrder;
    }

    public void PropagateUpdatesToTail(SC_Card source)
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
            SetNodeBasedOnNext(current);
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

    // General methods

    public void Clear()
    {
        if (Tail == null) {
            Debug.LogError("Failed to clear container! tail is null.");
            return;
        }
        int i = 0;
        SC_Card current = Tail, next;
        while(current != null && i < MaxCapacitiy)  
        {
            next = current.Next;
            Remove(current);
            Destroy(current.gameObject);
            Destroy(current);
            current = next;
            i++;
        }
    }

    public SC_Card GetRandomCard(int i)
    {
        SC_Card random = Tail;
        int randomIndex = UnityEngine.Random.Range(0, i);
        for (int j = 0; j < randomIndex && random != null; j++)
        {
            random = random.Next;
        }
        return random;
    }

    public SC_Card GetCard(int index = -1, CardTypes type = CardTypes.error, string name = null)
    {
        if (index == -1 && type == CardTypes.error && name == null) {
            return Head;
        }
        SC_Card card = Tail;
        int i = 0;
        while (card != null && i < MaxCapacitiy)
        {
            if (card.Index == index || card.name == name || card.Type == type) {
                return card;
            }
            card = card.Next;
        }
        Debug.LogError($"Failed to find card (index: {index}, type: {type}, name: {name}) in container {this.CS.Container}");
        return null;
    }

    /// <summary>
    /// Used when someone explodes to make sure he has defuses and to activate them. <para>
    /// Or when another player played to check if current player has a nope.
    /// </para>
    /// Not using <see cref="CardContainer.GetCard"/> becuase we want all defuses not just the first.
    /// </summary>
    public bool CheckHandForType(CardTypes type)
    {
        bool foundType = false;
        SC_Card current = Tail; int i = 0;
        while (current != null && i < MaxCapacitiy)
        {
            // set defuse active and continue 
            if (current.Type == type)
            {
                current.action = type == CardTypes.Defuse ? new Defuse(current) : 
                                 type == CardTypes.Nope   ? new Nope(current) : 
                                 null;
                foundType = true;
            }
            current = current.Next; i++;
        }
        return foundType;
    }

    public virtual void PositionPlayer(int numberOfPlayers)
    {

    }

    #endregion

}



