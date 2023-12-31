using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public class SC_GameLogic : MonoBehaviour
{
    #region Variables

    public int attackStack;
    [SerializeField]
    private int startingHandSize;
    [SerializeField]
    public int numberOfPlayers;
    [SerializeField]
    public GameStates currentState;
    public Containers currentPlayer;
    public List<User> users;
    public List<Containers> players;
    public List<GameObject> characters;
    public List<GameObject> gameOverCharacters;
    private float otherPlayersDelay;
    public bool isGiveDone;
    private bool isInitalized;
    private int backgroundSortOrder;
    #endregion

    #region Singleton
    private static SC_GameLogic instance;


    public static SC_GameLogic Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject _gameLogic = GameObject.FindGameObjectWithTag("GameLogic");
                if (_gameLogic == null)
                {
                    _gameLogic = new GameObject("SC_GameLogic", typeof(SC_GameLogic));
                }

                instance = _gameLogic.InitComponent<SC_GameLogic>();
            }
            return instance;
        }
    }
    #endregion

    #region MonoBehaviour
    void OnEnable()
    {
        SC_MenuController.OnAdjustNumberOfPlayers += OnAdjustNumberOfPlayers;
    }
    void OnDisable()
    { 
        SC_MenuController.OnAdjustNumberOfPlayers -= OnAdjustNumberOfPlayers;
    }

    void Start()
    {
        InitVariables();
    }

    #endregion

    #region Events

    private void OnAdjustNumberOfPlayers(int _numberOfPlayers)
    {
        numberOfPlayers = _numberOfPlayers;
    }

    #endregion

    #region Logic

    private void InitVariables()
    {
        backgroundSortOrder = -1;
        attackStack = 0;
        otherPlayersDelay = 1;
        startingHandSize = 6;
        users = new();
        InitPlayers();
        InitCardTypes();
    }

    public void AddUser(User _user)
    {
        if (_user == null)
        {
            Debug.LogError("Failed to add user! user is null.");
            return;
        }
        if (users == null || users.Count == 0) {
            users = new()
            {
                SC_GameData.Instance.user
            };
            buildCharacter(SC_GameData.Instance.user.Avatar, SC_GameData.Instance.user.Name);
            if (_user.ToString().Equals(SC_GameData.Instance.user.ToString())) {
                return;
            }
        }
        users.Add(_user);
        numberOfPlayers = users.Count;
        if (characters == null || characters.Count == 0) { 
            characters = new();
        }
        buildCharacter(_user.Avatar, _user.Name);
        PositionPlayers();
    }

    private void InitPlayers()
    {
        numberOfPlayers = 3;
        players = new List<Containers> { Containers.PlayerHand, Containers.OpponentHand1, Containers.OpponentHand2, Containers.OpponentHand3, Containers.OpponentHand4, };
        currentPlayer = players[0];
    }
    private void InitCharacters()
    {
        // characters already initialized
        if (characters != null && characters.Count != 0) { return; }

        List<Sprite> _sprites = new List<Sprite>();
        for (int i = 1; i < SC_GameData.Instance.NumberOfChars; i++) {
            Sprite _sprite = SC_GameData.Instance.GetCharacterSprite(i);
            if (_sprite != null) {  _sprites.Add(_sprite); }
        }
        for (int i = _sprites.Count - 1; i >= 0; i--) {
            int j = UnityEngine.Random.Range(0, i + 1);
            (_sprites[i], _sprites[j]) = (_sprites[j], _sprites[i]);
        }
        if (_sprites.Count < numberOfPlayers) {
            Debug.LogError($"Failed to init characters! not enough char sprites for players. \n" +
                           $"{_sprites.Count} < {numberOfPlayers}");
            return;
        }

        gameOverCharacters = new();
        characters = new();
        GameObject _characters = SC_GameData.Instance.GetUnityObject("Characters");
        if (_characters != null) { _characters.SetActive(true); } 
        for (int i = 0; i < numberOfPlayers; i++)
        {
            buildCharacter(i);
        }
    }

    private void buildCharacter(int _spriteIndex, string _name = null)
    {
        GameObject _characters = SC_GameData.Instance.GetUnityObject("Characters");
        if (_characters != null && !_characters.activeSelf) { _characters.SetActive(true); }

        Sprite _sprite = SC_GameData.Instance.GetCharacterSprite(_spriteIndex);
        if (_sprite == null) {
            Debug.LogError("Failed to build Character!, couldn't get sprite for index: " + _spriteIndex);
            return;
        }

        GameObject _char = new(_sprite.name, typeof(SpriteRenderer));
        if (name == null) { return; }
        GameObject _charName = Instantiate(Resources.Load<GameObject>("Prefabs/Name_UserName"));
        _charName.name = "Txt_" + _name;
        TextMeshPro _text = _charName.GetComponent<TextMeshPro>();
        _text.text = _name;

        _charName.transform.SetParent(_char.transform);
        SpriteRenderer _renderer = _char.GetComponent<SpriteRenderer>();
        _renderer.sprite = _sprite;
        _renderer.sortingOrder = backgroundSortOrder;

        _char.transform.SetParent(_characters != null ? _characters.transform : null);
        _char.transform.localPosition = SC_GameData.Instance.screenSize + Vector2.one;
        // _char.transform.GetChild(0).localPosition = new(0, 0.75f);
        characters.Add(_char); 

    }

    private void InitCardTypes()
    {
        GameObject cardTypes = SC_GameData.Instance.GetUnityObject("CardTypes");
        if (cardTypes == null)
        {
            Debug.LogError("Failed to Steal Spesific card! card types is null.");
            return;
        }
        cardTypes.SetActive(false);
    }

    /// <summary>
    /// Used when starting a new game to clear all the cards from all containers.
    /// </summary>
    private void ClearGameWorld()
    {
        foreach (GameObject _char in characters.Concat(gameOverCharacters))
        {
            Destroy(_char.gameObject);
        }
        characters = null; gameOverCharacters = null;

        foreach (CardContainer _container in SC_GameData.Instance.GameWorld)
        {
            _container.Clear();
            _container.InitVariables();
        }
        
    }

    /// <summary>
    /// Used when starting a new game to position the player and opponent on the screen based on how many players there are.
    /// </summary>
    private void PositionPlayers()
    {
        for (int i = 1; i < numberOfPlayers; i++)
        {
            CardContainer hand = SC_GameData.Instance.GetContainer(players[i]);
            if (hand == null) {
                Debug.LogError("Failed to postion players! player or opponent hand is null in game logic");
                return;
            }
            hand.PositionPlayer(numberOfPlayers);
            if (i != 0) { PositionCharacters(characters[i], hand); }
        }
    }

    /// <summary>
    /// used to add the cats above each opponent hand, the player hand cat is turned off.
    /// </summary>
    private void PositionCharacters(GameObject character, CardContainer hand)
    {
        if (!int.TryParse(character.name.Split('_')[2], out int charIndex)) {
            Debug.LogError($"Failed to get character index! character name: {character.name}");
            return;
        }
        if ((hand.CS.Origin.x > 0 && charIndex % 2 == 1) || hand.CS.Origin.x <=0 && charIndex % 2 == 0)
        {
            character.transform.localScale = new(-1,1,1);
            character.transform.GetChild(0).localScale = new(-Mathf.Abs(character.transform.GetChild(0).localScale.x), 
                                                              character.transform.GetChild(0).localScale.y,
                                                              character.transform.GetChild(0).localScale.z);
            
        }
        character.transform.position = hand.CS.Origin + Vector2.up + (hand.CS.Origin.x > 0 ? Vector2.right : Vector2.zero);
        character.transform.GetChild(0).localPosition = new(0, 0.75f); // remove magic numbers
    }


    /// <summary>
    /// Sets up and starts a new game.
    /// </summary>
    public void OnStartGame()
    {
        // ToggleButton("Btn_StartGame");
        if (isInitalized)
        {
            ClearGameWorld();
            InitVariables();
        }
        InitCharacters();
        PositionPlayers();
        SC_Deck deck = SC_GameData.Instance.GetContainer(Containers.Deck) as SC_Deck;
        if (deck == null)
        {
            Debug.LogError("Failed to Start Game! deck is null in game logic");
            return;
        }

        deck.PopulateDeck();
        deck.Shuffle();
        deck.AddDefuses();
        deck.DealCards(startingHandSize, numberOfPlayers);
        deck.AddExploding(numberOfPlayers);

        foreach (CardContainer container in SC_GameData.Instance.GameWorld)
        {
            container.Shuffle();
            container.ResetContainer();
        }
        currentPlayer = Containers.PlayerHand;
        ChangeState(GameStates.MyPlayOrDraw);
        isInitalized = true;
        SC_GameLog.Instance.AddMessege("New Game Started");
    }

    #endregion

    #region Game State

    public static Action OnYouWin;

    public delegate void StateTransition(GameStates newState);
    public static event StateTransition OnStateTransition;
    public void ChangeState(GameStates newState)
    {
        // if player is attacked cancel next turn button and repeat turn
        if (newState == GameStates.MyEndTurn && attackStack != 0) {
            attackStack--; 
            if (attackStack < 0) { attackStack = 0; }
            ChangeState(GameStates.MyPlayOrDraw);
            return;
        }

        // when starting a new turn, check if player won
        if (newState == GameStates.MyPlayOrDraw) {
            // only player left alive
            if (numberOfPlayers == 1)
            {
                SC_GameLog.Instance.AddMessege($"{currentPlayer} WON");
                if (OnYouWin != null) { OnYouWin(); }
                return;
            }
            if (currentState == GameStates.OthersEndTurn)
            {
                SC_GameLog.Instance.AddMessege($"{currentPlayer} Turn start");
            }
            
        }

        Debug.Log($"Changing Game State to {newState}");
        currentState = newState;
        if (OnStateTransition != null) { OnStateTransition(newState); }

    }

    public void OnEndTurn()
    {
        ChangeState(GameStates.OthersPlayOrDraw);
        StartCoroutine(SimulateOtherPlayers());
    }

    #endregion

    #region SinglePlayer Simulations

    public void OnPlayerDeath(CardContainer player)
    {
        if (player == null) { 
            Debug.LogError("Failed to clear player hand and change sprite on death! player is null."); 
            return; 
        }
        attackStack = 0;
        SC_Card card = player.Head; int i = 0;
        while (card != null && i < CardContainer.MaxCapacitiy)
        {
            if (card.Type != CardTypes.Exploding) {
                card.ChangeHome(Containers.Discards);
                card = player.Head;
            }
            else {
                card.IsFaceUp = true;
                card.TargetRotation = new(card.TargetRotation.x,
                                          card.TargetRotation.y,
                                         -card.TargetRotation.z);
                card = card.Prev; 
            }
            i++;
        }
        GameObject character = characters[players.IndexOf(player.CS.Container)];
        if (character == null)
        {
            Debug.LogError("Failed to change sprite on death! character is null.");
            return;
        }
        SpriteRenderer _renderer = character.InitComponent<SpriteRenderer>();
        _renderer.sprite = SC_GameData.Instance.GetCharacterSprite(0);
        players.Remove(player.CS.Container);
        characters.Remove(character);
        gameOverCharacters.Add(character);  
        numberOfPlayers--;
    }

    private IEnumerator SimulateOtherPlayers()
    {
        for (int i = 1; i < numberOfPlayers; i++)
        {
            currentPlayer = players[i];
            SC_GameLog.Instance.AddMessege($"{currentPlayer} Turn start");
            yield return new WaitForSeconds(otherPlayersDelay);
            // null check
            CardContainer _hand = SC_GameData.Instance.GetContainer(currentPlayer);
            if (_hand == null)
            {
                Debug.LogError("Failed to Simulate Other Players! other hand is null");
                yield break;
            }
            SC_Deck _deck = SC_GameData.Instance.GetContainer(Containers.Deck) as SC_Deck;
            if (_deck == null || _deck.head == null) { 
                Debug.LogError("Failed to draw card as other players in singleplayer!");
                yield break;
            }

            // do some action based on cards in hand
            ChangeState(GameStates.OthersTakeAction);
            SC_Card _card = _hand.GetRandomCard(_hand.count);
            if (PlayCard(_card, _deck, _hand))
            {
                continue;
            }
            if (_card.Type == CardTypes.Favor)
            {
                _card.action = new SetUpGive(_card);
                _card.OnClickUp();
                _card.ChangeHome(Containers.Center);
                SC_GameLog.Instance.AddMessege($"{currentPlayer} Played FAVOR on {Containers.PlayerHand}");
                // wait for player to give card before continuing 
                yield return WaitForPlayerGive();
            }
            ChangeState(GameStates.OthersPlayOrDraw);
            // draw a card and end turn or plant the bomb 
            SC_Card _drawnCard = _deck.head;
            _drawnCard.ChangeHome(currentPlayer);
            ChangeState(GameStates.OthersEndTurn);
            SC_GameLog.Instance.AddMessege($"{currentPlayer} Drew a card");
            if (_drawnCard.Type == CardTypes.Exploding)
            {
                SC_GameLog.Instance.AddMessege($"{currentPlayer} EXPOLDED");
                ChangeState(GameStates.OthersDefuse);
                SC_Card _defuse = _hand.GetCard(type: CardTypes.Defuse);
                if (_defuse == null)
                {
                    // game over for current player
                    SC_GameLog.Instance.AddMessege($"Game Over for {currentPlayer}");
                    OnPlayerDeath(_hand);
                    i--;
                }
                else
                {
                    SC_GameLog.Instance.AddMessege($"{currentPlayer} Defused the bomb");
                    // defuse resets attack
                    attackStack = 0;
                    _defuse.ChangeHome(Containers.Discards);
                    _drawnCard.ChangeHome(Containers.Deck); // planting the bomb
                }
            }
            // do the same turn twice
            if (attackStack != 0) { attackStack--; i--; }
        }
        currentPlayer = Containers.PlayerHand;
        ChangeState(GameStates.MyPlayOrDraw);
    }

    // Implement a method to wait for the player's decision event
    private IEnumerator WaitForPlayerGive()
    {
        // Use an event-driven approach to wait for the player's decision.
        // This coroutine should yield until the player's decision event is raised.
        while (!isGiveDone)
        {
            yield return null;
        }

        // Reset the player decision flag for the next iteration
        isGiveDone = false;
    }

    /// <summary>
    /// Simulates another player playing a card based on type, returns true if player skiped draw 
    /// <para></para>
    /// maybe for multiplayer have it set up by card name? 
    /// </summary>
    private bool PlayCard(SC_Card card, SC_Deck _deck, CardContainer _hand)
    {
        switch (card.Type)
        {
            case CardTypes.Seethefuture:
                card.ChangeHome(Containers.Center);
                SC_GameLog.Instance.AddMessege($"{currentPlayer} Played see the future");
                ChangeState(GameStates.OthersPlayOrDraw);
                if (_deck.head.Type == CardTypes.Exploding)
                {
                    ChangeState(GameStates.OthersTakeAction);
                    card = _hand.GetCard(type: CardTypes.Attack);
                    if (card != null) { 
                        card.ChangeHome(Containers.Center);
                        SC_GameLog.Instance.AddMessege($"{currentPlayer} Played attack");
                        attackStack++;
                        return true;
                    }
                    else {
                        card = _hand.GetCard(type: CardTypes.Skip);
                        if (card != null)
                        {
                            card.ChangeHome(Containers.Center);
                            SC_GameLog.Instance.AddMessege($"{currentPlayer} Played Skip");
                            if (attackStack > 0) { attackStack--; return false; }
                            else { return true; }
                        }
                    }
                }
                return false;
            case CardTypes.Shuffle:
                card.ChangeHome(Containers.Center);
                SC_GameLog.Instance.AddMessege($"{currentPlayer} Played shuffle");
                _deck.Shuffle();
                _deck.ResetContainer();
                return false;
            case CardTypes.Favor:
                return false;
            case CardTypes.Attack:
                card.ChangeHome(Containers.Center);
                SC_GameLog.Instance.AddMessege($"{currentPlayer} Played attack");
                attackStack++;
                return true;
            case CardTypes.Skip:
                card.ChangeHome(Containers.Center);
                SC_GameLog.Instance.AddMessege($"{currentPlayer} Played skip");
                if (attackStack > 0) { attackStack--; return false; }
                else { return true; }
            default:
                SC_Card card2 = _hand.GetCard(type: card.Type);
                if (card2 != null && card != card2) {
                    card2.ChangeHome(Containers.Center);
                    card.ChangeHome(Containers.Center);
                    int i = UnityEngine.Random.Range(0, players.Count);
                    CardContainer randomPlayer = SC_GameData.Instance.GameWorld[i];
                    SC_Card card3 = randomPlayer.GetRandomCard(randomPlayer.count);
                    if (card3 != null) {
                        card3.ChangeHome(currentPlayer);
                    }
                    SC_GameLog.Instance.AddMessege($"{currentPlayer} Played {card.Type}�2 on {randomPlayer.CS.Container}");
                }
                return false;
        }
    }

    private IEnumerator WaitForGiveCard()
    {
        yield return null;
    }

    #endregion

    /// <summary>
    /// Used to turn buttons on and off <para></para>
    /// Maybe move to controller script?
    /// </summary>
    public void ToggleButton(string buttonName)
    {
        GameObject button = SC_GameData.Instance.GetUnityObject(buttonName);
        if (button == null)
        {
            Debug.LogError($"Button {buttonName} not found. make sure name is current and tag is unityObjects.");
            return;
        }
        button.SetActive(!button.activeSelf);
    }
}