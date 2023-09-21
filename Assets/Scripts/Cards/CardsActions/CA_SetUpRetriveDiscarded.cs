using UnityEngine;
using UnityEngine.Events;

public class SetUpRetrieveDiscarded : PlayAction
{

    #region Constractor

    public SetUpRetrieveDiscarded(SC_Card _card) : base(_card)
    {
        Debug.Log($"<color=blue>Assgined {_card} with SetUp Retrieve Discarded</color>");
        onClickUp += SetRetrieveDiscardedAction;
        onClickUp += () => ChangeButtonAction(null);
        onClickUp += () => ChangeButtonText("Take a card from discards!");
        onClickUp += StartRetrieve;
    }

    #endregion

    #region Logic

    public static event UnityAction SetRetrieveDiscardedAction;

    public void StartRetrieve()
    {
        SC_Discards _discards = SC_GameData.Instance.GetContainer(Containers.Discards) as SC_Discards;
        if (_discards == null)
        {
            Debug.LogError("Failed to Retrieve Discarded! Discards is null");
            return;
        }
        _discards.isRetrieveDiscarded = true;
    }

    #endregion

}