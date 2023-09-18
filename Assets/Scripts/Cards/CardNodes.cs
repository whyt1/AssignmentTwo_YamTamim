using System;

/// <summary>
/// Nodes for the linked list structre <para></para>
/// <seealso cref="CardContainer"/>
/// </summary>
[Serializable]
public class CardNode
{

    #region Fields

    public CardContainer home;
    public int index;
    public SC_Card prev;
    public SC_Card card;
    public SC_Card next;

    #endregion
    
    #region Constractors

    public CardNode(SC_Card _prev = null, SC_Card _card = null, SC_Card _next = null, CardContainer _home = null, int _index = 0)
    {
        next = _next;
        card = _card;
        prev = _prev;
        index = _index;
        home = _home;
    }

    #endregion

    #region Methods

    public override string ToString()
    {
        return $"{prev} <-> {card} <-> {next}"; 
    }

    #endregion
    
}
