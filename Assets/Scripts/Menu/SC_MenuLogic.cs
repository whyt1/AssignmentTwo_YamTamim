using System;
using System.Collections.Generic;
using UnityEngine;
using static SC_MenuController;

public class SC_MenuLogic : MonoBehaviour
{
   
    #region Variables
    public enum Screens
    {
        MainMenu, Loading, Options, StudentInfo, Multiplayer, Previous
    };
    private Stack<Screens> ScreensStack;
    #endregion

    #region MonoBehaviour
    void OnEnable()
    {
        SC_MenuController.OnChangeScreen += OnChangeScreen;
        SC_MenuController.OnOpenLink += OnOpenLink;
        SC_MenuController.OnAdjustVolume += OnAdjustVolume;
    }
    void OnDisable()
    {
        SC_MenuController.OnChangeScreen -= OnChangeScreen;
        SC_MenuController.OnOpenLink -= OnOpenLink;
        SC_MenuController.OnAdjustVolume -= OnAdjustVolume;
    }
    void Awake()
    {
        InitVariables();
    }
    void Start()
    {
        InitLogic();
    }
    #endregion

    #region Logic
    private void InitVariables()
    {
        ScreensStack = new Stack<Screens>();
        ScreensStack.Push(Screens.MainMenu);
    }
    private void InitLogic()
    {
        SC_GameData.Instance.UnityObjects.ForEach((obj) =>
        {
            if (obj.name != "Screen_MainMenu") { obj.SetActive(false); }
        });
    }
    private Screens ParseScreens(string _screenStr)
    {
        try
        {
            return (Screens)Enum.Parse(typeof(Screens), _screenStr);
        }
        catch (Exception e)
        {
            Debug.LogError("Fail to convert: " + e.ToString());
            return ScreensStack.Pop();
        }
    }
    #endregion

    #region Events
    private void OnChangeScreen(string _screen)
    {
        Screens _toScreen = ParseScreens(_screen);

        if (SC_GameData.Instance.GetUnityObject("Screen_" + ScreensStack.Peek()) != null)
            SC_GameData.Instance.GetUnityObject("Screen_" + ScreensStack.Peek()).SetActive(false);

        if (_toScreen == Screens.Previous)
        {
            if (ScreensStack.Count > 1) { ScreensStack.Pop(); }
        }
        else { ScreensStack.Push(_toScreen); }

        if (SC_GameData.Instance.GetUnityObject("Screen_" + ScreensStack.Peek()) != null)
            SC_GameData.Instance.GetUnityObject("Screen_" + ScreensStack.Peek()).SetActive(true);
    }

    private void OnOpenLink(string url)
    {
        // should validate the url to avoid security risk 
        Application.OpenURL(url);
    }
    private void OnAdjustVolume(float newVolume)
    {
        GameObject _camera = GameObject.Find("Main Camera");
        if (_camera == null) { 
            Debug.LogError("Main Camera game object is null"); 
            return; 
        }
        AudioSource _audioSource = _camera.GetComponent<AudioSource>();
        if (_audioSource == null) { 
            Debug.LogError("Audio Source component is null"); 
            return; 
        }
        _audioSource.volume = newVolume;
    }
    #endregion
}
