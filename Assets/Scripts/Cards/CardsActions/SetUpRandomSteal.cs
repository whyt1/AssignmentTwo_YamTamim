using UnityEngine;
using UnityEngine.Events;

public class SetUpRandomSteal : PlayAction
{

    #region Constractor

    public SetUpRandomSteal(SC_Card _card) : base(_card)
    {
        Debug.Log($"<color=blue>Assgined {_card} with SetUp Random Steal</color>");
        onClickUp += SetRandomStealAction;
        onClickUp += () => ChangeButtonAction(null);
        onClickUp += () => ChangeButtonText("Click player to steal a card!");
    }

    #endregion

    #region Logic

    public static event UnityAction SetRandomStealAction;

    #endregion

}