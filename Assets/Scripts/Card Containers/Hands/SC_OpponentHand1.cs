using UnityEngine;

public class SC_OpponentHand1 : CardContainer
{
    #region Variables
    protected float gettingCloserRate;
    public float takeRange;
    #endregion

    #region MonoBehaviour
    void Start()
    {
        InitVariables();
    }

    #endregion

    #region Logic

    public override void InitVariables()
    {
        gettingCloserRate = 0.85837f;
        takeRange = 5;
        containerSettings = new()
        {
            KeepCardsFaceUp = false,
            FixPerspective = true,
            offsetDistance = 0.5f,
            OnHoverOffset = Vector2.up / 3,

            offsetRotation = new(0, 0, -5),

            originSortOrder = 0,
            offsetSortOrder = 1
        };
    }

    #endregion

    #region Methods
    public override void PositionPlayer(int numberOfPlayer)
    {
        containerSettings.Container = Containers.OpponentHand1;
        switch (numberOfPlayer)
        {
            case 2:
                containerSettings.stackDirection = new(1, 0);
                containerSettings.Ceiling = new(0, 6f);
                containerSettings.Origin = new(-0.5f, 3.5f);
                containerSettings.originRotation = new(0, 0, 10);
                break;
            case 3:
                containerSettings.stackDirection = new(1, 0.2f);
                containerSettings.Ceiling = new(0, 6f);
                containerSettings.Origin = new(-3, 3.35f);
                containerSettings.originRotation = new(0, 0, 20);
                break;
            case 4:
                containerSettings.stackDirection = new(1, 0.2f);
                containerSettings.Ceiling = new(0, 6f);
                containerSettings.Origin = new(-4, 3.15f);
                containerSettings.originRotation = new(0, 0, 20);
                break;
            case 5:
                containerSettings.stackDirection = new(1, 0.3f);
                containerSettings.Ceiling = new(-5, 2.75f);
                containerSettings.Origin = new(-5, 2.75f);
                containerSettings.originRotation = new(0, 0, 30);
                break;
            default:
                Debug.LogError("Failed to position Opponent hand 1! invalid number of players.");
                return;
        }
    }

    public override void InsertBefore(SC_Card node, SC_Card toNext)
    {
        base.InsertBefore(node, toNext);
        if (count != 1)
        {
            CS.offsetDistance *= gettingCloserRate;
            CS.Ceiling = CS.Origin + (CS.offsetDistance * count) * StackDirection; 
            CS.originRotation -= CS.offsetRotation / 2;
            ResetContainer();
        }
    }

    public override void Remove(SC_Card node)
    {
        base.Remove(node);
        if (count != 1)
        {
            CS.offsetDistance /= gettingCloserRate;
            CS.Ceiling = CS.Origin + (CS.offsetDistance * count) * StackDirection;
            CS.originRotation += CS.offsetRotation / 2;
            ResetContainer();
        }
    }

    #endregion
}
