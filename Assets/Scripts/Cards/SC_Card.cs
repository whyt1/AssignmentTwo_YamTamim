using System;
using TMPro;
using UnityEngine;
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
        return card;  
    }
    private void InitVariables()
    {
        size = new(1.65f, 2.4f);

        BoxCollider2D Collider = gameObject.InitComponent<BoxCollider2D>();
        Rigidbody2D Rigidbody = gameObject.InitComponent<Rigidbody2D>();
        SpriteRenderer spriteRenderer = gameObject.InitComponent<SpriteRenderer>();

        spriteRenderer.drawMode = SpriteDrawMode.Sliced;
        Collider.size = spriteRenderer.size = size;
        Rigidbody.isKinematic = true;
        Collider.isTrigger = true;

        node = new();
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
    private CardNode node;  
    [SerializeField]
    private CardView view;
    public float deckSlowDown =  4;

    #endregion

    #region Properties 

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
            transform.parent = node.home != null ? node.home.transform : null;
        }
    }

    /// <summary>
    /// The type of the card. used to determine behavoiur. <para>
    /// Maybe use subclasses? </para>
    /// </summary>
    public CardTypes Type { get => type; }

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

    }

    #endregion

    #region View API

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
        view.IsDragged = true;
        // view.TargetRotation = Vector3.zero;
        // view.IsFaceUp = true;
    }
    void OnMouseUp()
    {
        view.IsDragged = false;
        view.TargetRotation =  new(45, 0, -15);
        // view.TargetPosition = Vector3.zero;
        // view.IsFaceUp = false;
    }
    void OnMouseDrag()
    {
        view.TargetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
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

    #region Delegates
    /// <summary>
    /// <para>
    /// Used by Action Manager to set <see cref="PossibleActions"/>.
    /// </para>
    /// <seealso cref="OnClickUpCard"/>
    /// </summary>
    public static Action<SC_Card> OnClickDownCard;
    public static Action OnClickUpCard;

    public static Action OnTakeBackCard;
    public static Action GetAction = OnTakeBackCard;
    public static Action OnGiveCard;
    public static Action OnStealCard;
    public static Action OnDrawCard;
    public static Action OnPlayCard;
    #endregion

}