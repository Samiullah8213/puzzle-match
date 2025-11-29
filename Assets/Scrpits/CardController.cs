using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PrimeTween;
using UnityEngine.UI;
using System;
using TMPro;
using Random = UnityEngine.Random;

public class CardController : MonoBehaviour
{
    // --- FIELDS ---

    private const string SaveKey = "CardGameState";

    [Header("Game Settings")]
    [SerializeField] private int _columns = 2;

    [Header("References")]
    [SerializeField] private Card _cardPrefab;
    [SerializeField] private GridLayoutGroup _cardGridLayoutGroup;
    [SerializeField] private TMP_Text _matchCountText;
    [SerializeField] private TMP_Text _totalCountText;
    [SerializeField] private Sprite[] _sprites;
    
    private List<Card> _cards = new List<Card>();
    private List<Sprite> _spritePairs;
    private Card _firstSelectedCard;
    private Card _secondSelectedCard;
    private int _matchCount;
    private int _NoMatchCount;
    private int _totalCountOfSprites;
    private bool _isChecking; // Flag to prevent input during match check.

    // --- UNITY METHODS ---

    /// <summary>
    /// Initializes the game. Tries to load a saved game first, otherwise starts a new one.
    /// </summary>
    private void Start()
    {
        if (!LoadGame())
        {
            SetupGridLayout();
            InitilizeSprites();
            InitializeNewGame();
            _matchCount = 0;
            _NoMatchCount = 0;
            _totalCountOfSprites = _spritePairs.Count;
        }

        UpdateScore();
    }

    /// <summary>
    /// Saves the game state when the application quits.
    /// </summary>
    private void OnApplicationQuit()
    {
        SaveGame();
    }

    /// <summary>
    /// Configures the GridLayoutGroup component.
    /// </summary>
    private void SetupGridLayout()
    {
        if (_cardGridLayoutGroup == null)
        {
            Debug.LogError("Please assign a GridLayoutGroup to the CardController.");
            return;
        }
        _cardGridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        _cardGridLayoutGroup.constraintCount = _columns;
    }

    /// <summary>
    /// Creates and shuffles the list of sprite pairs for a new game.
    /// </summary>
    private void InitilizeSprites()
    {
        _spritePairs = new List<Sprite>();
        for (int i = 0; i < _sprites.Length; i++)
        {
            _spritePairs.Add(_sprites[i]);
            _spritePairs.Add(_sprites[i]);
        }
        ShuffleList(_spritePairs);
    }
    
    /// <summary>
    /// Instantiates cards for a new game.
    /// </summary>
    private void InitializeNewGame()
    {
        for (int i = 0; i < _spritePairs.Count; i++)
        {
            Card card = Instantiate(_cardPrefab, _cardGridLayoutGroup.transform);
            card.SetIconSprite(_spritePairs[i]);
            card.SetDefaultIcon();
            card._cardController = this;
            _cards.Add(card);
        }
    }
   
    /// <summary>
    /// Determines if a card can be selected.
    /// </summary>
    public bool CanSelectCards() => !_isChecking && _secondSelectedCard == null;

    /// <summary>
    /// Called by a card when clicked. Manages the two-card selection process.
    /// </summary>
    public void SetSelectedCard(Card card)
    {
        if (card == _firstSelectedCard) return; // Ignore clicking the same card twice.

        card.ShowImage();

        if (_firstSelectedCard == null)
        {
            _firstSelectedCard = card;
        }
        else
        {
            _secondSelectedCard = card;
            _isChecking = true; // Block further selections.
            StartCoroutine(CheckForMatch(_firstSelectedCard, _secondSelectedCard));
        }
    }
    
    /// <summary>
    /// Coroutine to check if the two selected cards match.
    /// </summary>
    private IEnumerator CheckForMatch(Card card1, Card card2)
    {
        yield return new WaitForSeconds(0.5f);

        if (card1.IconSprite == card2.IconSprite)
        {
            // --- MATCH FOUND ---
            card1.SetMatchedState();
            card2.SetMatchedState();
            _matchCount++;
            SaveGame(); // Save progress after a successful match.

            // Check for win condition
            if (_matchCount >= _totalCountOfSprites / 2)
            {
                // Play win animation
                PrimeTween.Sequence.Create()
                    .Chain(Tween.Scale(_cardGridLayoutGroup.transform, new Vector3(1.1f, 1.1f, 1.1f), 0.3f, Ease.OutBack))
                    .Chain(Tween.Scale(_cardGridLayoutGroup.transform, new Vector3(1f, 1f, 1f), 0.25f, Ease.InSine));
            }
        }
        else
        {
            // --- NO MATCH ---
            card1.HideImage();
            card2.HideImage();
            _NoMatchCount++;
        }
        
        // Reset selections and allow input again.
        _firstSelectedCard = null;
        _secondSelectedCard = null;
        _isChecking = false;

        UpdateScore();
    }

    /// <summary>
    /// Shuffles the provided list of sprites.
    /// </summary>
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    private void UpdateScore()
    {
        _matchCountText.text = $"Matches: {_matchCount}";
        _totalCountText.text = $"Total Try: {_NoMatchCount + _matchCount}";
    }
   
    /// <summary>
    /// Saves the current game state to PlayerPrefs.
    /// </summary>
    private void SaveGame()
    {
        SaveData data = new SaveData
        {
            matchCount = _matchCount
            , NoMatchCount = _NoMatchCount
        };

        foreach (Card card in _cards)
        {
            data.cardStates.Add(new CardState
            {
                spriteName = card.IconSprite.name,
                isMatched = card.IsMatched
            });
        }
        
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
        Debug.Log("Game Saved!");
    }

    /// <summary>
    /// Loads a game state from PlayerPrefs and rebuilds the board.
    /// </summary>
    /// <returns>True if a save was loaded, false otherwise.</returns>
    private bool LoadGame()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            return false;
        }

        string json = PlayerPrefs.GetString(SaveKey);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        _matchCount = data.matchCount;
        _NoMatchCount = data.NoMatchCount;
        _totalCountOfSprites = data.cardStates.Count;
        // Create a lookup for sprites to easily find them by name.
        var spriteDict = _sprites.ToDictionary(s => s.name);

        SetupGridLayout();

        foreach (CardState cardState in data.cardStates)
        {
            Card card = Instantiate(_cardPrefab, _cardGridLayoutGroup.transform);
            card.SetIconSprite(spriteDict[cardState.spriteName]);
            card._cardController = this;
            if (cardState.isMatched)
            {
                card.SetMatchedState();
            }
            _cards.Add(card);
        }
        
        Debug.Log("Game Loaded!");
        return true;
    }

    /// <summary>
    /// Deletes the saved game data from PlayerPrefs.
    /// Can be called from a UI button to allow players to restart.
    /// </summary>
    private void ResetSaveData()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        Debug.Log("Save data reset!");
    }

    private void ClearDate()
    {
        foreach (Card card in _cards)
        {
            card.ResetCardState();
        }
        _matchCount = 0;
        _NoMatchCount = 0;
        UpdateScore();
    }
    public void RestartLevel()//invoke from UI
    {
        ResetSaveData();
        ClearDate();
        InitilizeSprites();
    }
}
