using UnityEngine;

public class SC_OpponentHand3 : SC_OpponentHand1
{
    #region Methods
    public override void PositionPlayer(int numberOfPlayer)
    {
        containerSettings.Container = Containers.OpponentHand3;
        switch (numberOfPlayer)
        {
            case 4:
                containerSettings.stackDirection = new(1, -0.2f);
                containerSettings.Ceiling = new(0, 6f);
                containerSettings.Origin = new(3, 3.2f);
                containerSettings.originRotation = new(0, 0, -10);
                break;
            case 5:
                containerSettings.stackDirection = new(1, -0.2f);
                containerSettings.Ceiling = new(0, 6f);
                containerSettings.Origin = new(1, 3.5f);
                containerSettings.originRotation = new(0, 0, -20);
                break;
            default:
                Debug.LogError("Failed to position Opponent hand 3! invalid number of players.");
                return;
        }
    }
    #endregion
}
