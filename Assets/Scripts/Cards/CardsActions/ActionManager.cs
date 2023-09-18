using System;
using UnityEngine;
/// <summary>
/// Used in a card to set if it's playable and update it's action when clicked or dropped
/// </summary>
[Serializable]
public class ActionManager
{
    #region IDEA
    // invoke action:
    // not playable, no action -> do nothing
    // is head of deck, action draw -> call OnStartDraw event (depends on type)
    // in player hand, action play -> call OnPlay event (depends on type)
    // in discards, action take back -> Insert like in OnStartDraw
    // in others hands, steal, Insert to player like in OnStartDraw
    // in center, also insert to hand.

    // from player hand insert to others hand on favor, deck on defuse, center on play 
    #endregion

    #region Fields

    [Header("Globals")]
    [Header("Number of cards needed")]
    [SerializeField]
    private int ForRandomSteal;
    [SerializeField]
    private int ForSpecificSteal;
    [SerializeField]
    private int ForRetrieveDiscarded;

    private SC_Card card;

    #endregion

    #region Constractor

    public ActionManager(SC_Card _card)
    {
        card = _card;
        Subscribe();
        ForRandomSteal = 2;
        ForSpecificSteal = 3;
        ForRetrieveDiscarded = 5;
}

    ~ActionManager()
    {
        Unsubscribe();
    }

    #endregion

    #region Properties

    private Containers Home { get => card != null ? card.Home : Containers.error; }
    private void SetAction(CardAction _action) 
    { 
        if (card == null) { 
            Debug.LogError("Failed to Set Card Action! card is null. problem with action manager constractor in card initializtion."); 
            return; 
        }
        card.action = _action;
    } 
    private GameStates CurrentState { get => SC_GameLogic.Instance.currentState; } // get the current game state from state machine or game logic 
    private CardContainer CardsInPlay { get => SC_GameData.Instance.GetContainer(Containers.Center); }

    #endregion

    #region Events

    private void Subscribe()
    {
        SC_GameLogic.OnStateTransition += OnStateTransition;
        SetUpShuffle.SetShuffleAction += SetShuffleAction;
        SetUpSeeTheFuture.SetSeeTheFutureAction += SetSeeTheFutureAction;
        SetUpFavor.SetFavorAction += SetFavorAction;
        SetUpRandomSteal.SetRandomStealAction += SetRandomStealAction;
        SetUpGive.SetGiveAction += SetGiveAction;
    }
    private void Unsubscribe()
    {
        SC_GameLogic.OnStateTransition -= OnStateTransition;
        SetUpShuffle.SetShuffleAction -= SetShuffleAction;
        SetUpSeeTheFuture.SetSeeTheFutureAction -= SetSeeTheFutureAction;
        SetUpFavor.SetFavorAction -= SetFavorAction;
        SetUpRandomSteal.SetRandomStealAction -= SetRandomStealAction;
        SetUpGive.SetGiveAction -= SetGiveAction;
    }

    private void OnStateTransition(GameStates newState)
    {
        // start with a clean plate 
        ResetCardAction();

        if (newState == GameStates.MyPlayOrDraw)
        {
            // in hand default play action
            if (Home == Containers.PlayerHand) {
                SetDefaultAction();
                return;
            }
            // in deck draw (for head)
            if (Home == Containers.Deck && CheckForDraw()) {
                card.action = new DrawCard(card);
                return;
            }
        }
        if (newState == GameStates.MyTakeAction)
        {
            // in hand check for combos
            if (Home == Containers.PlayerHand) {
                SetComboAction();
            }
            // in center take back (if last one go back to my play or draw)
            if (Home == Containers.Center) {
                card.action = new TakeCard(card);
            }
            // change state happens before action effect 
            // everywhere else set in action, shuffle -> deck cards shuffle, steal -> others hand steal.
        }
        if (newState == GameStates.MyDefuse)
        {
            // set defuse action for defuses in hand, set bomb action for the bomb in center. 
            // move defuse stright to discards to keep bomb alone in center
        }
        if (newState == GameStates.MyEndTurn)
        {
            // everything has no action, maybe have the option to hover? 
            // next turn button clickable
        }
        if (newState == GameStates.OthersTakeAction)
        {
            // can use nope
        }
        else
        {
            // at any other point in the game you cant do anything, its the other players turn
        }
    }

    #endregion

    #region Conditions Checks

