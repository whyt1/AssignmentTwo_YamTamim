using System.Collections;
using UnityEngine;

public class SC_PlayerHand : CardContainer
{

    public float takeRange;
    [SerializeField]
    private float scrollRange;
    [SerializeField]
    private float scrollSpeed;
    [SerializeField]
    private float limitBeforeScroll;
    private bool isPushingCards;


    #region MonoBehaviour

    void Start()
    {
        InitVariables();
    }

    void Update()
    {
        if (CS.originRelative.x < limitBeforeScroll)
        {
            PushCards();
        }
    }

    #endregion

    #region Logic

    public override void InitVariables()
    {
        takeRange = 5;
        scrollRange = 2;
        scrollSpeed = 2;
        limitBeforeScroll = -0.9f; // -1 is screen edge
        containerSettings = new()
        {
            Container = Containers.PlayerHand,
            KeepCardsFaceUp = true,
            FixPerspective = true,
            stackDirection = new(1, 0),
            OnHoverOffset = Vector2.up,

            Ceiling = new(0, -4.5f),
            Origin = new(0, -4.5f),
            offsetDistance = 1, // 0.5 * card width

            originRotation = new(0, 0, 0),
            offsetRotation = new(0, 0, 0),

            originSortOrder = 0,
            offsetSortOrder = 1
        };
    }

    /// <summary>
    /// Used to scroll the cards when they are overflowing from the screen.
    /// </summary>
    private void PushCards()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 bottomRight = SC_GameData.Instance.screenSize * (Vector2.down + Vector2.right);
        Vector2 bottomLeft = SC_GameData.Instance.screenSize * (Vector2.down + Vector2.left);

        if ((mousePosition - bottomRight).magnitude < scrollRange)
        {

            if (!isPushingCards)
            {
                isPushingCards = true;
                StartCoroutine(PushCardsLeft());
            }
            ResetContainer();
        }
        else if ((mousePosition - bottomLeft).magnitude < scrollRange)
        {

            if (!isPushingCards)
            {
                isPushingCards = true;
                StartCoroutine(PushCardsRight());
            }
            ResetContainer();
        }
    } 

    #endregion

    #region Methods

    public override void InsertBefore(SC_Card node, SC_Card toNext)
    {
        base.InsertBefore(node, toNext);
        if (count != 1) { 
            CS.Origin -=  StackDirection * CS.offsetDistance / 2; 
            CS.Ceiling += StackDirection * CS.offsetDistance / 2;
            CS.originRotation -= CS.offsetRotation / 2;
        }
    }

    public override void Remove(SC_Card node)
    {
        base.Remove(node);
        if (count != 1)
        {
            CS.Origin += StackDirection * CS.offsetDistance / 2;
            CS.Ceiling -= StackDirection * CS.offsetDistance / 2;
            CS.originRotation += CS.offsetRotation / 2;
            ResetContainer();   
        }
    }

    #endregion


    #region IEnumerators

    /// <summary>
    /// Used when moving cards when cards are offscreen
    /// </summary>
    private IEnumerator PushCardsLeft()
    {
        Debug.Log("pushing cards left");
        yield return new WaitForSeconds(1 / scrollSpeed);

        CS.Origin -= StackDirection * CS.offsetDistance;
        CS.Ceiling -= StackDirection * CS.offsetDistance;

        yield return new WaitForSeconds(1 / scrollSpeed);
        isPushingCards = false;
    }

    /// <summary>
    /// Used when moving cards when cards are offscreen
    /// </summary>
    private IEnumerator PushCardsRight()
    {
        Debug.Log("pushing cards right");

        yield return new WaitForSeconds(1 / scrollSpeed);
        CS.Origin += StackDirection * CS.offsetDistance;
        CS.Ceiling += StackDirection * CS.offsetDistance;

        yield return new WaitForSeconds(1 / scrollSpeed);
        isPushingCards = false;
    }

    #endregion
}
