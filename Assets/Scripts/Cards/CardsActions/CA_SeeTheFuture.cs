using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class SetUpSeeTheFuture : PlayAction
{

    #region Constractor

    public SetUpSeeTheFuture(SC_Card _card) : base(_card)
    {
        Debug.Log($"<color=blue>Assgined {_card} with See The Future</color>");
        onClickUp += SetSeeTheFutureAction;
        onClickUp += () => ChangeButtonAction(() => { SeeTheFuture.SeeTheFutureDeck(); ChangeStateToMyPlayOrDraw(); });
        onClickUp += () => ChangeButtonText("See The Future!");
    }

    #endregion

    #region Logic

    public static event UnityAction SetSeeTheFutureAction;

    #endregion

}