    private bool CheckForDraw()
    {
        // must finish current action before drawing and ending turn
        if (CardsInPlay.count != 0) { return false; }

        SC_Deck _deck = SC_GameData.Instance.GetContainer(Containers.Deck) as SC_Deck;
        if (_deck == null)
        {
            Debug.LogError("Failed to set deck cards action! deck is null");
            return false;
        }

        if (_deck.Head == card)
        {
            return true;
        }
        else { return false; }
    }

    private bool CheckForRandomSteal()
    {
        return (CardsInPlay.count == ForRandomSteal - 1 &&
                CheckMatchingType());
    }

    private bool CheckForSpecificSteal()
    {
        return (CardsInPlay.count == ForSpecificSteal - 1 &&
                CheckMatchingType());
    }

    /// <summary>
    /// Goes over the cards in play to check if the are matching the current card type
    /// </summary>
    /// <returns>All cards match -> True. At least one card is different -> False.</returns>
    private bool CheckMatchingType()
    {
        SC_Card _tempCard = CardsInPlay.Head;
        for (int i = 0; i < CardsInPlay.count && _tempCard != null; i++)
        {
            if (_tempCard.Type != card.Type)
            {
                return false;
            }
            _tempCard = _tempCard.Prev;
        }
        return true;
    }

    private bool CheckForRetrieveDiscarded()
    {
        if (CardsInPlay.count != ForRetrieveDiscarded - 1) { return false; }
        // looking for different types, if same type card exit
        SC_Card _tempCard = CardsInPlay.Head;
        for (int i = 0; i < CardsInPlay.count && _tempCard != null; i++)
        {
            if (_tempCard.Prev != null && _tempCard.Type == _tempCard.Prev.Type || _tempCard.Type == card.Type)
            {
                return false;
            }
            _tempCard = _tempCard.Prev;
        }
        return true;
    }

    private bool CheckOpponentHand()
    {
        return (card.Home == Containers.OpponentHand1 ||
                card.Home == Containers.OpponentHand2 ||
                card.Home == Containers.OpponentHand3 ||
                card.Home == Containers.OpponentHand4);
    }

    #endregion

    #region Set Card Actions

    public void ResetCardAction()
    {
        if (card == null) { Debug.LogError("Failed to Reset Card Action! card is null."); return; }
        // Debug.Log($"Resetting card action for {card}");
        card.action = null; 
    }
    
    private void SetDefaultAction()
    {
        if (card == null) { Debug.LogError("Failed to Set Card Action! card is null."); return; }

        card.action = card.Type switch
        {
            CardTypes.Seethefuture => new SetUpSeeTheFuture(card),
            CardTypes.Shuffle => new SetUpShuffle(card),
            CardTypes.Favor => new SetUpFavor(card),
            CardTypes.Attack => new Attack(card),
            CardTypes.Skip => new Skip(card),
            _ => new PlayAction(card),// general cards dont have a default action but can be played as part of a combo
        };
    }

    private void SetComboAction()
    {
        // if cards in play match the condition, action set to start random steal
        if (CheckForRandomSteal())
        {
            card.action = new SetUpRandomSteal(card); 
        }

        // Checking if cards in play match the condition, action set to start specific steal
        else if (CheckForSpecificSteal())
        {
            card.action = new PlayAction(card); // StartSpecificSteal;
        }

        // Checking if cards in play match the condition, action set to start retrieving a discarded card
        else if (CheckForRetrieveDiscarded())
        {
            card.action = new PlayAction(card); // StartRetrieveDiscarded;
        }
    }

    private void SetGiveAction()
    {
        if (Home == Containers.PlayerHand)
        {
            card.action = new GiveCard(card);
        }
    }

    private void SetShuffleAction()
    {
        if (Home == Containers.Deck)
        {
            card.action = new Shuffle(card);
        }
    }
    private void SetSeeTheFutureAction()
    {
        if (Home == Containers.Deck)
        {
            card.action = new SeeTheFuture(card);
        }
    }

    private void SetFavorAction()
    {
        if (CheckOpponentHand())
        {
            card.action = new Favor(card);
        }
    }

    private void SetRandomStealAction()
    {
        if (CheckOpponentHand())
        {
            card.action = new TakeCard(card);
        }
    }

    private void SetSpecificStealAction()
    {

    }

    private void SetRetrieveDiscardedAction()
    {

    }

    private void SetNopeAction() { }

    #endregion
}
