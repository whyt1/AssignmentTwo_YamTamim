using AssemblyCSharp;
using com.shephertz.app42.gaming.multiplayer.client;
using com.shephertz.app42.gaming.multiplayer.client.events;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UI.GridLayoutGroup;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class SC_MultiplayerLogic : MonoBehaviour
{
    #region API Keys
    private readonly string apiKey = "7a4d7ab6dd01a049d4153354dfe6b262e328f6c0f4b5b63cfda7a4898b993adf";
    private readonly string secretKey = "72870153452ab88beae898d3d3331814f579f8695eb9d56d8a4cec746d0b6a69";
    #endregion

    #region Variables

    private Listener listener;
    private List<string> roomsIds;
    private int roomIndex;
    private Dictionary<string, object> passedParams;
    private Dictionary<string, object> roomProps;
    [SerializeField]
    private int maxPlayers;
    [SerializeField]
    private int turnTime;
    [SerializeField]
    private string roomName;
    [SerializeField]
    private string roomId;
    [SerializeField]
    private int playersInRoom;
    [SerializeField]
    private List<User> users;
    private User User { get => SC_GameData.Instance.user; }
    WarpClient Client { get => WarpClient.GetInstance(); }
    #endregion

    #region Delegates

    public delegate void ChangeButton(UnityAction clickOnAction = null, string newText = null);
    public static event ChangeButton OnChangeButton;

    public delegate void JoinRoomSuccess();
    public static event JoinRoomSuccess OnJoinRoomSuccess;

    #endregion

    #region MonoBehaviour
    void OnEnable()
    {
        Listener.OnConnect += OnConnect;
        Listener.OnRoomsInRange += OnRoomsInRange;
        Listener.OnGetLiveRoomInfo += OnGetLiveRoomInfo;
        Listener.OnCreateRoom += OnCreateRoom;
        Listener.OnJoinRoom += OnJoinRoom;
        Listener.OnUserJoinRoom += OnUserJoinRoom;
        Listener.OnGameStarted += OnGameStarted;

        SC_MenuController.OnChangeScreen += OnChangeScreen;
        SC_MenuController.OnJoin += OnJoin;
        SC_MenuController.OnAdjustPassword += OnAdjustPassword;
        SC_MenuController.OnChooseAvatar += OnChooseAvatar;
        SC_MenuController.OnInputName += OnInputName;
        SC_MenuController.OnSetPlayers += OnSetPlayers;
        SC_MenuController.OnSetTurnTimer += OnSetTurnTimer;
        SC_MenuController.OnSetHandSize += OnSetHandSize;
        SC_MenuController.OnCreate += OnCreate;
        SC_MenuController.OnKeepSearching += OnKeepSearching;
    }

    void OnDisable()
    {
        Listener.OnConnect -= OnConnect;
        Listener.OnRoomsInRange -= OnRoomsInRange;
        Listener.OnGetLiveRoomInfo -= OnGetLiveRoomInfo;
        Listener.OnCreateRoom -= OnCreateRoom;
        Listener.OnJoinRoom -= OnJoinRoom;
        Listener.OnUserJoinRoom -= OnUserJoinRoom;
        Listener.OnGameStarted -= OnGameStarted;

        SC_MenuController.OnChangeScreen -= OnChangeScreen;
        SC_MenuController.OnJoin -= OnJoin;
        SC_MenuController.OnAdjustPassword -= OnAdjustPassword;
        SC_MenuController.OnChooseAvatar -= OnChooseAvatar;
        SC_MenuController.OnInputName -= OnInputName;
        SC_MenuController.OnSetPlayers -= OnSetPlayers;
        SC_MenuController.OnSetTurnTimer -= OnSetTurnTimer;
        SC_MenuController.OnSetHandSize -= OnSetHandSize;
        SC_MenuController.OnCreate -= OnCreate;
        SC_MenuController.OnKeepSearching -= OnKeepSearching;
    }

    void Awake()
    {
        InitVariables();
    }
    #endregion

    #region Logic
    private void InitVariables()
    {
        turnTime = 30;
        maxPlayers = 5;
        playersInRoom = 0;
        users = new();
        passedParams = new Dictionary<string, object>()
        {
            { "Password", "4" },
            { "NumberOfPlayers", "4" },
            { "StartingHandSize", "6" }
        };

        listener = new Listener();
        WarpClient.initialize(apiKey, secretKey);
        Client.AddConnectionRequestListener(listener);
        Client.AddChatRequestListener(listener);
        Client.AddUpdateRequestListener(listener);
        Client.AddLobbyRequestListener(listener);
        Client.AddNotificationListener(listener);
        Client.AddRoomRequestListener(listener);  
        Client.AddTurnBasedRoomRequestListener(listener);
        Client.AddZoneRequestListener(listener);

    }

    private void DoRoomSearch()
    {
        if (roomIndex >= roomsIds.Count) {
            UpdateText("Searching", "no rooms avaliable");
            if (SC_GameData.Instance.GetUnityObject("CreateRoom") != null) {
                SC_GameData.Instance.GetUnityObject("CreateRoom").SetActive(true);
            }
            return;
        }
        UpdateText("Searching", "Searching for rooms...");
        Client.GetLiveRoomInfo(roomsIds[roomIndex]);
    }

    private void FoundRoom(LiveRoomInfoEvent eventObj)
    {
        UpdateText("Searching", "found room");
        // getting users data
        foreach (string userdata in eventObj.getJoinedUsers())
        {
            Debug.Log(userdata);
            SC_GameLogic.Instance.AddUser(User.FromString(userdata));
            playersInRoom++;
        }
        // getting room data
        roomId = eventObj.getData().getId();
        string _ownerData = eventObj.getData().getRoomOwner();
        User _owner = User.FromString(_ownerData);
        roomProps = eventObj.getProperties();
        // updating ui, maybe should be in view manager?
        if (SC_GameData.Instance.GetUnityObject("JoinRoom") != null) {
            SC_GameData.Instance.GetUnityObject("JoinRoom").SetActive(true);
        }
        int.TryParse((string)roomProps["NumberOfPlayers"], out int _players); 
        UpdateText("RoomInfo", $"Owner: {_owner.Name}\nPlayers: {playersInRoom} / {_players}");
        if (SC_GameData.Instance.GetUnityObject("Btn_Join") != null) {
            SC_GameData.Instance.GetUnityObject("Btn_Join").SetActive(playersInRoom < _players);
        }
    }

    private void HandleMismatch(string errorMessege)
    {
        UpdateText("Searching", "Searching for rooms..");
        Debug.Log(errorMessege);
        roomIndex++;
        DoRoomSearch();
    }

    // should be in view manager
    private void UpdateText(string GameObjectName, string newText)
    {
        GameObject TxtGameObject = SC_GameData.Instance.GetUnityObject("Txt_" + GameObjectName);
        if (TxtGameObject != null)
        {
            if (!TxtGameObject.activeSelf) { TxtGameObject.SetActive(true); }
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

    // in menu
    private void OnConnect(bool _IsSuccess)
    {
        if (!_IsSuccess)
        {
            UpdateText("Searching", "Failed to connect");
            return;
        }

        UpdateText("Searching", "Searching for rooms.");
        Client.GetRoomsInRange(1, maxPlayers);
    }

    private void OnRoomsInRange(bool _IsSuccess, MatchedRoomsEvent eventObj)
    {
        if (!_IsSuccess)
        {
            UpdateText("Searching", "Failed to Search Rooms");
            return;
        }

        UpdateText("Searching", "Searching for rooms..");
        roomsIds = new List<string>();
        foreach (var roomData in eventObj.getRoomsData())
        {
            Debug.Log($"Room id: {roomData.getId()}\nRoom owner: {roomData.getRoomOwner()}\n");
            roomsIds.Add(roomData.getId());
        }
        roomIndex = 0;
        DoRoomSearch();
    }

    private void OnGetLiveRoomInfo(LiveRoomInfoEvent eventObj)
    {
        // UpdateText("status", "Status: Parsing Room Info...");
        if (eventObj == null || eventObj.getProperties() == null) {
            HandleMismatch($"Failed to Parse info for room {roomsIds[roomIndex]}");
            return;
        }
        var _props = eventObj.getProperties();
        if (!_props.ContainsKey("Password"))
        {
            HandleMismatch($"Room {roomsIds[roomIndex]} does not contain password definition.");
            return;
        }
        if (!Equals(_props["Password"].ToString(), passedParams["Password"].ToString()))
        {
            HandleMismatch($"Room {roomsIds[roomIndex]} password does not match.");
            return;
        }

        FoundRoom(eventObj);
    }

    private void OnCreateRoom(bool _IsSuccess, string _RoomId)
    {
        if (!_IsSuccess)
        {
            // need to give option to try again
            UpdateText("Searching", "Failed to Create Room");
            return;
        }
        UpdateText("Searching", "Created Room");
        roomId = _RoomId;
        playersInRoom = 0;
        if (SC_GameData.Instance.GetUnityObject("CreateRoom") != null) {
            SC_GameData.Instance.GetUnityObject("CreateRoom").SetActive(false);
        }
        if (SC_GameData.Instance.GetUnityObject("JoinRoom") != null) {
            SC_GameData.Instance.GetUnityObject("JoinRoom").SetActive(true);
        }
        roomProps = passedParams;
        UpdateText("RoomInfo", $"Owner: {User.Name}\nPlayers: {playersInRoom} / {passedParams["NumberOfPlayers"]}");
    }

    // in game
    private void OnJoinRoom(bool _IsSuccess, string _RoomId)
    {
        if (!_IsSuccess)
        {
            UpdateText("Searching", $"Failed to join room");
            return;
        }
        if (OnJoinRoomSuccess != null) { OnJoinRoomSuccess(); }
        playersInRoom++; // count yourself in the players
        SC_GameLog.Instance.AddMessege(User.Name + " Joined Room");
        int.TryParse((string)roomProps["NumberOfPlayers"], out int _players);
        string msg = playersInRoom < _players ? $"Wait for Players\n ({playersInRoom} / {_players})" 
                                              : "Wait for host to start";
        if (OnChangeButton != null) { OnChangeButton(null, msg); }
    }

    private void OnUserJoinRoom(RoomData eventObj, string _UserName)
    {
        if (eventObj == null || User.ToString() == _UserName) {
            return;
        }
        User _joinedUser = User.FromString(_UserName);
        SC_GameLog.Instance.AddMessege(_joinedUser.Name + " Joined Room");
        SC_GameLogic.Instance.AddUser(_joinedUser);
        playersInRoom++;
        int.TryParse((string)roomProps["NumberOfPlayers"], out int _players);
        if (playersInRoom < _players) {
            if (OnChangeButton != null) {
                OnChangeButton(null, $"Wait for players\n ({playersInRoom} / {_players})");
            }
            return;
        }
        if (eventObj.getRoomOwner() != User.ToString()) {
            if (OnChangeButton != null) {
                OnChangeButton(null, "Wait for host to start");
            }
            return;
        }
        if (OnChangeButton != null) {
            OnChangeButton(Client.startGame, "Click To Start");
        }
    } 

    private void OnGameStarted(string _Sender, string _RoomId, string _NextTurn)
    {
        SC_GameLog.Instance.AddMessege("game started! "+ User.FromString(_NextTurn).Name + " Turn start");
    }

    #endregion

    #region UI Input Events

    // user settings
    private void OnInputName(string _name)
    {
        Debug.Log("New Name: " + _name);
        User.Name = _name;
        UpdateText("Name", _name);
    }
    private void OnChooseAvatar(int _avatar)
    {
        Debug.Log("New Avatar: " + _avatar);
        User.Avatar = _avatar;
    }
    // room settings
    private void OnAdjustPassword(int _password)
    {
        Debug.Log("New password: "+ _password);
        passedParams["Password"] = _password.ToString();
    }
    private void OnSetPlayers(int _players)
    {
        Debug.Log("New Players: " + _players);
        passedParams["NumberOfPlayers"] = _players;
    }
    private void OnSetHandSize(int _handSize)
    {
        Debug.Log("New Hand Size: " + _handSize);
        passedParams["StartingHandSize"] = _handSize.ToString();
    }
    private void OnSetTurnTimer(int _turnTimer)
    {
        Debug.Log("New Turn Timer: " + _turnTimer);
        turnTime = _turnTimer;
    }
    // navigation
    private void OnChangeScreen(string _screen)
    {
        if (_screen == "Searching")
        { 
            Debug.Log("Connecting with userID: " + User.ToString());
            Client.Connect(User.ToString());
            if (SC_GameData.Instance.GetUnityObject("Txt_Searching") != null) {
                SC_GameData.Instance.GetUnityObject("Txt_Searching").SetActive(true);
            }
            UpdateText("Searching", "Connecting to server...");
        }
        if (_screen == "Previous")
        {
            Client.Disconnect();
            if (SC_GameData.Instance.GetUnityObject("JoinRoom") != null) {
                SC_GameData.Instance.GetUnityObject("JoinRoom").SetActive(false);
            }
            if (SC_GameData.Instance.GetUnityObject("CreateRoom") != null) {
                SC_GameData.Instance.GetUnityObject("CreateRoom").SetActive(false);
            }
        }
    }
    private void OnJoin()
    {
        Client.JoinRoom(roomId);
        Client.SubscribeRoom(roomId);
    }
    private void OnCreate()
    {
        roomName = Random.Range(1000, 9999).ToString();
        Client.CreateTurnRoom(roomName, User.ToString(), maxPlayers, passedParams, turnTime);     
    }
    private void OnKeepSearching()
    {
        roomIndex++;
        DoRoomSearch();
    }

    #endregion
}
