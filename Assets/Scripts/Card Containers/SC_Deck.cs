using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class SC_Deck : CardContainer
{
    #region Fields

    [Header("Card Effects")]
    [SerializeField]
    private Vector2 seeTheFuturePosition;
    [SerializeField]
    private Vector2 seeTheFutureOffset;

    #endregion

    #region MonoBehaviour

    void Start()
    {
        InitVariables();
        PopulateDeck();
        AddExplodingDefuses(2); // number of players 
        Shuffle();
        SetNodeBasedOnPrev(Tail);
        PropagateUpdates(Tail);
    }

    #endregion

    #region Logic

    private void InitVariables()
    {
        head = null;
        tail = null;

        Container = Containers.Deck;
        KeepCardsFaceUp = false;
        FixPerspective = false;
        stackDirection = new(0,1);

        originPosition = new(-1,0);
        offsetDistance = 0.02f;

        originRotation = new(50, 0, 345);
        offsetRotation = Vector3.zero;

        originSortOrder = 0;
        offsetSortOrder = 1;

        seeTheFuturePosition = new(0,0);
        seeTheFutureOffset = new(1,0);  
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
        SC_Card first, random;
        first = Tail;
        for (int i = 0; i < count - 1; i++)
        {
            // get a random card from the list
            random = Tail;
            int randomIndex = Random.Range(1, count - i);
            for (int j = 0; j < randomIndex; j++) 
            {
                random = random.Next;
                if (random == null) {
                    Debug.LogError("random reached null during shuffle");
                    return; 
                }
            }
            // swap first and random cards 
            first.Swap(random);
            // update tail and head if needed
            if (first.Next == null) { Head = first; }
            if (random.Prev == null) { Tail = random; }
            // move on to next card
            first = first.Next;
        }
    }

    /// <summary>
    /// Used when the card see the future is played
    /// </summary>
    private void SeeTheFuture(int n)
    {
        // going over the first n cards and showing them to the player
        SC_Card temp = Head;
        for (int i = 0; i < n; i++)
        {
            temp.TargetPosition = seeTheFuturePosition + i * seeTheFutureOffset;
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
        foreach (string _name in cardNames.GetRange(0, 6))
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
        PropagateUpdates(node);
    }

    public void OnCardExit(SC_Card node)
    {
        if (node == null || Head == null || Tail == null) { return; }
        else { Remove(node, false); }
    }

    private void OnDefuse()
    {

    }

    #endregion
}
