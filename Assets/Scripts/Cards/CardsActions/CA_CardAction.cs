using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class CardAction
{

    #region Fields

    [Header("Card Current Active Effect")]
    [SerializeField]
    protected string Action;

    protected SC_Card card;
    public UnityAction onClickDown;
    public UnityAction onClickUp;

    // public bool IsActive { get => onClickDown != null; } move to view

    #endregion

    #region Constractor

    public CardAction(SC_Card _card)
    {
        if (_card == null)
        {
            Debug.LogError("Failed to Constract card action! card is null.");
            return;
        }
        card = _card;
        Action = this.GetType().Name;
    }

    #endregion

    #region Actions

    #region Drag

    protected virtual void StartDrag()
    {

        Debug.Log($"started dragging {card}");
        card.IsDragged = true;
    }
    protected virtual void EndDrag()
    {
        Debug.Log($"finished dragging {card}");
        card.IsDragged = false;
    }

    #endregion

    #region Change Home

    protected virtual void MoveToCenter()
    {
        Debug.Log($"moved {card} to Containers.Center");
        card.ChangeHome(Containers.Center);
    }
    protected virtual void MoveToDeck()
    {
        Debug.Log($"moved {card} to Containers.Deck");
        card.ChangeHome(Containers.Deck);
    }
    protected virtual void MoveToPlayerHand()
    {
        Debug.Log($"moved {card} to Containers.PlayerHand");
        card.ChangeHome(Containers.PlayerHand);
    }
    protected virtual void MoveToOpponentHand()
    {
        // card.ChangeHome(CurrentTurn); 
        // figure out whos turn it is
    }
    protected virtual void MoveToDiscards()
    {
        Debug.Log($"moved {card} to Containers.Discards");
        card.ChangeHome(Containers.Discards);
    }

    #endregion

    #region Change State

    protected virtual void ChangeStateToMyTakeAction()
    {
        SC_GameLogic.Instance.ChangeState(GameStates.MyTakeAction);
    }
    protected virtual void ChangeStateToMyPlayOrDraw()
    {
        SC_GameLogic.Instance.ChangeState(GameStates.MyPlayOrDraw);
    }
    protected virtual void ChangeStateToMyEndTurn()
    {
        SC_GameLogic.Instance.ChangeState(GameStates.MyEndTurn);
    }


    #endregion

    #region Change Button
    public delegate void ChangeButton(UnityAction clickOnAction = null, string newText = null);
    public static event ChangeButton OnChangeButton;

    protected virtual void ChangeButtonAction(UnityAction clickOnAction)
    {
        if (OnChangeButton != null) { OnChangeButton(clickOnAction, null); }
    }
    protected virtual void ChangeButtonText(string newText)
    {
        if (OnChangeButton != null) { OnChangeButton(null, newText); }
    }

    #endregion

    #endregion
}
