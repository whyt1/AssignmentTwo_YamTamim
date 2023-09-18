using UnityEngine;

public class Shuffle : CardAction
{
    #region Constractor

    public Shuffle(SC_Card _card) : base(_card)
    {
        onClickUp += ShuffleDeck;
        onClickUp += ChangeStateToMyPlayOrDraw;
    }

    #endregion

    #region Action

    public static void ShuffleDeck()
    {
        SC_Deck _deck = SC_GameData.Instance.GetContainer(Containers.Deck) as SC_Deck;
        if (_deck == null)
        {
            Debug.LogError("Failed to shuffle deck! deck is null");
            return;
        }
        _deck.Shuffle();
        _deck.ResetContainer();
    }

    #endregion
}