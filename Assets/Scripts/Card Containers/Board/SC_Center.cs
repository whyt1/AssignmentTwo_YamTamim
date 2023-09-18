using System;
using UnityEngine;

public class SC_Center : CardContainer
{

    #region MonoBehaviour
    void OnEnable()
    {
        SC_GameLogic.OnStateTransition += OnStateTransition;
    }
    void Ondisable()
    {
        SC_GameLogic.OnStateTransition -= OnStateTransition;
    }

    void Start()
    {
        InitVariables();
    }

    #endregion

    #region Logic

    /// <summary>
    /// On State Change dumps cards in play to the discards
    /// </summary>
    private void OnStateTransition(GameStates newState)
    { 
        // nothing to dump or action not done yet
        if (Head == null || newState == GameStates.MyTakeAction)
        {
            return;
        }
        int i = 0;
        while (Head != null && i < MaxCapacitiy) {
            if (Head.Type != CardTypes.Exploding) {
                Head.ChangeHome(Containers.Discards);
            }
            i++;
        }
    }

    public override void InitVariables()
    {
        head = null;
        tail = null;

        containerSettings = new()
        {
            Container = Containers.Center,
            KeepCardsFaceUp = true,
            FixPerspective = false,
            stackDirection = new(1, -0.3f),
            OnHoverOffset = Vector2.up/3,

            Ceiling = new(1, 3),
            Origin = new(-0.4f, 0.3f),
            offsetDistance = 0.3f,

            originRotation = new(0, 0, 0),
            offsetRotation = new(0, 0, 5),

            originSortOrder = 0,
            offsetSortOrder = 1
        };
    }

    #endregion
}
