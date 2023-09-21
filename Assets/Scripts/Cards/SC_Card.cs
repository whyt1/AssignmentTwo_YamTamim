using System;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// Script for a single card object. 
/// <para>
/// Is a node in linked lists <seealso cref="CardContainer"/>
/// </para>
/// Uses <see cref="CardView"/> for frontend logic 
/// </summary>
public class SC_Card : MonoBehaviour
{
    #region Cards Factory

    public static SC_Card CreateCard(string _name, Containers _home = Containers.Deck, 
                                     CardTypes _type = CardTypes.error)
    {
        if (_name == null || !_name.Contains('_')) { 
            Debug.LogError($"Failed to instantiate card! Name is null or not vaild");
            return null;
        }
        if (_type == CardTypes.error) { 
            string _typeStr = _name.Split('_')[1];
            if (!Enum.TryParse(_typeStr, true, out _type)) { 
                Debug.LogError($"Failed to instantiate{ _name }! is not a valid card name"); 
                return null; 
            }
        }
        GameObject cardObject = new(_name, typeof(SC_Card)); // , typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(BoxCollider2D));
        // GameObject cardObject = Instantiate(Resources.Load<GameObject>("Prefabs/Card_Prefab"));
        if (cardObject == null) { 
            Debug.LogError($"Failed to instantiate { _name }!"); 
            return null; 
        }

        SC_Card card = cardObject.InitComponent<SC_Card>();
        card.name = _name;
        card.Home = _home;
        card.type = _type;
        card.InitVariables();
        return card;  
    }
    void InitVariables()
    {
        size = new(1.65f, 2.4f);

        BoxCollider2D Collider = gameObject.InitComponent<BoxCollider2D>();
        Rigidbody2D Rigidbody = gameObject.InitComponent<Rigidbody2D>();
        SpriteRenderer spriteRenderer = gameObject.InitComponent<SpriteRenderer>();
        ActionManager actionManager = gameObject.InitComponent<ActionManager>();

        spriteRenderer.drawMode = SpriteDrawMode.Sliced;
        Collider.size = spriteRenderer.size = size;
        Rigidbody.isKinematic = true;
        Collider.isTrigger = true;
        actionManager.card = this;

        if (actionManager == null || actionManager.card == null) { Debug.LogError("Failed to create card! could not create Card Action Manager"); }
        node = new(_card: this);
        if (node == null) { Debug.LogError("Failed to create card! could not create Card Node"); }
        view = new(name, size, spriteRenderer, Rigidbody, transform);
        if (view == null) { Debug.LogError("Failed to create card! could not create Card View."); }
    }

    #endregion

    #region Fields 

    private Containers enumHome;
    [Header("General")]
    [SerializeField]
    private bool forceUpdate;
    [SerializeField]
    private CardTypes type;
    [SerializeField]
    private Vector2 size;
    [Space]
    [SerializeField]
    private ActionManager actionManager;
    [SerializeField]
    public CardAction action;
    [SerializeField]
    private CardNode node;  
    [SerializeField]
    private CardView view;
    public float deckSlowDown = 4;

    #endregion

    #region Properties 

    public bool IsActive { get => action != null && action.onClickDown != null; }

    /// <summary>
    /// Current Container of the card.
    /// <para></para>
    /// Changes <see cref="Transform.parent"/> on set.
    /// </summary>
    public Containers Home { 
        get => enumHome; 
        set {
            enumHome = value;
            node.home = SC_GameData.Instance.GetContainer(value);
            transform.parent = (node.home != null) ? node.home.transform : null;
        }
    }

    /// <summary>
    /// The type of the card. used to determine behavoiur. <para>
    /// Maybe use subclasses? </para>
    /// </summary>
    public CardTypes Type { get => type; }
    private GameStates CurrentState { get => SC_GameLogic.Instance.currentState; } // get the current game state from state machine or game logic 
    private Containers CurrentTurn { get; } // get the current turn from game logic

    #endregion

    #region Linked List API

    /// <summary>
    /// The card's index in the linked list <para></para>
    /// Automatically sets transform hierarchy on assignment.
    /// </summary>
    public int Index
    {
        get => node.index;
        set
        {
            node.index = value;
            transform.SetSiblingIndex(value);
        }
    }

    /// <summary>
    /// The next card in the linked list <para></para>
    /// Automatically sets double link on assignment.
    /// </summary>
    public SC_Card Next
    {
        get => node.next;
        set
        {
            if (value != null) { value.node.prev = this; }
            node.next = value;
        }
    }

    /// <summary>
    /// The prev card in the linked list <para></para>
    /// Automatically sets double link on assignment.
    /// </summary>
    public SC_Card Prev
    {
        get => node.prev;
        set
        {
            if (value != null) { value.node.next = this; }
            node.prev = value;
        }
    }

    // Methods
    public void Swap(SC_Card other)
    {
        if (other == null || this == other)
        {
            Debug.LogError($"Failed to swap card nodes! {this} and {other}, other is null or this == other");
            return;
        }
        // reseting Head and Tail as needed
        if (node.home.Head == this) { node.home.Head = other; }
        else if (node.home.Head == other) { node.home.Head = this; }
        if (node.home.Tail == this) { node.home.Tail = other; }
        else if (node.home.Tail == other) { node.home.Tail = this; }
        SC_Card
        thisNext = this.Next, otherNext = other.Next,
        thisPrev = this.Prev, otherPrev = other.Prev;
        // adjacent nodes 
        if (otherNext == this)
        {
            this.Next = other; this.Prev = otherPrev;
            other.Prev = this; other.Next = thisNext;
        }
        else if (otherPrev == this)
        {
            this.Prev = other; this.Next = otherNext;
            other.Next = this; other.Prev = thisPrev;
        }
        // non adjacent nodes 
        else
        {
            this.Next = otherNext; this.Prev = otherPrev;
            other.Next = thisNext; other.Prev = thisPrev;
        }
        (this.Index, other.Index) = (other.Index, this.Index);
    }

