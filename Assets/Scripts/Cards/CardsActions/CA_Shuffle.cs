using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class SetUpShuffle : PlayAction
{

    #region Constractor

    public SetUpShuffle(SC_Card _card) : base(_card)
    {
        Debug.Log($"<color=blue>Assgined {_card} with SetUp Shuffle</color>");
        onClickUp += SetShuffleAction;
        onClickUp += () => ChangeButtonAction(() => { Shuffle.ShuffleDeck(); ChangeStateToMyPlayOrDraw(); });
        onClickUp += () => ChangeButtonText("Shuffle the deck!");
    }

    #endregion

    #region Logic

    public static event UnityAction SetShuffleAction;

    #endregion

}
