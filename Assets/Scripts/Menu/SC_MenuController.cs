using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SC_MenuController;

public class SC_MenuController : MonoBehaviour
{
    #region Variables
    public enum Sliders
    {
        Slider_Volume, Slider_NumberOfPlayers, Slider_TurnTimer, Slider_HandSize, Slider_Players, Slider_Password
    };
    #endregion

    #region Delegates

    public delegate void Join();
    public static Join OnJoin;

    public delegate void Create();
    public static Create OnCreate;

    public delegate void KeepSearching();
    public static KeepSearching OnKeepSearching;

    public delegate void ChangeScreen(string _screen);
    public static ChangeScreen OnChangeScreen;

    public delegate void OpenLink(string _url);
    public static OpenLink OnOpenLink;

    public delegate void AdjustVolume(float _volume);
    public static AdjustVolume OnAdjustVolume;

    /// <summary>
    /// used for single player
    /// </summary>
    public delegate void AdjustNumberOfPlayers(int _numberOfPlayers);
    public static AdjustNumberOfPlayers OnAdjustNumberOfPlayers;

    public delegate void AdjustPassword(int _password);
    public static AdjustPassword OnAdjustPassword;

    public delegate void ChooseAvatar(int _newSprite);
    public static ChooseAvatar OnChooseAvatar;

    public delegate void InputName(string _name);
    public static InputName OnInputName;

    public delegate void SetTurnTimer(int _turnTimer);
    public static SetTurnTimer OnSetTurnTimer;

    /// <summary>
    /// used for multiplayer
    /// </summary>
    public delegate void SetPlayers(int _players);
    public static SetPlayers OnSetPlayers;

    public delegate void SetHandSize(int _handSize);
    public static SetHandSize OnSetHandSize;

    #endregion

    #region Logic
    public void Btn_Join()
    {
        if (OnJoin != null) { OnJoin(); }
    }
    public void Btn_Create()
    {
        if (OnCreate != null) { OnCreate(); }
    }
    public void Btn_KeepSearching()
    {
        if (OnKeepSearching != null) { OnKeepSearching(); }
    }
    public void Btn_ChangeScreen(string _ScreenName)
    {
        if (OnChangeScreen != null) { OnChangeScreen(_ScreenName); }
    }
    public void Btn_OpenLink(string url)
    {
        if (OnOpenLink != null) { OnOpenLink(url); }
    }
    public float GetSliderValue(string _sliderName)
    {
        GameObject _obj = GameObject.Find(_sliderName);
        if (_obj == null) { 
            Debug.LogError("\"" + _sliderName + "\" game object is null"); 
            return -1; 
        }
        Slider _slider = _obj.GetComponent<Slider>();
        if (_slider == null) { 
            Debug.LogError("\"" + _sliderName + "\" component is null"); 
            return -1; 
        }
        return _slider.value;
    }
    public void Slider_AdjustVolume(string _sliderName)
    {
        float _newVolume = GetSliderValue(_sliderName);
        if (_newVolume >= 0 && OnAdjustVolume != null) { OnAdjustVolume(_newVolume); }

        GameObject _text = GameObject.Find("Txt_" + _sliderName);
        if (_text == null) { 
            Debug.LogError("\"Txt_" + _sliderName + "\" game object is null"); 
            return; 
        }
        TextMeshProUGUI _myText = _text.GetComponent<TextMeshProUGUI>();
        if (_myText == null) { 
            Debug.LogError("\"Txt_" + _sliderName + "\" component is null"); 
            return; 
        }

        _myText.text = ((int)(_newVolume*10)).ToString();
    }
    public void Slider_int(string _sliderName)
    {
        int _value = (int)GetSliderValue(_sliderName);
        if (_value == -1 || !Enum.TryParse(_sliderName, out Sliders _slider)) {
            Debug.LogError(_sliderName + " value cannot be negative");
            return;
        } 
        
        if (_slider == Sliders.Slider_NumberOfPlayers && 
            OnAdjustNumberOfPlayers != null) { OnAdjustNumberOfPlayers(_value); }
        if (_slider == Sliders.Slider_Password &&
            OnAdjustPassword != null) { OnAdjustPassword(_value); }
        if (_slider == Sliders.Slider_TurnTimer &&
            OnSetTurnTimer != null) { OnSetTurnTimer(_value); }
        if (_slider == Sliders.Slider_Players &&
            OnSetPlayers != null) { OnSetPlayers(_value); }
        if (_slider == Sliders.Slider_HandSize &&
            OnSetHandSize != null) { OnSetHandSize(_value); }

        // should be in view manager
        GameObject _text = GameObject.Find("Txt_" + _sliderName);
        if (_text == null) { 
            Debug.LogError("\"Txt_" + _sliderName + "\" game object is null"); 
            return; 
        }
        TextMeshProUGUI _myText = _text.GetComponent<TextMeshProUGUI>();
        if (_myText == null) { 
            Debug.LogError("\"Txt_" + _sliderName + "\" component is null"); 
            return; 
        }
        _myText.text = _value.ToString();
    }

    public void Btn_ChooseAvatar(int _direction)
    {
        int currSprite = SC_GameData.Instance.user.Avatar;
        int m = SC_GameData.Instance.NumberOfChars;
        // using mod trick to cicle sprites while keeping the value between 1 to m
        int newSprite = (((currSprite + _direction) % m) + m) % m;
        newSprite = newSprite == 0 ? m : newSprite;
        if (OnChooseAvatar != null)  { OnChooseAvatar(newSprite); }
    }
    public void Input_Name(string _name)
    {
        if (OnInputName != null) { OnInputName(_name); }
    }
    #endregion
}
