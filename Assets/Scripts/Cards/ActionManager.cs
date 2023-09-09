using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
/// <summary>
/// Used in a card to set if it's playable and update it's effect when clicked or dropped
/// </summary>
/// 
/*
public class ActionManager
{
    // Fields
    List<CardTypes> cardsInPlay = new();
    private Containers currContainer;

    #region Events
    private void Subscribe()
    {
        SC_Card.OnClickDownCard += OnClickDownCard;
        SC_Card.OnClickUpCard += OnClickUpCard;   
    }
    private void UnSubscribe()
    {
        SC_Card.OnClickDownCard -= OnClickDownCard;
        SC_Card.OnClickUpCard -= OnClickUpCard;
    }
    private void OnClickDownCard()
    {
        if (currContainer == Containers.error || cardsInPlay == null) {
            Debug.LogError("OnClickDown Failed on " + this + "!");
            return; 
        }
        if (CurrContainer == Containers.PlayerHand)
        {
            OnClickDownHand();
        }
        if (CurrContainer == Containers.SC_Deck)
        {
            OnClickDownDeck();
        }
        if (CurrContainer == Containers.Discards)
        {
            OnClickDownDiscards();
        }
        else // CurrContainer == Any Opponent Hand
        {
            OnClickDownOpponent();
        }
    }

    private void OnClickUpCard()
    {
        
    }

    #endregion

    #region Logic
    private void OnClickDownHand()
    {
        if (PossibleActions.Play)
        {
            cardsInPlay.Add(Type);
            // IsInPlay = true; view.isDragged = true;

            if (cardsInPlay.Count == 0)
            {
                // activate card 
                return;
            }
            if (cardsInPlay.All(c => c == Type) && cardsInPlay.Count < 3)
            {
                if (cardsInPlay.Count == 2) {
                    // activate two of a kind
                }
                view.isDragged = true;
                return;
            }
            if (cardsInPlay.All(c => c != Type) && cardsInPlay.Count < 5)
            {
                // activate 5 different
                view.isDragged = true;
                return;
            }
        }
    }

    private void OnClickDownDeck()
    {
        if (PossibleActions.Draw)
        {
            // IsInDraw = true; // view.isDragged = true;
        }
    }

    private void OnClickDownOpponent()
    {
        // throw new NotImplementedException();
    }

    private void OnClickDownDiscards()
    {
        // throw new NotImplementedException();
    }
    #endregion

    ~ActionManager()
    {
        UnSubscribe();
    }
}
*/