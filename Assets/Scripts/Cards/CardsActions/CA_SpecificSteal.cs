using System;
using UnityEngine;

/// <summary>
/// Used when cards are taken from other containers to the player hand
/// </summary>
public class SpecificSteal : CardAction
{

    #region Constractor

    public SpecificSteal(SC_Card _card) : base(_card)
    {
        onClickUp += ChooseToStealFrom;
        onClickUp += () => ChangeButtonText("Pick Card Type To Steal!");
        onClickUp += ChangeStateToMyTakeAction;
    }

    #endregion

    #region Actions

    private void ChooseToStealFrom()
    {
        CardContainer toStealFrom = SC_GameData.Instance.GetContainer(card.Home) as CardContainer;
        if (toStealFrom == null)
        {
            Debug.LogError("Failed to Choose To Steal! clicked card Home is null");
            return;
        }
        GameObject cardTypes = SC_GameData.Instance.GetUnityObject("CardTypes");
        if (cardTypes == null) { Debug.LogError("Failed to Steal Spesific card! card types is null.");
            return;
        }
        cardTypes.SetActive(true);
        cardTypes.transform.position = toStealFrom.CS.Origin;
        SC_CardTypeChoice.ToStealFrom = toStealFrom;
    }

    #endregion

}
