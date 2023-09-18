using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SC_ButtonManager : MonoBehaviour
{
    #region Variables

    Button myButton;
    TextMeshProUGUI myText;

    #endregion

    #region MonoBehaviour
    void OnEnable()
    {
        SC_GameLogic.OnStateTransition += OnStateTransition;
        CardAction.OnChangeButton += OnChangeButton;
        SC_GameLogic.OnYouWin += OnYouWin;
    }
    void OnDisable()
    {
        SC_GameLogic.OnStateTransition -= OnStateTransition;
        CardAction.OnChangeButton -= OnChangeButton;
        SC_GameLogic.OnYouWin -= OnYouWin;
    }
    void Awake()
    {
        InitVariables();
    }
    #endregion

    #region Events

    private void OnChangeButton(UnityAction clickOnAction = null, string newText = null)
    {
        if (clickOnAction != null) { ChangeButtonAction(clickOnAction); }
        if (newText != null) { ChangeButtonText(newText); }
    }

    private void OnYouWin()
    {
        ChangeButtonAction(SC_GameLogic.Instance.OnStartGame);
        ChangeButtonText("YOU WIN! \nClick to Restart");
    }

    private void OnStateTransition(GameStates state)
    {
        switch (state)
        {
            case GameStates.error:
                Debug.LogError("Failed to set button action and text. Button got error game state");
                ChangeButtonAction(SC_GameLogic.Instance.OnStartGame);
                ChangeButtonText("ERROR! \nClick to Restart");
                return;
            case GameStates.MyPlayOrDraw:
                ChangeButtonAction(null);
                ChangeButtonText("Play Cards Or Draw");
                return;
            case GameStates.MyTakeAction:
                ChangeButtonAction(null);
                ChangeButtonText("Play more cards to combo!");
                return;
            case GameStates.MyDefuse:
                ChangeButtonAction(null);
                ChangeButtonText("Play Your Defuse!");
                return;
            case GameStates.MyEndTurn:
                ChangeButtonAction(SC_GameLogic.Instance.OnEndTurn);
                ChangeButtonText("Click To End Turn");
                return;
            case GameStates.MyGameOver:
                ChangeButtonAction(SC_GameLogic.Instance.OnStartGame);
                ChangeButtonText("Game Over! \nClick to Restart");
                return;
            case GameStates.OthersTakeAction:
                // nope activate
            default:
                ChangeButtonAction(null);
                ChangeButtonText("Wait Your Turn");
                return;
        }
    }

    #endregion

    #region Logic

    private void InitVariables()
    {
        GameObject _text = transform.GetChild(0) != null ? transform.GetChild(0).gameObject 
                                                         : null;
        myText = _text != null ? _text.GetComponent<TextMeshProUGUI>()
                               : null;
        myButton = GetComponent<Button>();
        ChangeButtonAction(SC_GameLogic.Instance.OnStartGame);
        ChangeButtonText("Click To Start");
    }

    public void ChangeButtonAction(UnityAction onClickAction)
    {
        if (myButton == null) { 
            Debug.LogError("Failed to set button action! button Component is null");
            return;
        }

        myButton.onClick.RemoveAllListeners();
        if (onClickAction != null) {
            myButton.enabled = true;
            myButton.onClick.AddListener(onClickAction);
        }
        else {
            myButton.enabled = false;   
        }
    }

    public void ChangeButtonText(string newText)
    {
        if (myText == null) {
            Debug.LogError("Failed to set button text! text Component is null");
            return;
        }

        myText.text = newText;
    }

    #endregion
}
