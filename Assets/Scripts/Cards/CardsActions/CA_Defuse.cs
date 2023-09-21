using System;
using UnityEngine;

/// <summary>
/// Triggers Defuse sequence, used when a defuse card is played after bomb drawn 
/// </summary>
[Serializable]
public class Defuse : CardAction
{

    #region Constractor

    public Defuse(SC_Card _card) : base(_card)
    {
        Debug.Log($"<color=red>Set Defuse Action for {_card}</color>");
        onClickDown += StartDrag;
        onClickUp += EndDrag;
        onClickUp += () => ChangeButtonText("Plant The Bomb!");
        onClickUp += StartDefuse;
        onClickUp += MoveToDiscards;
    }

    #endregion

    #region Actions

    public void StartDefuse()
    {
        SC_Deck _deck = SC_GameData.Instance.GetContainer(Containers.Deck) as SC_Deck;
        if (_deck == null)
        {
            Debug.LogError("Failed to start defuse! deck is null");
            return;
        }
        _deck.isDefusing = true; // sets bomb in deck and starts defusing 
        _deck.bomb = GetBombFromCenter();
        if (_deck.bomb == null) { Debug.LogError("Failed to defuse! bomb is null."); return; }
        _deck.bomb.action = new EndExplode(_deck.bomb);
    }

    private SC_Card GetBombFromCenter()
    {
        CardContainer _center = SC_GameData.Instance.GetContainer(Containers.Center);
        if (_center == null)
        {
            Debug.LogError("Failed to defuse! can't get bomb, center is null.");
            return null;
        }

        SC_Card bomb = _center.Tail;
        while (bomb != null && bomb.Type != CardTypes.Exploding)
        {
            bomb = bomb.Next;
        }
        return bomb;
    }
    #endregion

}
