using System.Collections;
using System.Net.NetworkInformation;
using UnityEngine;

public class SC_Discards : CardContainer
{
    #region Variables

    private bool isMovingCards;
    public bool isRetrieveDiscarded;
    public float retrieveRange;
    private SC_Card cardToMove;
    private float moveCardsSpeed;

    #endregion
    #region MonoBehaviour
    void OnEnable()
    {
        SC_GameLogic.OnStateTransition += OnStateTransition;
    }
    void OnDisable()
    {
        SC_GameLogic.OnStateTransition -= OnStateTransition;
    }
    void Start()
    {
        InitVariables();
    }

    private void Update()
    {
        if (isRetrieveDiscarded && count > 0) { RetrieveDiscarded(); }
    }

    #endregion

    public override void InitVariables()
    {
        isMovingCards = false;
        isRetrieveDiscarded = false;
        retrieveRange = 2;
        cardToMove = Head;
        moveCardsSpeed = 1;

        head = null;
        tail = null;

        containerSettings = new()
        {
            Container = Containers.Discards,
            KeepCardsFaceUp = true,
            FixPerspective = false,
            stackDirection = new(0, 1),
            OnHoverOffset = Vector2.up / 2,

            Ceiling = new(3, 4),
            Origin = new(3, -1),
            offsetDistance = 0.06f,

            originRotation = new(50, 0, 15),
            offsetRotation = Vector3.zero,

            originSortOrder = 0,
            offsetSortOrder = 1
        };
    }

    public override void InsertBefore(SC_Card node, SC_Card toNext)
    {
        base.InsertBefore(node, toNext);
        // reset actions for discarded cards
        node.action = null;
        SetNodeBasedOnPrev(node);
    }

    public override void Remove(SC_Card node)
    {
        base.Remove(node);
        isRetrieveDiscarded = false;
        isMovingCards = false;
        ResetContainer();
    }

    public void RetrieveDiscarded()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (!isMovingCards && Mathf.Abs(mousePosition.x - CS.Origin.x) < retrieveRange)
        {
            isMovingCards = true;
            StartCoroutine(MoveCardsUp(mousePosition));
        }
    }

    private IEnumerator MoveCardsUp(Vector2 mousePosition)
    {
        if (cardToMove == null) { cardToMove = Head; }
        // { Debug.LogError("Failed to move card! card to move is null."); yield break; }
        if (cardToMove.Position.y > mousePosition.y) {
            SetNodeBasedOnNext(cardToMove);
            cardToMove = cardToMove.Prev;
        } 
        else {
            SetNodeBasedOnPrev(cardToMove);
            cardToMove = cardToMove.Next;
        }
        yield return new WaitForSeconds(1 / moveCardsSpeed);
        isMovingCards = false;
    }
    
    private void OnStateTransition(GameStates state)
    {
        isRetrieveDiscarded = false;
    }
}
