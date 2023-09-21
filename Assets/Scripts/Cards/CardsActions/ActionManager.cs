using System;
using UnityEngine;
/// <summary>
/// Used in a card to set if it's playable and update it's action when clicked or dropped
/// </summary>
[Serializable]
public class ActionManager : MonoBehaviour
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

    public SC_Card card;

    #endregion

    #region MonoBehaviour

    void OnEnable()
    {
        Subscribe();
        ForRandomSteal = 2;
        ForSpecificSteal = 3;
        ForRetrieveDiscarded = 5;
    }

    void OnDisable()
    {
        card = null;
        Unsubscribe();
    }

    #endregion

    #region Properties

    private CardTypes Type { get => card != null ? card.Type : CardTypes.error; }
    private Containers Home { get => card != null ? card.Home : Containers.error; }
    private void SetAction(CardAction _action) 
    { 
        if (card == null) { 
            Debug.LogError("Failed to Set Card Action! card is null. problem with action manager constractor in card initializtion."); 
            return; 
        }
        if (_action != null) { Debug.Log($"set action: <color=green>{_action}</color> \nfor card: {card}"); }
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
        SetUpRetrieveDiscarded.SetRetrieveDiscardedAction += SetRetrieveDiscardedAction;
        SetUpSpecificSteal.SetSpecificStealAction += SetSpecificStealAction;
    }
    private void Unsubscribe()
    {
        SC_GameLogic.OnStateTransition -= OnStateTransition;
        SetUpShuffle.SetShuffleAction -= SetShuffleAction;
        SetUpSeeTheFuture.SetSeeTheFutureAction -= SetSeeTheFutureAction;
        SetUpFavor.SetFavorAction -= SetFavorAction;
        SetUpRandomSteal.SetRandomStealAction -= SetRandomStealAction;
        SetUpGive.SetGiveAction -= SetGiveAction;
        SetUpRetrieveDiscarded.SetRetrieveDiscardedAction -= SetRetrieveDiscardedAction;
        SetUpSpecificSteal.SetSpecificStealAction += SetSpecificStealAction;
    }

    private void OnStateTransition(GameStates newState)
    {
        // start with a clean plate 
        SetAction(null);

        if (newState == GameStates.MyPlayOrDraw)
        {
            // in hand default play action
            if (Home == Containers.PlayerHand) {
                SetDefaultAction();
                return;
            }
            // in deck draw (for head)
            if (Home == Containers.Deck && CheckForDraw()) {
                SetAction(new DrawCard(card));
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
                SetAction(new TakeCard(card));
            }
            // change state happens before action effect 
            // everywhere else set in action, shuffle -> deck cards shuffle, steal -> others hand steal.
        }
        if (newState == GameStates.MyDefuse)
        {
            // all done in the actions
            // set defuse action for defuses in hand, set bomb action for the bomb in center. 
            // move defuse stright to discards to keep bomb alone in center
        }
        if (newState == GameStates.MyEndTurn)
        {
            // all done in the actions
            // everything has no action, maybe have the option to hover? 
            // next turn button clickable
        }
        if (newState == GameStates.OthersTakeAction)
        {
            /*
            if (CheckForNope())
            {
                SetAction(new Nope(card));
            }
            */
        }
        else
        {
            // at any other point in the game you cant do anything, its the other players turn
        }
    }

    #endregion

    #region Conditions Checks

    private bool CheckForNope()
    {
        return (Home == Containers.PlayerHand && Type == CardTypes.Nope);
    }

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

        return _deck.Head == card;
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
            if (_tempCard.Type != Type)
            {
                return false;
            }
            _tempCard = _tempCard.Prev;
        }
        return true;
    }

    /// <summary>
    /// Goes over the cards in play to check if the are different then current card type
    /// </summary>
    /// <returns>All cards different -> True. At least one card is matching -> False.</returns>
    private bool CheckDifferentType()
    {
        SC_Card _tempCard = CardsInPlay.Head;
        for (int i = 0; i < CardsInPlay.count && _tempCard != null; i++)
        {
            if (_tempCard.Prev != null && _tempCard.Type == _tempCard.Prev.Type || _tempCard.Type == Type)
            {
                return false;
            }
            _tempCard = _tempCard.Prev;
        }
        return true;
    }

    /*
    private bool CheckForRetrieveDiscarded()
    {
        return (CardsInPlay.count == ForRetrieveDiscarded - 1 &&
                CheckDifferentType());
    }
    */

    private bool CheckOpponentHand()
    {
        return (Home == Containers.OpponentHand1 ||
                Home == Containers.OpponentHand2 ||
                Home == Containers.OpponentHand3 ||
                Home == Containers.OpponentHand4);
    }

    #endregion

    #region Set Card Actions
    
    private void SetDefaultAction()
    {
        if (card == null) { Debug.LogError("Failed to Set Card Action! card is null."); return; }

        SetAction(Type switch
        {
            CardTypes.Seethefuture => new SetUpSeeTheFuture(card),
            CardTypes.Shuffle => new SetUpShuffle(card),
            CardTypes.Favor => new SetUpFavor(card),
            CardTypes.Attack => new Attack(card),
            CardTypes.Skip => new Skip(card),
            _ => new PlayAction(card),// general cards dont have a default action but can be played as part of a combo
        });
    }

    private void SetComboAction()
    {
        // if cards in play match the condition, action set to start random steal
        if (CheckForRandomSteal())
        {
            SetAction(new SetUpRandomSteal(card)); 
        }

        // Checking if cards in play match the condition, action set to start specific steal
        else if (CheckForSpecificSteal())
        {
            SetAction(new SetUpSpecificSteal(card));
        }

        // Can play cards of different type to stack up for combo
        else if (CheckDifferentType() && CardsInPlay.count < ForRetrieveDiscarded)
        {
            SetAction(new PlayAction(card));
            // Checking if cards in play match the condition, action set to start retrieving a discarded card
            if (CardsInPlay.count == ForRetrieveDiscarded - 1)
            {
                SetAction(new SetUpRetrieveDiscarded(card)); 
            }
        }
    }

    private void SetGiveAction()
    {
        if (Home == Containers.PlayerHand)
        {
            SetAction(new GiveCard(card));
        }
    }

    private void SetShuffleAction()
    {
        if (Home == Containers.Deck)
        {
            SetAction(new Shuffle(card));
        }
    }
    private void SetSeeTheFutureAction()
    {
        if (Home == Containers.Deck)
        {
            SetAction(new SeeTheFuture(card));
        }
    }

    private void SetFavorAction()
    {
        if (CheckOpponentHand())
        {
            SetAction(new Favor(card));
        }
    }

    private void SetRandomStealAction()
    {
        if (CheckOpponentHand())
        {
            SetAction(new TakeCard(card));
        }
    }

    private void SetSpecificStealAction()
    {
        if (CheckOpponentHand())
        {
            SetAction(new SpecificSteal(card));
        }
    }

    private void SetRetrieveDiscardedAction()
    {
        if (Home == Containers.Discards)
        {
            SetAction(new TakeCard(card));
        }
    }

    #endregion
}
