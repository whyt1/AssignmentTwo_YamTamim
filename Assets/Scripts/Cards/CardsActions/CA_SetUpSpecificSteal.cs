using UnityEngine;
using UnityEngine.Events;

public class SetUpSpecificSteal : PlayAction
{

    #region Constractor

    public SetUpSpecificSteal(SC_Card _card) : base(_card)
    {
        Debug.Log($"<color=blue>Assgined {_card} with SetUp Specific Steal</color>");
        onClickUp += SetSpecificStealAction;
        onClickUp += () => ChangeButtonAction(null);
        onClickUp += () => ChangeButtonText("Click player to steal a card!");
    }

    #endregion

    #region Logic

    public static event UnityAction SetSpecificStealAction;

    #endregion

}