
/// <summary>
/// Represents the card containers in the game world.
/// <para></para>
/// <seealso cref="CardContainer"/> each Container is matched with a card list type
/// <para></para>
/// <seealso cref="SC_GameData.gameWorld"/> Used as keys in game world dict
/// </summary>
public enum Containers
{
    error = -1,
    /// <summary>
    /// Used to hold cards in play until action is taken.
    /// </summary>
    Center,
    PlayerHand,
    Deck,
    Discards,
    OpponentHand1,
    OpponentHand2,
    OpponentHand3,
    OpponentHand4,
}
