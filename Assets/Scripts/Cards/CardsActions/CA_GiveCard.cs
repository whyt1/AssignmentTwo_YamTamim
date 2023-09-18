using System;
using UnityEngine;

/// <summary>
/// Used when cards are taken from other containers to the player hand
/// </summary>
public class GiveCard : CardAction
{

    #region Constractor

    public GiveCard(SC_Card _card) : base(_card)
    {
        // bomb cards are not playable like that
        if (card.Type == CardTypes.Exploding) { return; }
        onClickDown += StartDrag;
        onClickUp += EndGive;
        onClickUp += EndDrag;
    }

    #endregion

    #region Actions

    private void EndGive()
    {
        SC_OpponentHand1 opponentHand = SC_GameData.Instance.GetContainer(SC_GameLogic.Instance.currentPlayer) as SC_OpponentHand1;
        if (opponentHand == null)
        {
            Debug.LogError("Failed to take card! opponent Hand is null");
            return;
        }
        // Give end successful
        if (Mathf.Abs(opponentHand.CS.Origin.y - card.Position.y) < opponentHand.takeRange)
        {
            card.ChangeHome(SC_GameLogic.Instance.currentPlayer);
            SC_GameLogic.Instance.isGiveDone = true;
        }
        // Give cancel
        else
        {
            // nothing happens
        }
    }

    #endregion

}
