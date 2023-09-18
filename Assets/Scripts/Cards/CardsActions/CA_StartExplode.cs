using System;
using UnityEngine;
[Serializable]
public class StartExplode : CardAction
{

    #region Constractor

    public StartExplode(SC_Card _card): base(_card)
    {
        onClickDown += StartDrag;
        onClickDown += MoveToPlayerHand;
        onClickUp += EndDrag;
        onClickUp += StartExploding;
    }

    #endregion

    #region Actions

    public void StartExploding()
    {
        Debug.Log("Finished Drawing Card");
        // draw end successful
        if (card.IsFaceUp)
        {
            Debug.Log("<color=red>EXPLODING KITTEN</color>");
            // move bomb to center
            card.ChangeHome(Containers.Center);

            // Check if player has defuse in hand
            if (!CheckHandForDefuses())
            {
                // game over
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
        bool foundDefuse = false;
        SC_Card current = playerHand.Tail; int i = 0;
        while (current != null && i < CardContainer.MaxCapacitiy)
        {
            // set defuse active and continue 
            if (current.Type == CardTypes.Defuse)
            {
                current.action = new Defuse(current);
                foundDefuse = true;
            }
            current = current.Next; i++;
        }
        return foundDefuse;
    }

    #endregion

}
