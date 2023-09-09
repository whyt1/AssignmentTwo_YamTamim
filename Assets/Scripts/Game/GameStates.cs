/// <summary>
/// Represents the possible states of the game.
/// <para>
/// Used by Game Manager to set possible interactions.
/// </para>
/// </summary>
public enum GameStates
{
    error = -1,
    MyPlayOrDraw,
    MyTakeAction,
    MyEndTurn,
    MyDefuse,
    MyGameOver,
    OthersPlayOrDraw,
    OthersTakeAction,
    OthersEndTurn,
    OthersDefuse,
    OthersGameOver,
}
