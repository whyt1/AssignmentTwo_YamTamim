using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SC_GameData : MonoBehaviour
{
    #region Variables

    /// <summary>
    /// Holds refrences to all card sprites.
    /// <para>
    /// <see cref="SC_Card"/> Used by cards when setting their sprite.
    /// </para>
    /// </summary>
    private Dictionary<string, Sprite> cardSprites;

    /// <summary>
    /// Holds refrences to all game objects marked with the unity object tag
    /// <para>
    /// <see cref="SC_MenuLogic"/> Used by the menu to show / hide objects as needed.
    /// </para>
    /// <seealso cref="GetUnityObject(string)"/>
    /// </summary>
    private Dictionary<string, GameObject> unityObjects;

    /// <summary>
    /// 
    /// </summary>
    private Dictionary<Containers, CardContainer> gameWorld;

    public Vector2 screenSize;

    #endregion

    #region Singleton
    private static SC_GameData instance;
    public static SC_GameData Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject _gameData = GameObject.FindGameObjectWithTag("GameData");
                if (_gameData == null) { 
                    _gameData = new GameObject("SC_GameData", typeof(SC_GameData));
                }

                instance = _gameData.InitComponent<SC_GameData>();
            }
            return instance;
        }
    }
    #endregion

    #region API

    public List<GameObject> UnityObjects { get => new(unityObjects.Values); }

    public List<string> CardNames { get; private set; }

    /// <summary>
    /// Get unity object from unity objects dict (<see cref="unityObjects"/>)
    /// <para></para>
    /// <paramref name="_objName"/> must be tagged "UnityObjects" in unity editor.
    /// </summary>
    /// <returns>The Unity Object coresponding to the name.</returns>
    public GameObject GetUnityObject(string _objName)
    {
        if (unityObjects == null) { 
            Debug.LogError("Failed to Get Unity Object! unityObjects dict is not initialized."); 
            return null; 
        }
        if (unityObjects.ContainsKey(_objName))
            return unityObjects[_objName];
        return null;
    }

    /// <summary>
    /// Get card Sprite from preloaded card sprites dict (<see cref="cardSprites"/>)
    /// <para></para>
    /// <paramref name="_name"/> must contain prefix "Card_" or "Sprite_".
    /// </summary>
    /// <returns>The sprite coresponding to the Sprite or Card name.</returns>
    public Sprite GetCardSprite(string _name)
    {
        if (cardSprites == null) { 
            Debug.LogError("Failed to Get Card Sprite! cardSprite dict is not initialized.");
            return null;
        }
        if (_name == null) { return null; }
        _name = _name.StartsWith('S') ? _name : _name.Replace("Card_", "Sprite_");
        if (cardSprites.ContainsKey(_name))
            return cardSprites[_name];
        return null;
    }

    /// <summary>
    /// Get unity object representing Container from <see cref="gameWorld"/>
    /// <para></para>
    /// <paramref name="Container"/> the Container in the game world.
    /// </summary>
    /// <returns>The unity object coresponding to the Container.</returns>
    public CardContainer GetContainer(Containers Container) 
    {
        if (gameWorld.ContainsKey(Container)) {
            return gameWorld[Container]; 
        }
        Debug.LogError($"{Container} Container not found in GameWorld");
        return null;
    }


    #endregion

    #region MonoBehaviour

    void Awake()
    {
        screenSize = new(Camera.main.aspect * Camera.main.orthographicSize, Camera.main.orthographicSize);
        InitGameWorld();
        InitUnityObjects();
        InitCardsData();
    }

    void Update()
    {
        // screenSize = new(Camera.main.aspect * Camera.main.orthographicSize, Camera.main.orthographicSize);
    }

    #endregion

    #region Logic

    private void InitUnityObjects()
    {
        unityObjects = new();
        GameObject[] _ = GameObject.FindGameObjectsWithTag("UnityObjects");
        foreach (GameObject obj in _)
        {
            if (unityObjects.ContainsKey(obj.name))
            {
                Debug.LogError("Duplicate unity object");
            }
            else unityObjects.Add(obj.name, obj);
        }
    }

    private void InitCardsData()
    {
        cardSprites = new();
        CardNames = new();
        Sprite[] _temp = Resources.LoadAll<Sprite>("Sprites/Cards");
        foreach (Sprite s in _temp)
        {
            if (s.name == null) { continue; }
            if (cardSprites.ContainsKey(s.name)) {
                Debug.LogError("Duplicate card sprite");
            }
            else { cardSprites.Add(s.name, s); }

            if (s.name.Contains("cardback")) { continue; }
            CardNames.Add(s.name.Replace("Sprite_", "Card_"));
        };
    }

    private void InitGameWorld()
    {
        gameWorld = new();
        GameObject[] _ = GameObject.FindGameObjectsWithTag("Containers");
        foreach (GameObject obj in _)
        {
            CardContainer Container;
            if (!Enum.TryParse(obj.name, true, out Containers key))
            {
                Debug.LogError($"Failed to Initialize game world! couldnt parse container name: {obj.name}");
                return;
            }
            if (gameWorld.ContainsKey(key))
            {
                Debug.LogError("Duplicate game world card container");
                return;
            }
            switch (key)
            {
                case Containers.Deck:
                    Container = obj.InitComponent<SC_Deck>();
                    break;

                default:
                    Debug.LogError($"Failed to Initialize game world! could not find component type for: {obj.name}");
                    return;
            }
            
            gameWorld.Add(key, Container);
        }
    }
    
    #endregion
}
    /*
    /// <summary>
    /// Adds a new Container to the game world dict <see cref="gameWorld"/>.
    /// <para></para>
    /// <see cref="InitGameWorld"/> Used to initilazie the game world.
    /// </summary>
    private void AddContainer<T>(Containers conEnum) where T : CardContainer
    {
        GameObject locObj = new(conEnum.ToString(), typeof(T));
        T locLinkedList = locObj.InitComponent<T>() as T;
        if (locLinkedList == null) {
            Debug.LogError($"Failed to Add Container!");
            return;
        }
        if (gameWorld.ContainsKey(conEnum)) {
            Debug.LogError("Duplicate Containers");
            return;
        }
        else gameWorld.Add(conEnum, locLinkedList);
    }
    
    
}
*/