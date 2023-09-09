using System;
using UnityEngine;

/// <summary>
/// Nodes for the linked list structre <para></para>
/// <seealso cref="CardContainer"/>
/// </summary>
[Serializable]
public class CardNode
{

    #region Fields

    [SerializeField]
    private int index;
    [SerializeField]
    private SC_Card next;
    [SerializeField]
    private SC_Card card;
    [SerializeField]
    private SC_Card prev;

    private SC_Card nextNode;
    private SC_Card prevNode;

    #endregion

    #region Proporties



    #endregion
    /*
    #region Constractors

    public CardNode(SC_Card _card, CardNode _prev = null, CardNode _next = null)
    {
        if (_card == null) { 
            Debug.LogError("Failed to create new node! card is null"); 
            return; 
        }
        Card = _card;
        Next = _next;
        Prev = _prev;
    }

    #endregion

    #region Methods

    public override string ToString()
    {
        if (Prev != null && Next != null) return $"{Prev.Card} <-> {Card} <-> {Next.Card}"; 
        if (Prev != null) return $"{Prev.Card} <-> {Card} <-|";
        if (Next != null) return $"|-> {Card} <-> {Next.Card}";
        return $"|-> {Card} <-|";
    }

    #endregion
    */
}
