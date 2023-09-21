using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Skip : PlayAction
{

    #region Constractor

    public Skip(SC_Card _card) : base(_card)
    {
        // Debug.Log($"<color=blue>Assgined {_card} with Skip</color>");
        onClickUp += () => ChangeButtonAction(() => { 
            SC_GameLog.Instance.AddMessege($"{SC_GameLogic.Instance.currentPlayer} Played Skip"); 
            ChangeStateToMyEndTurn(); 
        });
        onClickUp += () => ChangeButtonText("Click To Skip Draw");
    }

    #endregion

}
