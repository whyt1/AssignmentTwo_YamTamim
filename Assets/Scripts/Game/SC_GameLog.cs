using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SC_GameLog : MonoBehaviour
{
    #region Variables 

    public ScrollRect scrollRect;
    public TextMeshProUGUI logText;
    public RectTransform rectTransform;
    public Image image;
    private List<string> messages;
    private bool showAll;
    public Vector2 openPosition;
    public Vector2 closePosition;
    public Vector2 openSize;
    public Vector2 closeSize;

    #endregion

    #region Singleton
    private static SC_GameLog instance;

    public static SC_GameLog Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject _gameLog = GameObject.FindGameObjectWithTag("GameLog");
                if (_gameLog == null)
                {
                    _gameLog = new GameObject("_gameLog", typeof(SC_GameLog));
                }

                instance = _gameLog.InitComponent<SC_GameLog>();
            }
            return instance;
        }
    }
    #endregion

    #region MonoBehaviour 

    private void Start()
    {
        InitVariables();
        UpdateLog(); 
    }

    private void Update()
    {
        LockScrollPosition();
    }

    #endregion

    #region Events

    public void AddMessege(string messege)
    {
        if (messages == null) { return; }
        Debug.Log(messege);
        messages.Insert(0, messege);
        UpdateLog();
    }

    #endregion

    #region Logic 

    private void InitVariables()
    {
        openPosition = new(0, -40);
        closePosition = new(0, 80);
        openSize = new(600, 300);
        closeSize = new(600, 50);
        messages = new List<string>();
        if (scrollRect == null)
        {
            Debug.LogError($"Failed to update scroll position! " +
                $"scrollRect = {scrollRect}\n" +
                "make sure to connect a components in unity inspector.");
            return;
        }
        scrollRect.normalizedPosition = new Vector2(0f, 1f);
    }

    private void LockScrollPosition()
    {
        if (scrollRect == null)
        {
            Debug.LogError($"Failed to update scroll position! " +
                $"scrollRect = {scrollRect}\n" +
                "make sure to connect a components in unity inspector.");
            return;
        }

        // Check if the scroll position is at the top
        if (scrollRect.normalizedPosition.y >= 1f)
        {
            // Disable vertical scrolling when at the top
            scrollRect.verticalNormalizedPosition = 1f;
            scrollRect.vertical = false;
        }
        else
        {
            // Enable vertical scrolling when not at the top
            scrollRect.vertical = showAll;
        }
    }

    private void UpdateLog()
    {
        if (logText == null || scrollRect == null || rectTransform == null || image == null)
        {
            Debug.LogError($@"Failed to update scroll position!
            logText = {logText},
            scrollRect = {scrollRect}, 
            image = {image}, 
            rectTransform = {rectTransform}.
            make sure to connect a components in unity inspector.");
            return;
        }
        if (!showAll)
        {
            image.enabled = false; 
            scrollRect.verticalNormalizedPosition = 1f;
            scrollRect.vertical = false;
            rectTransform.sizeDelta = closeSize;
            rectTransform.anchoredPosition = closePosition; 
            logText.text = (messages != null && messages.Count != 0) ? messages[0]
                                                                     : null;
        }
        else
        {
            image.enabled = true;
            scrollRect.vertical = true;
            rectTransform.sizeDelta = openSize;
            rectTransform.anchoredPosition = openPosition;
            logText.text = (messages != null && messages.Count != 0) ? string.Join("\n", messages)
                                                                     : null;
        }
    }

    public void ToggleLogVisibility()
    {
        showAll = !showAll;
        UpdateLog();
    }

    #endregion
}