    #endregion

    #region View API

    public bool IsDragged
    {
        get => view != null && view.IsDragged == true;
        set {
            if (view != null) {
                view.IsDragged = value;
                IsHovered = false;
                // card falls into place after drag
                if (!value) { Reposition(); }
            }
        }
    }

    public bool FixPerspective
    {
        get => (view != null) ? view.FixPerspective : default;
        set { if (view != null) view.FixPerspective = value; }
    }

    public bool IsFaceUp
    {
        get => (view != null) ? view.IsFaceUp : default;
        set { if (view != null) view.IsFaceUp = value; }
    }

    /// <summary>
    /// Get current Current Rotation <para></para>
    /// Set current Current Rotation with no transition.
    /// </summary>
    public Vector3 CurrentRotation
    {
        get => (view != null) ? view.CurrentRotation : default;
        set { if (view != null) view.SetCurrentRotation(value); }
    }

    /// <summary>
    /// Get target Current Rotation <para></para>
    /// Set target Current Rotation triggering movment.
    /// </summary>
    public Vector3 TargetRotation
    {
        get => (view != null) ? view.TargetRotation : default;
        set { if (view != null) view.TargetRotation = value; }
    }

    /// <summary>
    /// Get current position <para></para>
    /// Set current position with no transition.
    /// </summary>
    public Vector2 Position
    {
        get => (view != null) ? view.CurrentPosition : default;
        set { if (view != null) view.SetCurrentPosition(value); }
    }

    /// <summary>
    /// Get target position <para></para>
    /// Set target position triggering movment.
    /// </summary>
    public Vector2 TargetPosition {
        get => (view != null) ? view.TargetPosition : default; 
        set { if (view != null) view.TargetPosition = value; }
    }

    public int SortOrder
    {
        get => (view != null) ? view.SortOrder : default;
        set { if (view != null) view.SortOrder = value; }
    }

    private bool IsHovered
    {
        get => (view != null) ? view.IsHovered : default;
        set { if (view != null) view.IsHovered = value; }
    }

    public Vector2 OnHoverOffset
    {
        get => (view != null) ? view.onHoverOffset : default;
        set { if (view != null) view.onHoverOffset = value; }
    }

    #endregion

    #region MonoBehaviour

    // Initialize
    void Awake()
    {
        InitVariables();
    }

    // Input Events
    void OnMouseDown()
    {
        Debug.Log($"{this} was clicked DOWN");
        if (OnClickDown != null) { OnClickDown(); }
    }

    void OnMouseUpAsButton()
    {
        Debug.Log($"{this} was clicked UP");
        if (OnClickUp != null) { OnClickUp(); }
    }

    void OnMouseDrag()
    {
        if (IsDragged) { 
            MoveOnDrag();
            RepositionOnDrag();
        }
    }

    void OnMouseEnter()
    {
        if (IsActive && !IsDragged)
        {
            IsHovered = true; // used to bring card to front 
            // is hovered automatic sets to false when reaching the front
        }
    }

    // Updates (Keep Game Logic Out! only run with triggers when needed)
    void FixedUpdate()
    {
        if (view == null) {
            Debug.LogError($"Failed to Update {name}! View is null.");
            return; 
        }

        if (view.IsMoving || forceUpdate) { view.UpdatePosition(); }
        if (view.IsRotating || forceUpdate) { view.UpdateRotation(); }
    }

    #endregion

    #region Card Actions

    private void Reposition()
    {
        if (node == null | node.home == null)
        {
            Debug.LogError("Failed to Reposition! home container in node is null");
            return;
        }
        node.home.ResetContainer();
        // node.home.SetNodeBasedOnNext(this);
        // node.home.PropagateUpdatesToTail(this);
        // node.home.PropagateUpdatesToHead(this);
    }

    private void RepositionOnDrag()
    {
        if (Home == Containers.Deck) { return; } // cant re order deck
        if (Prev != null && Position.x < Prev.Position.x)
        {
            // swap with prev
            this.Swap(Prev);
            // move prev, now next to the next spot 
            node.home.SetNodeBasedOnNext(Next);
        }
        else if (Next != null && Next.Position.x < Position.x)
        {
            // swap with next
            Next.Swap(this);
            // move next, now prev to the prev spot
            node.home.SetNodeBasedOnPrev(Prev);
        }
    }

    private void MoveOnDrag()
    {
        TargetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }


    public void ChangeHome(Containers _newHome)
    {
        if (_newHome == Containers.error) { return; }
        CardContainer newHome = SC_GameData.Instance.GetContainer(_newHome);
        if (newHome == null || node == null)
        {
            Debug.LogError($"Failed to Change Home! new home: {newHome}, card node: {node}");
            return;
        }

        CardContainer oldHome = SC_GameData.Instance.GetContainer(enumHome);
        if (oldHome != null) { oldHome.Remove(this); }
        newHome.InsertBefore(this);
        newHome.ResetContainer();   
    }

    public UnityAction OnClickDown { get => action != null ? action.onClickDown : null; }

    public UnityAction OnClickUp { get => action != null ? action.onClickUp : null; }
    #endregion

}