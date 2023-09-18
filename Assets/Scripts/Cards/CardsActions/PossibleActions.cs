using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// Represents the possible actions a player can take.
/// <para>
/// Static Class used as a single, shared set of possible actions across the entire game. 
/// </para>
/// Used in cards to Update behavior and view
/// </summary>
public static class PossibleActions
{

    public static bool Draw;
    public static bool Play;
    public static bool TakeBack;
    public static bool ClickDeck;
    public static bool ClickDiscards;
    public static bool ClickOpponent;
    public static bool PickCard;
    public static bool PickCardType;
    public static bool Nope;

    public static void Set(GameStates gameState)
    {
        if (gameState == GameStates.error) {
            Debug.LogError($"Failed to set possible actions, game state: { gameState }");
            return;
        }

        Draw = Play = TakeBack = ClickDeck = ClickDiscards =
        ClickOpponent = PickCard = PickCardType = Nope = false;

        switch (gameState)
        {   
            case GameStates.MyPlayOrDraw:
                Play = Draw = true;
                break;
            case GameStates.MyTakeAction:
                Play = TakeBack = true;
                break;
            case GameStates.MyDefuse:
                Play = true;
                break;
            case GameStates.OthersTakeAction:
                Nope = true;

                return;
        }
    }
}
public enum Actions
{
    error = -1,
    // play or draw
    Draw,
    Play,
    // take action
    TakeBack, 
    ClickDeck,
    ClickDiscards,
    ClickOpponent,
    // complete action
    PickCard,
    PickCardType,
    PlantBomb,
    EndTurn,

    nope,

}

