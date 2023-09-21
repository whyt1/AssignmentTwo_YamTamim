using UnityEngine;

public class Favor : CardAction
{

    #region Constractor

    public Favor(SC_Card _card) : base(_card)
    {
        onClickUp += TakeRandomCard;
        onClickUp += ChangeStateToMyPlayOrDraw;
    }

    #endregion

    #region Action

    public void TakeRandomCard()
    {
        Containers opponent = card.Home;
        SC_OpponentHand1 opponentHand = SC_GameData.Instance.GetContainer(opponent) as SC_OpponentHand1;
        if (opponentHand == null)
        {
            Debug.LogError("Failed to Take Random Card! opponent hand is null");
            return;
        }
        SC_Card cardToTake = opponentHand.GetRandomCard(opponentHand.count);
        cardToTake.ChangeHome(Containers.PlayerHand);
        SC_GameLog.Instance.AddMessege($"{SC_GameLogic.Instance.currentPlayer} Played Favor on {opponent}");
    }

    #endregion
}