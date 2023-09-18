using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SC_MenuController : MonoBehaviour
{
    #region Delegates
    public delegate void StartGame();
    public static StartGame OnStartGame;

    public delegate void ChangeScreen(string _screen);
    public static ChangeScreen OnChangeScreen;

    public delegate void OpenLink(string _url);
    public static OpenLink OnOpenLink;

    public delegate void AdjustVolume(float _volume);
    public static AdjustVolume OnAdjustVolume;
    #endregion

    #region Logic
    public void Btn_StartGame()
    {
        if (OnStartGame != null) { OnStartGame(); }
    }
    public void Btn_ChangeScreen(string _ScreenName)
    {
        if (OnChangeScreen != null) { OnChangeScreen(_ScreenName); }
    }
    public void Btn_OpenLink(string url)
    {
        if (OnOpenLink != null) { OnOpenLink(url); }
    }
    private float GetSliderValue(string _sliderName)
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
    public void Slider_Multiplayer(string _sliderName)
    {
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
        _myText.text = (GetSliderValue(_sliderName)).ToString();
    }
    #endregion
}
