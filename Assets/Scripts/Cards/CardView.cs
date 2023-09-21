using System;
using System.Collections;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
/// <summary>
/// <see cref="SC_Card"/> Used in card to set face and update position
/// </summary>
[Serializable]
// [CreateAssetMenu(fileName = "CardViewSettings", menuName = "Custom/Card View Settings")]
public class CardView // : ScriptableObject
{
    #region Fields

    [Header("Sprite")]
    [SerializeField]
    private SpriteRenderer SpriteRenderer;
    [SerializeField]
    private string backSpriteName;
    [SerializeField]
    private string frontSpriteName;

    [Space]
    [Header("Perspective")]
    [SerializeField]
    private Transform transform;
    [SerializeField]
    private bool fixPerspective;
    [SerializeField]
    private float perspectiveScaler;
    [SerializeField]
    private bool isHovered;
    [SerializeField]
    public Vector2 onHoverOffset;
    [SerializeField]
    private float fallBackDelay;
    [SerializeField]
    private Vector3 currentScale;

    [Space]
    [Header("Sorting Order")]
    [SerializeField]
    public static readonly int frontSortingOrder = 60;
    [SerializeField]
    public static readonly int frontZ = 60;
    [SerializeField]
    private int sortOrder;
    [SerializeField]
    private int positionZ;

    [Space]
    [Header("Movment")]
    [SerializeField]
    private Rigidbody2D Rigidbody;
    [SerializeField]
    private float epsilon;
    [SerializeField]
    private float dragSpeed;
    [SerializeField]
    public float repositionSpeed;
    [SerializeField]
    private Vector2 targetPosition;
    [SerializeField]
    private bool isMoving;
    [SerializeField]
    private bool isDragged;

    [Space]
    [Header("CurrentRotation")]
    [SerializeField]
    private float initalYRotation;
    [SerializeField]
    private static float rotationSpeed;
    [SerializeField]
    private Vector3 targetRotation;
    private Quaternion tarRot;
    private Quaternion curRot;
    [SerializeField]
    private bool isRotating;
    [SerializeField]
    private bool isFaceUp;
    [SerializeField]
    private float rotationDuringMovment;
    [SerializeField]
    private float maxRotationDuringMovment;

    #endregion

    #region Constracor

