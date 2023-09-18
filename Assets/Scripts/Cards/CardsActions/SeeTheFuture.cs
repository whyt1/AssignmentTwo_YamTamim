using UnityEngine;

internal class SeeTheFuture : CardAction
{
    static int depth = 3;

    #region Constractor

    public SeeTheFuture(SC_Card _card) : base(_card)
    {
        onClickUp += SeeTheFutureDeck;
        onClickUp += ChangeStateToMyPlayOrDraw;
    }

    #endregion

    #region Action

    public static void SeeTheFutureDeck()
    {
        SC_Deck _deck = SC_GameData.Instance.GetContainer(Containers.Deck) as SC_Deck;
        if (_deck == null)
        {
            Debug.LogError("Failed to See The Future! deck is null");
            return;
        }
        _deck.SeeTheFuture(depth);
    }

    #endregion
}