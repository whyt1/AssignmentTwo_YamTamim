using System;
using UnityEngine;
[Serializable]
public class DrawCard : CardAction
{

    #region Constractor

    public DrawCard(SC_Card _card) : base(_card)
    {
        // Debug.Log($"<color=green>Assgined {_card} with Draw Card Action</color>");
        onClickDown += StartDrag;
        onClickDown += MoveToCenter;
        onClickUp += EndDrag;
        onClickUp += EndDraw;
    }

    #endregion

    #region Actions

    public void EndDraw()
    {
        Debug.Log("Finished Drawing Card");
        // draw end successful
        if (card.IsFaceUp)
        { 
            if (card.Type != CardTypes.Exploding)
            {
                Debug.Log("change state to end turn");
                // game state transtion to end turn
                MoveToPlayerHand();
                SC_GameLogic.Instance.ChangeState(GameStates.MyEndTurn);
                SC_GameLog.Instance.AddMessege($"{SC_GameLogic.Instance.currentPlayer} drew a card");
                return;
            }

            Debug.Log("<color=red>EXPLODING KITTEN</color>");
            // move bomb to center
            card.ChangeHome(Containers.Center);
            SC_GameLogic.Instance.ChangeState(GameStates.MyDefuse);
            SC_GameLog.Instance.AddMessege($"{SC_GameLogic.Instance.currentPlayer} exploded");

            // Check if player has defuse in hand
            if (!CheckHandForDefuses())
            {
                // game over
                Debug.Log("<color=red>game over</color>");
                SC_GameLogic.Instance.ChangeState(GameStates.MyGameOver);
            }
        }
        // draw was canceled 
        else
        {
            Debug.Log("send card back to deck");
            card.ChangeHome(Containers.Deck);
        }
    }

    /// <summary>
    /// Used when someone explodes to make sure he has defuses and to activate them. <para>
    /// </para>
    /// Not using <see cref="CardContainer.GetCard"/> becuase we want all defuses not just the first.
    /// </summary>
    private bool CheckHandForDefuses()
    {
        SC_PlayerHand playerHand = SC_GameData.Instance.GetContainer(Containers.PlayerHand) as SC_PlayerHand;
        if (playerHand == null)
        {
            Debug.LogError("Failed to start defuse! deck is null");
            return false;
        }
        return playerHand.CheckHandForType(CardTypes.Defuse);
    }

    #endregion

}
