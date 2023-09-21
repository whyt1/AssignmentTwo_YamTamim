using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_Deck : CardContainer
{
    #region Fields

    [Header("Card Effects")]
    [Space]
    [Header("See The Future")]
    [SerializeField]
    private Vector2 seePosition;
    [SerializeField]
    private Vector2 seeOffset;
    [SerializeField]
    private float seeTheFutureDelay = 0.5f;
    [SerializeField]
    private SC_Card seeTheFutureTemp;

    [Header("Defuse")]
    [SerializeField]
    private int numberOfDefuses;
    [SerializeField]
    public SC_Card bomb;
    [SerializeField]
    private SC_Card toNextForBomb;
    [SerializeField]
    private float bombInsertRange;
    [SerializeField]
    private SC_Card currentMoving;
    [SerializeField]
    private float deckScrollSpeed;
    [SerializeField]
    private float middleRange;
    [SerializeField]
    public bool isDefusing; // game state 
    [SerializeField]
    public bool isBombPlanted; // game state 
    private bool isPushingCard;
    private bool DeckOnTop;
    private bool isDrawing; // game state
    private bool waitingForInput;
    #endregion

    #region Properties
    public float DeckHeight { get => currentMoving != null ? currentMoving.Prev != null ? currentMoving.Prev.Position.y
                                                           : CS.Origin.y                :CS.Origin.y; } 
    #endregion 

    #region MonoBehaviour
    void Start()
    {
        InitVariables();
    }

    private void Update()
    {
        if (isDefusing) { OnDefuse(); }
    }

    #endregion

    #region Defuse

    /// <summary>
    /// Coordinates and manage the defuse sequence 
    /// </summary>
    private void OnDefuse() 
    {
        if (bomb != null)
        {
            // if bomb not near the deck return.
            if (!BombNearDeck()) { return; }
            // if deck is empty just plant bomb
            if (Tail == null && Head == null) {
                toNextForBomb = null;
                CheckBombPlanted();
                return;
            }
            // if bomb below deck, move all card up and return.
            if (BombBelowDeck()) {
                toNextForBomb = Tail;
            }
            // if bomb is above Ceiling, move all cards down and return
            if (BombAboveCeiling()) { 
                toNextForBomb = null;
            }
            // if the deck is on top, return to avoid issues with moving cards unnecessarily 
            if (DeckOnTop) { 
                CheckBombPlanted();
                return; 
            }
            // set up next card to move if null or head
            if (currentMoving == null || currentMoving == Head) { currentMoving = Head.Prev; }
            // move cards as needed according to where the bomb is
            MoveCardsByBomb();
            // check if defuse is done and reset deck and exit, pass turn 
            CheckBombPlanted();
            return;
        }
    }

    private void CheckBombPlanted()
    {
        if (isBombPlanted)
        {
            // getting the center to remove bomb and add to deck at the correct spot.
            // not using ChangeHome because we want to add at the correct spot and not as head.
            CardContainer _center = SC_GameData.Instance.GetContainer(Containers.Center);
            if (_center == null)
            {
                Debug.LogError("Failed to Check Bomb Planted! can't get bomb, center is null.");
                return;
            }
            isDefusing = false;
            _center.Remove(bomb);
            InsertBefore(bomb, toNextForBomb);
            // Debug.Log($"bomb planted before: {toNextForBomb}({toNextForBomb.Index}) at index: {bomb.Index}");
            SetNodeBasedOnPrev(bomb.Prev != null ? bomb.Prev : bomb);
            PropagateUpdatesToHead(bomb.Prev != null ? bomb.Prev : bomb);
            Debug.Log("change state to end turn");
            SC_GameLogic.Instance.ChangeState(GameStates.MyEndTurn);
            isBombPlanted = false;
        }
    }

    /// <summary>
    /// Check if the bomb is too far from the deck. 
    /// <para></para>
    /// Rotate the bomb as needed.
    /// </summary>
    /// <returns>True if bomb is near deck, else false</returns>
    public bool BombNearDeck()
    {
        float BombDistanceToDeck = MathF.Abs(bomb.Position.x - CS.Origin.x);
        if (BombDistanceToDeck > bombInsertRange)
        {
            // reset deck
            ResetContainer();   
            // Reset bomb's rotation and properties
            if (bomb.TargetRotation != Vector3.zero)
            {
                bomb.TargetRotation = Vector3.zero;
                bomb.IsFaceUp = true;
                bomb.FixPerspective = true;
            }
            return false;
        }
        // rotate bomb for insert
        if (bomb.TargetRotation != CS.originRotation)
        {
            bomb.TargetRotation = CS.originRotation;
            bomb.IsFaceUp = CS.KeepCardsFaceUp;
            bomb.FixPerspective = CS.FixPerspective;
        }
        return true;
    }

    /// <summary>
    /// If the bomb is below the deck's Origin, move all cards to Ceiling.
    /// </summary>
    /// <returns>True if bomb is below deck.</returns>
    private bool BombBelowDeck()
    {
        if (bomb.Position.y < CS.Origin.y && Tail.TargetPosition != CS.Ceiling)
        {
            DeckOnTop = true;
            Tail.TargetPosition = CS.Ceiling;
            PropagateUpdatesToHead(Tail);
            bomb.SortOrder = Tail.SortOrder - 1;
        }
        return DeckOnTop;
    }

    /// <summary>
    /// If the bomb is above the deck's Ceiling, move all cards to Origin
    /// </summary>
    /// <returns>True if bomb is above Ceiling</returns>
    private bool BombAboveCeiling()
    {
        if (Head == null) { 
            Debug.LogError("Failed to deteremine deck ceiling! head is null.");
            return false;
        }
        if (Head.Position.y < bomb.Position.y)
        {
            DeckOnTop = false;
            bomb.SortOrder = Head.SortOrder + 1;
            Tail.TargetPosition = CS.Origin;
            PropagateUpdatesToHead(Tail);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Moves the cards near the bomb to make it possible to insert. <para>
    /// </para>
    /// Uses <see cref="PushCardDown"/> and <see cref="PushCardUp"/>
    /// </summary>
    private void MoveCardsByBomb()
    {
        // If the bomb is within the middleRange region of the deck, push cards up or down 
        if (CS.Origin.y < bomb.Position.y && bomb.Position.y < DeckHeight)
        {
            // entered push up zone 
            if (currentMoving == null || currentMoving.Prev == null) {
                Debug.LogError($"NULL!");
            }

            bomb.SortOrder = currentMoving.Prev.SortOrder - 1;
            toNextForBomb = currentMoving.Prev;
            Debug.Log($"entered push UP zone <color=green>head: {head}, curr: {currentMoving}, " +
                $"prev: {currentMoving.Prev}, toNext: {toNextForBomb}</color> ");

            if (!isPushingCard)
            {
                isPushingCard = true;
                StartCoroutine(PushCardUp());
            }
        }
        else if (DeckHeight  < bomb.Position.y && bomb.Position.y < DeckHeight + middleRange)
        {
            // entered wait zone
            if (currentMoving == null) {
                Debug.LogError($"NULL!");
                return;
            }
            bomb.SortOrder = currentMoving.SortOrder - 1;
            toNextForBomb = currentMoving;
            Debug.Log($"entered WAIT zone <color=green>head: {head}, curr: {currentMoving}, " +
                $"prev: {currentMoving.Prev}, toNext: {toNextForBomb}</color> ");
            // if deck not open yet, open deck
            if (!isPushingCard && currentMoving.TargetPosition == new Vector2(currentMoving.Position.x, DeckHeight + middleRange))
            {
                isPushingCard = true;
                StartCoroutine(PushCardUp());
            }
        }
        else if (DeckHeight + middleRange < bomb.Position.y && bomb.Position.y < DeckHeight + 2*middleRange)
        {
            // entered push down zone
            if (currentMoving == null || currentMoving.Next == null) {
                Debug.LogError($"NULL!");
            }

            bomb.SortOrder = currentMoving.Next.SortOrder - 1;
            toNextForBomb = currentMoving.Next;
            Debug.Log($"entered push DOWN zone <color=green>head: {head}, curr: {currentMoving}, " +
                $"next: {currentMoving.Next}, toNext: {toNextForBomb}</color> ");

            if (!isPushingCard)
            {
                isPushingCard = true;
                StartCoroutine(PushCardDown());
            }
        }
    }

    #endregion

    #region SetUp

    public override void InitVariables()
    {
        head = null;
        tail = null;

        containerSettings = new()
        {
            Container = Containers.Deck,
            KeepCardsFaceUp = false,
            FixPerspective = false,
            stackDirection = new(0, 1),
            OnHoverOffset = Vector2.up/3,

            Ceiling = new(-3, 3),
            Origin = new(-3, -1),
            offsetDistance = 0.06f,

            originRotation = new(50, 0, 15),
            offsetRotation = Vector3.zero,

            originSortOrder = 0,
            offsetSortOrder = 2
        };

        seePosition = new(0,0);
        seeOffset = new(1,0);
        numberOfDefuses = 6;
        isBombPlanted = false;
        bombInsertRange = 2;
        isDefusing = false;
        middleRange = 1.5f;
        isPushingCard = false;
        deckScrollSpeed = 1f;
    }

    /// <summary>
    /// Creates cards and adds them to the deck. <para></para>
    /// Exlodes Exploding and Defuses.
    /// </summary>
    public void PopulateDeck()
    {
        if (Tail != null) { Clear(); }
        List<string> cardNames = SC_GameData.Instance.CardNames;
        if (cardNames == null)
        {
            Debug.LogError("Failed to Create SC_Deck! cards names is null");
            return;
        }
        foreach (string _name in cardNames)
        {
            if (_name == null) { 
                Debug.LogError("Failed to Populate Deck! card name is null."); 
                return; 
            }

            // skips exploding and defuses, will be added after dealing the cards
            if (_name.Contains("exploding") || _name.Contains("defuse")) { continue; }

            SC_Card _card = SC_Card.CreateCard(_name);
            if (_card == null)
            {
                Debug.LogError("Failed to Create Card!");
                return;
            }
            InsertBefore(_card, Tail);
        }
    }

    public void DealCards(int startingHandSize, int numberOfPlayers)
    {
        Containers[] players = { Containers.PlayerHand, Containers.OpponentHand1, Containers.OpponentHand2, Containers.OpponentHand3, Containers.OpponentHand4, };
        int i = 0;
        while (i < startingHandSize * numberOfPlayers && Tail != null && Head != null)
        {
            // first card is dealt from the bottom to garennte each player starts with a defuse.
            SC_Card card = i < numberOfPlayers ? Tail : Head;
            card.ChangeHome(players[i % numberOfPlayers]);
            i++;
        }
    }

    public void AddDefuses()
    {

        for (int i = 1; i <= numberOfDefuses; i++)
        {
            SC_Card _card = SC_Card.CreateCard("Card_defuse_" + i);
            if (_card == null)
            {
                Debug.LogError("Failed to Create Card!");
                return;
            }
            InsertBefore(_card, Tail);
        }
    }

    public void AddExploding(int numberOfPlayers)
    {
        if (numberOfPlayers <= 0 || 5 < numberOfPlayers)
        {
            Debug.LogError(@$"Failed to Add Exploding and Defuses! 
                              number of players is invalid, {numberOfPlayers}");
            return;
        }

        int numberOfExploding = numberOfPlayers - 1;

        for (int i = 1; i <= numberOfExploding; i++)
        {
            SC_Card _card = SC_Card.CreateCard("Card_exploding_" + i);
            if (_card == null)
            {
                Debug.LogError("Failed to Create Card!");
                return;
            }
            InsertBefore(_card, Tail);
        }
    }

    #endregion

    #region Events

    /// <summary>
    /// Used when the card see the future is played
    /// </summary>
    public void SeeTheFuture(int n)
    {
        StartCoroutine(MoveCardsOneByOne(n));
        StartCoroutine(WaitForPlayerInput());
    }

    #endregion

    #region IEnumerators

    private IEnumerator WaitForPlayerInput()
    {
        Debug.Log("Waiting for player input...");

        waitingForInput = true;

        while (waitingForInput)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.anyKeyDown)
            {
                waitingForInput = false;
            }
            yield return null;
        }
        while (seeTheFutureTemp != null)
        {
            seeTheFutureTemp.IsFaceUp = false;
            SetNodeBasedOnPrev(seeTheFutureTemp);
            seeTheFutureTemp = seeTheFutureTemp.Next;
        }
    }

    /// <summary>
    /// going over the current n cards and showing them to the player
    /// </summary>
    private IEnumerator MoveCardsOneByOne(int n)
    {
        seeTheFutureTemp = Head;

        for (int i = 0; i < n; i++)
        {
            seeTheFutureTemp.TargetPosition = seePosition + i * seeOffset;
            seeTheFutureTemp.TargetRotation = Vector3.zero;
            seeTheFutureTemp.SortOrder = CardView.frontSortingOrder + i;
            seeTheFutureTemp.IsFaceUp = true;
            seeTheFutureTemp = seeTheFutureTemp.Prev;

            // Wait for a short duration before moving the next card
            yield return new WaitForSeconds(seeTheFutureDelay); 
        }
    }

    /// <summary>
    /// Used when moving cards to make room for bomb to be planted
    /// </summary>
    private IEnumerator PushCardUp()
    {
        if (DeckOnTop || currentMoving == null)
        {
            ResetContainer();
        }
        else
        {
            Debug.Log($"pushing up <color=green>{currentMoving.Index}</color> ");

            // Push current up
            currentMoving.TargetPosition = new Vector2 (currentMoving.Position.x, DeckHeight + middleRange);

            // Push next up
            if (currentMoving.Next != null)
            {
                currentMoving.Next.TargetPosition = new Vector2(currentMoving.Next.Position.x, DeckHeight + 2*middleRange);
            }
            // Push the rest of the cards after next
            PropagateUpdatesToHead(currentMoving.Next);
            currentMoving = currentMoving.Prev;
        }

        yield return new WaitForSeconds(1 / deckScrollSpeed);
        isPushingCard = false;
    }

    /// <summary>
    /// Used when moving cards to make room for bomb to be planted
    /// </summary>
    private IEnumerator PushCardDown()
    {
        if (DeckOnTop || currentMoving == null || currentMoving == Head)
        {
            ResetContainer();
        }
        else
        {
            Debug.Log($"pushing down <color=green>{currentMoving.Index}</color> ");

            // Push current down
            SetNodeBasedOnPrev(currentMoving);

            // Push next down
            if (currentMoving.Next != null)
            {
                currentMoving.Next.TargetPosition = new Vector2(currentMoving.Next.Position.x, 
                                                                currentMoving.TargetPosition.y + middleRange);
            }
            currentMoving = currentMoving.Next;
        }

        yield return new WaitForSeconds(1 / deckScrollSpeed);
        isPushingCard = false;
    }

    #endregion

        #region Methods

    public override void ResetContainer()
    {
        base.ResetContainer();
        currentMoving = null;   
    }

    #endregion

}
