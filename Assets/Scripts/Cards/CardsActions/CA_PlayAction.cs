using System;
using UnityEngine;
/// <summary>
/// super class used for all action cards played from the hand <para></para>
/// Starts drag and moves to center
/// </summary>
[Serializable]
public class PlayAction : CardAction // abstract
{

    #region Constractor

    public PlayAction(SC_Card _card) : base(_card)
    {
        // bomb cards are not playable like that
        if (card.Type == CardTypes.Exploding) { return; }
        Debug.Log($"<color=blue>Assgined {_card} with Play Card Action</color>");
        onClickDown += StartDrag;
        onClickDown += MoveToCenter;
        onClickUp += EndDrag;
        onClickUp += ChangeStateToMyTakeAction;
    }

    #endregion

}
