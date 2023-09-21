using System;
using UnityEngine;

/// <summary>
/// Used when cards are taken from other containers to the player hand
/// </summary>
public class TakeCard : CardAction
{

    #region Constractor

    public TakeCard(SC_Card _card) : base(_card)
    {
        // bomb cards are not playable like that
        if (card.Type == CardTypes.Exploding) { return; }
        onClickDown += StartDrag;
        onClickUp += EndTake;
        onClickUp += EndDrag;
    }

    #endregion

    #region Actions

    private void EndTake()
    {
        SC_PlayerHand playerHand = SC_GameData.Instance.GetContainer(Containers.PlayerHand) as SC_PlayerHand;
        if (playerHand == null)
        {
            Debug.LogError("Failed to take card! player hand is null");
            return;
        }
        // take end successful
        if (Mathf.Abs(playerHand.CS.Origin.y - card.Position.y) < playerHand.takeRange)
        {
            Containers oldHome = card.Home;
            card.ChangeHome(Containers.PlayerHand);
            // update state as needed
            CardContainer _center = SC_GameData.Instance.GetContainer(Containers.Center);
            if (_center == null) {
                Debug.LogError("Failed to set update after taking card! center is null.");
                return;
            }
            if (oldHome != Containers.Center || _center.count == 0) {
                ChangeStateToMyPlayOrDraw();
            }
            else { ChangeStateToMyTakeAction(); }
            if (oldHome != Containers.Center) {
                SC_GameLog.Instance.AddMessege($"{SC_GameLogic.Instance.currentPlayer} Stole a card from {oldHome}");
            }
            
        }
        // take cancel
        else
        {
            // nothing happens
        }
    }

    #endregion

}
