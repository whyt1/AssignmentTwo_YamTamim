using AssemblyCSharp;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SC_MenuLogic : MonoBehaviour
{
   
    #region Variables
    public enum Screens
    {
        MainMenu, Searching, Options, StudentInfo, Multiplayer, SinglePlayer, Previous, Game
    };
    private Stack<Screens> ScreensStack;
    #endregion

    #region MonoBehaviour
    void OnEnable()
    {
        SC_MultiplayerLogic.OnJoinRoomSuccess += OnJoinRoomSuccess;
        SC_MenuController.OnChangeScreen += OnChangeScreen;
        SC_MenuController.OnOpenLink += OnOpenLink;
        SC_MenuController.OnAdjustVolume += OnAdjustVolume;
        SC_MenuController.OnChooseAvatar += OnChooseAvatar;
    }

    void OnDisable()
    {
        SC_MultiplayerLogic.OnJoinRoomSuccess -= OnJoinRoomSuccess;
        SC_MenuController.OnChangeScreen -= OnChangeScreen;
        SC_MenuController.OnOpenLink -= OnOpenLink;
        SC_MenuController.OnAdjustVolume -= OnAdjustVolume;
        SC_MenuController.OnChooseAvatar -= OnChooseAvatar;
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
            return (Screens)System.Enum.Parse(typeof(Screens), _screenStr);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Fail to convert: " + e.ToString());
            return ScreensStack.Pop();
        }
    }
    #endregion

    #region Events 
    
    private void OnJoinRoomSuccess()
    {
        OnChangeScreen("Game");
    }

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

        // should be in view managar
        if (_toScreen == Screens.Multiplayer)
        {
            GameObject _char1 = SC_GameData.Instance.GetUnityObject("Sprite_Character_Searching");
            GameObject _char = SC_GameData.Instance.GetUnityObject("Sprite_Character");
            if (_char == null || _char1 == null)
            {
                Debug.LogError("Sprite_Character game object is null");
                return;
            }
            _char.SetActive(true);
            _char1.SetActive(true);
            Image _image1 = _char1.GetComponent<Image>();
            Image _image = _char.GetComponent<Image>();
            if (_image == null || _image1 == null)
            {
                Debug.LogError("Sprite_Character SpriteRenderer component is null");
                return;
            }
            int currSprite = SC_GameData.Instance.user.Avatar;
            _image.sprite = SC_GameData.Instance.GetCharacterSprite(currSprite);
            _image1.sprite = SC_GameData.Instance.GetCharacterSprite(currSprite);
        }

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

    // should be in view managar
    private void OnChooseAvatar(int _newSprite)
    {
        GameObject _char1 = SC_GameData.Instance.GetUnityObject("Sprite_Character_Searching");
        GameObject _char2 = SC_GameData.Instance.GetUnityObject("Sprite_Character");
        if (_char1 == null || _char2 == null) {
            Debug.LogError("Sprite_Character game object is null");
            return;
        }
        _char1.SetActive(true);
        Image _image1 = _char1.GetComponent<Image>();
        Image _image2 = _char2.GetComponent<Image>(); 
        if (_image1 == null || _image2 == null)  {
            Debug.LogError("Sprite_Character SpriteRenderer component is null or invalid sprite name");
            return;
        }
        _image1.sprite = SC_GameData.Instance.GetCharacterSprite(_newSprite);
        _image2.sprite = SC_GameData.Instance.GetCharacterSprite(_newSprite);
    }
    #endregion
}
