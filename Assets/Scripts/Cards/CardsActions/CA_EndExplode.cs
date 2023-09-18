﻿using System;
using UnityEngine;
[Serializable]
internal class EndExplode : CardAction
{
    public EndExplode(SC_Card _card) : base(_card)
    {
        onClickDown += StartDrag;
        onClickUp += EndExploding;
        onClickUp += EndDrag;
    }

    private void EndExploding()
    {
        SC_Deck _deck = SC_GameData.Instance.GetContainer(Containers.Deck) as SC_Deck;
        if (_deck == null) {
            Debug.LogError("Failed to end explode! deck is null");
            return;
        }
        if (_deck.BombNearDeck())
        {
            Debug.Log("<color=red>Bomb Planted!</color>");
            _deck.isBombPlanted = true; 
        }
    }

}