using System.ComponentModel;
using UnityEngine;

public class SC_OpponentHand4 : SC_OpponentHand1
{
    #region Methods
    public override void PositionPlayer(int numberOfPlayer)
    {
        containerSettings.Container = Containers.OpponentHand4;
        switch (numberOfPlayer)
        {
            case 5:
                containerSettings.stackDirection = new(1, -0.3f);
                containerSettings.Ceiling = new(0, 6f);
                containerSettings.Origin = new(4, 3f);
                containerSettings.originRotation = new(0, 0, -30);
                break;
            default:
                Debug.LogError("Failed to position Opponent hand 4! invalid number of players.");
                return;
        }
    }
    #endregion
}
