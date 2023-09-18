using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_Discards : CardContainer
{

    #region MonoBehaviour

    void Start()
    {
        InitVariables();
    }

    #endregion

    public override void InitVariables()
    {
        head = null;
        tail = null;

        containerSettings = new()
        {
            Container = Containers.Discards,
            KeepCardsFaceUp = true,
            FixPerspective = false,
            stackDirection = new(0, 1),
            OnHoverOffset = Vector2.up / 2,

            Ceiling = new(3, 3),
            Origin = new(3, -1),
            offsetDistance = 0.06f,

            originRotation = new(50, 0, 15),
            offsetRotation = Vector3.zero,

            originSortOrder = 0,
            offsetSortOrder = 1
        };
    }

    public override void InsertBefore(SC_Card node, SC_Card toNext)
    {
        base.InsertBefore(node, toNext);
        // reset actions for discarded cards
        node.action = null;
        ResetContainer();
    }
}
