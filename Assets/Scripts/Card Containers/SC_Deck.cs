using System;
using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
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

    [Header("Defuse")]
    [SerializeField]
    private SC_Card bomb;
    [SerializeField]
    private float bombInsertRange;
    [SerializeField]
    private SC_Card currentMoving;
    [SerializeField]
    private float deckScrollSpeed;
    [SerializeField]
    private float middleRange;
    [SerializeField]
    public bool isDefusing;
    [SerializeField]
    private bool isPushingCard;
    private bool DeckOnTop;
    private float epsilon;

    #endregion

    #region MonoBehaviour

    void Start()
    {
        InitVariables();
        PopulateDeck();
        AddExplodingDefuses(2); // number of players 
        Shuffle();
        SetNodeBasedOnPrev(Tail);
        PropagateUpdatesToHead(Tail);
    }

    private void Update()
    {

        if (!isDefusing || bomb == null || Tail == null) { return; }
        // Debug.Log($"Bomb to head distance: <color=red>{bomb.Position.y - head.Position.y}</color>");
        // Debug.Log($"Bomb y position: ");
        // Check if the bomb is too far from the deck
        float BombDistanceToDeck = MathF.Abs(bomb.Position.x - CS.floor.x);
        if (BombDistanceToDeck > bombInsertRange)
        {
            // Reset bomb's rotation and properties
            if (bomb.TargetRotation != Vector3.zero)
            {
                bomb.TargetRotation = Vector3.zero;
                bomb.IsFaceUp = true;
                bomb.FixPerspective = true;
            }
            return;
        }
        // rotate bomb for insert
        if (bomb.TargetRotation != CS.originRotation)
        { 
            bomb.TargetRotation = CS.originRotation;
            bomb.IsFaceUp = CS.KeepCardsFaceUp;
            bomb.FixPerspective = CS.FixPerspective;
        }

        // If the bomb is below the deck's floor, move all cards to ceiling 
        if (bomb.Position.y < CS.floor.y && Tail.TargetPosition != CS.ceiling)
        {
            Debug.Log($"entered bottom zone {bomb.Position.y}");
            DeckOnTop = true;
            Tail.TargetPosition = CS.ceiling;
            PropagateUpdatesToHead(Tail);
            bomb.SortOrder = Tail.SortOrder - 1;
        }

        // If the bomb is above the deck's ceiling, move all cards to floor
        else if (CS.ceiling.y < bomb.Position.y)
        {
            Debug.Log($"entered upper zone {bomb.Position.y}");
            DeckOnTop = false;
            bomb.SortOrder = Head.SortOrder + 1;
            Tail.TargetPosition = CS.floor;
            PropagateUpdatesToHead(Tail);
        }

        else if (DeckOnTop) { return; }

        else if (currentMoving == null || currentMoving == Head) { currentMoving = Head.Prev; }

        // If the bomb is within the middleRange region of the deck, push cards up or down 
        else if (CS.floor.y < bomb.Position.y && bomb.Position.y < CS.ceiling.y - 3*middleRange)
        {
            Debug.Log($"entered push up zone <color=green>{currentMoving.Index}</color> ");
            if (!isPushingCard) {
                isPushingCard = true; 
                StartCoroutine(PushCardUp()); 
            }
            bomb.SortOrder = currentMoving.SortOrder - 1;
        }
        else if (CS.ceiling.y - 3*middleRange < bomb.Position.y && bomb.Position.y < CS.ceiling.y - 2*middleRange)
        {
            Debug.Log($"entered wait zone (DOWN) <color=red>{currentMoving.Index}</color>");
            bomb.SortOrder = currentMoving.SortOrder;
        }
        else if (CS.ceiling.y - 2 * middleRange < bomb.Position.y && bomb.Position.y < CS.ceiling.y - middleRange)
        {
            Debug.Log($"entered wait zone (UP) <color=red>{currentMoving.Index}</color>");
            bomb.SortOrder = currentMoving.SortOrder + 1;
        }
        else if (CS.ceiling.y - middleRange < bomb.Position.y && bomb.Position.y < CS.ceiling.y)
        {
            Debug.Log($"entered push down zone <color=green>{currentMoving.Index}</color>");
            if (!isPushingCard) { 
                isPushingCard = true; 
                StartCoroutine(PushCardDown()); 
            }
            bomb.SortOrder = currentMoving.SortOrder + 1;
        }

        // check if defuse is done and reset and exit, pass turn
    }

    private IEnumerator PushCardUp()
    {
        if (DeckOnTop || currentMoving == null)
        {
            ResetContainer();
        }
        else
        {
            Debug.Log($"pushing up <color=green>{currentMoving.Index}</color> ");

            // Push Prev up
            if (currentMoving.Prev != null)
            {
                currentMoving.Prev.TargetPosition = CS.ceiling - (2 * middleRange * StackDirection);
            }
            // Push current up
            currentMoving.TargetPosition = CS.ceiling - (middleRange * StackDirection);

            // Push next up
            if (currentMoving.Next != null)
            {
                currentMoving.Next.TargetPosition = CS.ceiling;
            }
            // Push the rest of the cards after next
            PropagateUpdatesToHead(currentMoving.Next);
            currentMoving = currentMoving.Prev;
        }

        yield return new WaitForSeconds(1/deckScrollSpeed);
        isPushingCard = false;
    }

    private IEnumerator PushCardDown()
    {
        if (DeckOnTop || currentMoving == null || currentMoving == Head)
        { 
            ResetContainer(); 
        }
        else
        {
            Debug.Log($"pushing down <color=green>{currentMoving.Index}</color> ");
            // Set prev back on top of the deck
            if (currentMoving.Prev != null)
            {
                SetNodeBasedOnPrev(currentMoving.Prev);
            }

            // Push current down
            currentMoving.TargetPosition = CS.ceiling - (2 * middleRange * StackDirection);

            // Push next down
            if (currentMoving.Next != null)
            {
                currentMoving.Next.TargetPosition = CS.ceiling - (middleRange * StackDirection);
            }
            currentMoving = currentMoving.Next;
        }

        yield return new WaitForSeconds(1 / deckScrollSpeed);
        isPushingCard = false;
    }

    #endregion

    #region Logic

    public float NodeDistance(SC_Card A, SC_Card B)
    {
        return ((A.Position - B.Position) * StackDirection).magnitude;
    }

    private void InitVariables()
    {
        head = null;
        tail = null;

        containerSettings = new()
        {
            Container = Containers.Deck,
            KeepCardsFaceUp = false,
            FixPerspective = false,
            stackDirection = new(0, 1),

            ceiling = new(-1, 3),
            floor = new(-1, -1),
            offsetDistance = 0.02f,

            originRotation = new(50, 0, 15),
            offsetRotation = Vector3.zero,

            originSortOrder = 0,
            offsetSortOrder = 2
    };

        seePosition = new(0,0);
        seeOffset = new(1,0);

        bombInsertRange = 2;
        isDefusing = false;
        middleRange = 1f;
        isPushingCard = false;
        deckScrollSpeed = 1f;
        epsilon = 0.02f;
    }

    /// <summary>
    /// shuffle the deck using the Fisher-Yates Shuffle algorithm
    /// </summary>
    private void Shuffle()
    {
        if (Tail == null || Head == null)
        {
            Debug.LogError("Failed to Shuffle! list is empty? tail or head null");
        }
        SC_Card current, random, next;
        int i = 0;
        current = Head;
        while (current != null && i<MaxCapacitiy)
        {
            next = current.Prev;

            // get a random card from the list
            random = GetRandomCard(count - 1 - i);
            if (random == null) { Debug.LogError("Failed to shuffle, got null on random"); return; }
            // swap current and random cards 
            current.Swap(random);
            // update tail and head if needed
            if (current.Prev == null) { Tail = current; }
            if (current.Next == null) { Head = current; }
            if (random.Prev == null) { Tail = random; }
            if (random.Next == null) { Head = random; }
            // move on to next card
            current = next;
            i++;
        }
    }

    /// <summary>
    /// Used when the card see the future is played
    /// </summary>
    private void SeeTheFuture(int n)
    {
        // going over the current n cards and showing them to the player
        SC_Card temp = Head;
        for (int i = 0; i < n; i++)
        {
            temp.TargetPosition = seePosition + i * seeOffset;
            temp.TargetRotation = Vector3.zero;
            temp.SortOrder = CardView.frontSortingOrder + i;
            temp.IsFaceUp = true;
            temp = temp.Prev;
        }
        // wait for player input
        // SetNodeBasedOnPrev(temp);
        // PropagateUpdates(temp);
    }

    /// <summary>
    /// Creates cards and adds them to the deck. <para></para>
    /// Exlodes Exploding and Defuses.
    /// </summary>
    private void PopulateDeck()
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

    private void AddExplodingDefuses(int numberOfPlayers)
    {
        if (numberOfPlayers <= 0 || 5 < numberOfPlayers)
        {
            Debug.LogError(@$"Failed to Add Exploding and Defuses! 
                              number of players is invalid, {numberOfPlayers}");
            return;
        }

        int numberOfDefuses = 6 - numberOfPlayers;  
        int numberOfExploding = numberOfPlayers - 1;

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
    /// Handles a card entering the linked list. <para>
    /// Cards added to the end, with tail being at origin position. </para>
    /// </summary>
    public void OnCardEnter(SC_Card node)
    {
        if (node == null) { return; }
        InsertBefore(node, Tail);
        PropagateUpdatesToHead(node);
    }

    public void OnCardExit(SC_Card node)
    {
        if (node == null || Head == null || Tail == null) { return; }
        else { Remove(node, false); }
    }

    private void OnDefuseStart()
    {
        isDefusing = true;
        // Getting bomb from center
        CardContainer _center = SC_GameData.Instance.GetContainer(Containers.Center);
        if (_center == null)
        {
            Debug.LogError("Failed to defuse! can't get bomb, center is null.");
            return;
        }
        SC_Card bomb = _center.Head; 
        if (bomb == null)
        {
            Debug.LogError($"Failed to defuse! bomb is null, center is empty: {_center}");
            return;
        }
        Head.TargetPosition = CS.ceiling;
        SC_Card current = Head.Prev;
        int i = 0;
        while (current != null && i < 3)
        {
            CS.offsetDistance = middleRange;
            SetNodeBasedOnNext(current); 
            current = current.Prev;
            i++;
        }

    }

    #endregion
}
