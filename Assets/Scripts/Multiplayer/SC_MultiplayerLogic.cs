using AssemblyCSharp;
using com.shephertz.app42.gaming.multiplayer.client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using static SC_MenuController;

public class SC_MultiplayerLogic : MonoBehaviour
{
    #region API Keys
    private string apiKey = "7a4d7ab6dd01a049d4153354dfe6b262e328f6c0f4b5b63cfda7a4898b993adf";
    private string secretKey = "72870153452ab88beae898d3d3331814f579f8695eb9d56d8a4cec746d0b6a69";
    private Listener listener;
    #endregion

    #region Variables
    private string userId;
    #endregion

    #region MonoBehaviour
    void OnEnable()
    {
        Listener.OnConnect += OnConnect;
        SC_MenuController.OnChangeScreen += OnChangeScreen;
    }

    void OnDisable()
    {
        Listener.OnConnect -= OnConnect;
        SC_MenuController.OnChangeScreen -= OnChangeScreen;
    }
    void Awake()
    {
        initVariables();
    }
    #endregion

    #region Logic
    private void initVariables()
    {
        if (listener == null)
        {
            listener = new Listener();
        }
        WarpClient.initialize(apiKey, secretKey);
        WarpClient.GetInstance().AddConnectionRequestListener(listener);
        WarpClient.GetInstance().AddChatRequestListener(listener);
        WarpClient.GetInstance().AddUpdateRequestListener(listener);
        WarpClient.GetInstance().AddLobbyRequestListener(listener);
        WarpClient.GetInstance().AddNotificationListener(listener);
        WarpClient.GetInstance().AddRoomRequestListener(listener);  
        WarpClient.GetInstance().AddTurnBasedRoomRequestListener(listener);
        WarpClient.GetInstance().AddZoneRequestListener(listener);

        userId = System.Guid.NewGuid().ToString();
        UpdateText("userId", "User ID: " + userId);

    }

    // should be in view manager
    private void UpdateText(string GameObjectName, string newText)
    {
        GameObject TxtGameObject = GameObject.Find("Txt_" + GameObjectName);
        if (TxtGameObject != null)
        {
            TextMeshProUGUI myText = TxtGameObject.GetComponent<TextMeshProUGUI>();
            if (myText != null)
            {
                myText.text = newText;
            } 
            else { Debug.LogError(GameObjectName + " text component is null"); }
        } 
        else { Debug.LogError(GameObjectName + " game object is null"); }
    }
    #endregion

    #region Server Callbacks
    private void OnConnect(bool _IsSuccess)
    {
        if (_IsSuccess)
        {
            UpdateText("status", "Status: Connected");
            if (SC_GameData.Instance.GetUnityObject("Btn_MoveToGame") != null) {
                SC_GameData.Instance.GetUnityObject("Btn_MoveToGame").SetActive(true);
            }
            if (SC_GameData.Instance.GetUnityObject("Txt_Loading") != null) {
                SC_GameData.Instance.GetUnityObject("Txt_Loading").SetActive(false);
            }
        }
        else
        {
            UpdateText("status", "Status: Failed to connect");
        }
    }
    #endregion

    #region Events
    private void OnChangeScreen(string _screen)
    {
        if (_screen == "Loading")
        {
            WarpClient.GetInstance().Connect(userId);
            UpdateText("status", "Status: Open Connection...");

            if (SC_GameData.Instance.GetUnityObject("Btn_MoveToGame") != null) {
                SC_GameData.Instance.GetUnityObject("Btn_MoveToGame").SetActive(false);
            }
            if (SC_GameData.Instance.GetUnityObject("Txt_Loading") != null) {
                SC_GameData.Instance.GetUnityObject("Txt_Loading").SetActive(true);
            }
            return;
        }
        if (_screen == "Game")
        {

        }
    }
    #endregion
}
