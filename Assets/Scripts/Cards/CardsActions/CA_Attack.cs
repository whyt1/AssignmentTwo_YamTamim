using System;
using UnityEngine;

[Serializable]
public class Attack : PlayAction
{

    #region Constractor

    public Attack(SC_Card _card) : base(_card)
    {
        // Debug.Log($"<color=blue>Assgined {_card} with Attack</color>");
        onClickUp += () => ChangeButtonAction(() => {
            SC_GameLogic.Instance.OnEndTurn();
            SC_GameLogic.Instance.attackStack++;
        });
        onClickUp += () => ChangeButtonText("Click To Skip Turn and attack!");
    }

    #endregion

}
