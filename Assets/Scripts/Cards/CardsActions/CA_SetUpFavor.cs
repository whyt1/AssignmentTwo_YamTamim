using UnityEngine;
using UnityEngine.Events;

internal class SetUpFavor : PlayAction
{
    #region Constractor

    public SetUpFavor(SC_Card _card) : base(_card)
    {
        // Debug.Log($"<color=blue>Assgined {_card} with SetUp Shuffle</color>");
        onClickUp += SetFavorAction;
        onClickUp += () => ChangeButtonAction(null);
        onClickUp += () => ChangeButtonText("Click player to ask favor!");
    }

    #endregion

    #region Logic

    public static event UnityAction SetFavorAction;

    #endregion
}