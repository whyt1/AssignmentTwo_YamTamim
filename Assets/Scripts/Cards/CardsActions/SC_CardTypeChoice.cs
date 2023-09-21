using UnityEngine;

public class SC_CardTypeChoice : MonoBehaviour
{
    #region Variables

    public static CardContainer ToStealFrom; 
    [SerializeField]
    private CardTypes type;

    #endregion

    #region MonoBehaviour

    void OnMouseUpAsButton()
    {
        if (type == CardTypes.error || ToStealFrom == null)
        {
            Debug.LogError($"Failed to choose type to steal! type = {type}, to steal from = {ToStealFrom}");
            return;
        }
        SC_Card card = ToStealFrom.GetCard(type: type);
        if (card != null) {
            card.ChangeHome(Containers.PlayerHand);
        }
        SC_GameLogic.Instance.ChangeState(GameStates.MyPlayOrDraw);
        SC_GameLog.Instance.AddMessege($"{SC_GameLogic.Instance.currentPlayer} Stole a card from {ToStealFrom}");

        GameObject cardTypes = SC_GameData.Instance.GetUnityObject("CardTypes");
        if (cardTypes == null)
        {
            Debug.LogError("Failed to Steal Spesific card! card types is null.");
            return;
        }
        cardTypes.SetActive(false);
    }

    #endregion
}
