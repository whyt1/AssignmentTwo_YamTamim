using UnityEngine;

public class SC_OpponentHand2 : SC_OpponentHand1
{
    #region Methods
    public override void PositionPlayer(int numberOfPlayer)
    {
        containerSettings.Container = Containers.OpponentHand2;
        switch (numberOfPlayer)
        {
            case 3:
                containerSettings.stackDirection = new(1, -0.1f);
                containerSettings.Ceiling = new(0, 6f);
                containerSettings.Origin = new(2, 3.5f);
                containerSettings.originRotation = new(0, 0, 10);
                break;
            case 4:
                containerSettings.stackDirection = new(1, 0);
                containerSettings.Ceiling = new(0, 6f);
                containerSettings.Origin = new(-0.5f, 3.5f);
                containerSettings.originRotation = new(0, 0, 10);
                break;
            case 5:
                containerSettings.stackDirection = new(1, 0.2f);
                containerSettings.Ceiling = new(0, 6f);
                containerSettings.Origin = new(-2, 3.35f);
                containerSettings.originRotation = new(0, 0, 20);
                break;
            default:
                Debug.LogError("Failed to position Opponent hand 2! invalid number of players.");
                return;
        }
    }
    #endregion
}