    public CardView(string _name, Vector2 size, SpriteRenderer _SpriteRenderer, Rigidbody2D _Rigidbody, Transform _Transform)
    {
        if (_SpriteRenderer == null || _name == null || _Rigidbody == null || _Transform == null) {
            Debug.LogError(@$"Failed to initialize CardView! 
                              name: {_name} 
                              Renderer: {_SpriteRenderer} 
                              Rigidbody: {_Rigidbody}
                              Transform: {_Transform}");
            return;
        }

        transform = _Transform;
        Rigidbody = _Rigidbody;
        SpriteRenderer = _SpriteRenderer;

        frontSpriteName = _name != null ? _name.Contains('_') ? _name.Replace("Card_", "Sprite_")
                                        : _name : _name;
        backSpriteName = "Sprite_cardback";

        SpriteRenderer.sprite = SC_GameData.Instance.GetCardSprite(CurrentSprite);
        SpriteRenderer.size = size;
        isHovered = false;
        onHoverOffset = new(0,0.75f);
        fallBackDelay = 0.25f;
        fixPerspective = true;
        perspectiveScaler = 0.5f;

        epsilon = 0.001f;
        dragSpeed = 10f;
        repositionSpeed = 7.5f;
        isMoving = false;
        isDragged = false;

        rotationSpeed = 120;
        isRotating = false;
        initalYRotation = CurrentRotation.y;
        rotationDuringMovment = 2;
        maxRotationDuringMovment = 20;
    }

    #endregion

    #region Properties

    // Zoom & Sorting Order
    /// <summary>
    /// Triggers change in sort order on set.
    /// <para> </para>
    /// <seealso cref="UpdateSortOrder"/>
    /// </summary>
    public int SortOrder { get => sortOrder; set { sortOrder = value; UpdateSortOrder(value); } }
    public Vector3 CurrentScale {
        get => (transform != null) ? transform.localScale : Vector3.one;
        set { if (transform != null) { transform.localScale = value; } }
    }

    // Position
    public bool IsDragged {
        get => isDragged;
        set { isDragged = value; isHovered = false; UpdateSortOrder(); SetCurrentPosition(TargetPosition); }
    }
    /// <summary>
    /// Triggers movment to given position 
    /// <para> </para>
    /// <seealso cref="UpdatePosition"/>
    /// </summary>
    public Vector2 TargetPosition {
        get => targetPosition;
        set { targetPosition = value;
            isMoving = true;
        }
    }
    public Vector2 CurrentPosition { get => Rigidbody != null ? Rigidbody.position : default; }
    private float Distance { get => (targetPosition + (isHovered ? onHoverOffset : Vector2.zero) - CurrentPosition).magnitude; }
    private Vector2 Direction { get => (targetPosition + (isHovered ? onHoverOffset : Vector2.zero) - CurrentPosition).normalized; }
    public bool IsMoving { get => isMoving; }
    public bool FixPerspective { get => fixPerspective; set { fixPerspective = value; UpdatePerspective(); } }

    // CurrentRotation & Face 
    private string CurrentSprite { get => IsFaceUp ? frontSpriteName : backSpriteName; }

    /// <summary>
    /// Flag for 180 degrees rotation in y angle 
    /// </summary>
    public bool IsFaceUp {
        get => 90 <= CurrentRotation.y && CurrentRotation.y <= 270;
        set 
        { 
            if (isFaceUp == value) { 
                return; 
            }
            isFaceUp = value;
            TargetRotation = TargetRotation;
        }
    }

    /// <summary>
    /// Triggers rotation to given angles. 
    /// <para>
    /// y angle handled by <see cref="IsFaceUp"/>
    /// </para>
    /// </summary>
    public Vector3 TargetRotation {
        get => targetRotation;
        set { targetRotation = isFaceUp ? new Vector3(value.x, initalYRotation + 180, value.z) // -1 * value.z to fix flips on z when y is 180
                                        : new Vector3(value.x, initalYRotation, value.z);
            tarRot = Quaternion.Euler(targetRotation);
            isRotating = true;
        }
    }
    public Vector3 CurrentRotation { get => transform != null ? transform.rotation.eulerAngles : default; }
    public bool IsRotating { get => isRotating; }
    private float MaxRot { get => maxRotationDuringMovment; }
    public bool IsHovered { get => isHovered; 
        set { if (isHovered == value) { return; }
            isHovered = value;
            UpdateSortOrder();
            isMoving = true;
        } 
    }
    /// <summary>
    /// Used to delay fall back to place after hover offset to stop jumping
    /// </summary>
    private IEnumerator FallIntoPlace()
    {
        yield return new WaitForSeconds(fallBackDelay);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Updates Sorting Order and Z position 
    /// </summary>
    private void UpdateSortOrder()
    {
        if (SpriteRenderer == null || transform == null) {
            return;
        };
        SpriteRenderer.sortingOrder = isDragged ? frontSortingOrder+1 : (isHovered ? frontSortingOrder : SortOrder);

        float zPosition = isDragged ? -frontZ-1 : (isHovered ? -frontZ : -SortOrder);
        transform.position = new(
            transform.position.x,
            transform.position.y,
            zPosition
        );
    }
    private void UpdateSortOrder(int _sortOrder)
    {
        if (SpriteRenderer == null || transform == null)
        {
            return;
        };
        SpriteRenderer.sortingOrder = _sortOrder;
        transform.position = new(
            transform.position.x,
            transform.position.y,
            -_sortOrder
        );
    }

    /// <summary>
    /// Called every update frame, runs only if card is moving and has rigidbody <para></para>
    /// Starts a slight rotation in the direction of movment.
    /// </summary>
    public void UpdatePosition()
    {
        if (!isMoving || Rigidbody == null) {
            return;
        }
        if (Distance <= epsilon) {
            Rigidbody.velocity = Vector3.zero;
            CurrentPosition.Set(
                MathF.Round(CurrentPosition.x),
                MathF.Round(CurrentPosition.y)
            );
            tarRot = Quaternion.Euler(TargetRotation);
            isMoving = false;
            IsHovered = false;
            return;
        }
        UpdatePerspective();
        Rigidbody.velocity = (isDragged ? dragSpeed : repositionSpeed) * Distance * Direction;

        float addedRotationY = Rigidbody.velocity.y > MaxRot ? MaxRot : Rigidbody.velocity.y < -MaxRot ? -MaxRot : Rigidbody.velocity.y;
        float addedRotationX = Rigidbody.velocity.x > MaxRot ? MaxRot : Rigidbody.velocity.x < -MaxRot ? -MaxRot : Rigidbody.velocity.x;
        tarRot = Quaternion.Euler(TargetRotation + rotationDuringMovment * new Vector3(addedRotationY, addedRotationX, 0));
        isRotating = true;
    }

    /// <summary>
    /// Force the card into given position with no transition.
    /// </summary>
    public void SetCurrentPosition(Vector2 newPosition)
    {
        if (newPosition == null || Rigidbody == null) {
            Debug.Log($"Failed to Set Current Position! new Position: {newPosition} Rigidbody: {Rigidbody}");
            return;
        }
        Rigidbody.position = new(newPosition.x, newPosition.y);
    }

    /// <summary>
    /// Cards get bigger closer to the bottom screen and smaller away from the bottom screen. <para></para>
    /// Uses <see cref="isMoving"/> to decide when to run.
    /// <para></para>
    /// Called during movment in UpdatePosition.
    /// </summary>
    private void UpdatePerspective()
    {
        if (!isMoving || Rigidbody == null) { return; }
        float newScale;
        if (fixPerspective)
        {
            Vector2 normalizedPosition = CurrentPosition / SC_GameData.Instance.screenSize;
            // max scale at 0,-1 
            newScale = -perspectiveScaler * normalizedPosition.y + 1;
        }
        else 
        {
            newScale = 1;
        }

        CurrentScale = Vector3.one * newScale;
    }

    /// <summary>
    /// Called every update frame, runs only if card is rotating and has rigidbody
    /// <para>
    /// Calls <see cref="UpdateSprite"/> to update sprite if needed </para>
    /// </summary>
    public void UpdateRotation()
    {
        if (!isRotating || Rigidbody == null) {
            return;
        }

        if (Quaternion.Angle(curRot, tarRot) <= epsilon)
        {
            CurrentRotation.Set(
                MathF.Round(CurrentRotation.x), 
                MathF.Round(CurrentRotation.y), 
                MathF.Round(CurrentRotation.z)
            );
            SetCurrentRotation(Quaternion.Euler(CurrentRotation));
            isRotating = false;
            return;
        }

        SetCurrentRotation(Quaternion.RotateTowards(curRot, tarRot, rotationSpeed * Time.deltaTime * (1+Distance)));
        UpdateSprite();
    }

    /// <summary>
    /// Force the card into given CurrentRotation with no transition.
    /// </summary>
    public void SetCurrentRotation(Vector3 newRotation)
    {
        transform.rotation = Quaternion.Euler(newRotation.x,
                                              newRotation.y,
                                              newRotation.z);
    }
    /// <summary>
    /// Used in update CurrentRotation
    /// </summary>
    private void SetCurrentRotation(Quaternion newRotation)
    {
        curRot = transform.rotation = newRotation;
        CurrentRotation.Set(
            newRotation.eulerAngles.x,
            newRotation.eulerAngles.y,
            newRotation.eulerAngles.z
        );
    }    

    /// <summary>
    /// Called during rotation
    /// </summary>
    private void UpdateSprite()
    {
        if (SpriteRenderer == null || SpriteRenderer.sprite == null || SpriteRenderer.sprite.name == CurrentSprite) {
            return;
        }
        Sprite _sprite = SC_GameData.Instance.GetCardSprite(CurrentSprite);
        if (_sprite == null) { 
            Debug.LogError($"Failed to Update Sprite! failed to get { (CurrentSprite) } for { frontSpriteName }"); 
            return;
        }
        SpriteRenderer.sprite = _sprite;
        SpriteRenderer.flipX = IsFaceUp;
        
    }

    #endregion
}
