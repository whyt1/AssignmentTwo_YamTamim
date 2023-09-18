using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class SetUpGive : PlayAction
{

    #region Constractor

    public SetUpGive(SC_Card _card) : base(_card)
    {
        Debug.Log($"<color=blue>Assgined {_card} with SetUp Shuffle</color>");
        onClickUp += SetGiveAction;
        onClickUp += () => ChangeButtonAction(null);
        onClickUp += () => ChangeButtonText("Give a card!");
    }

    #endregion

    #region Logic

    public static event UnityAction SetGiveAction;

    #endregion

}
