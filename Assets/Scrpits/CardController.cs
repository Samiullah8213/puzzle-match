using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PrimeTween;
using UnityEngine.UI;

public class CardController : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int _columns = 2;

    [Header("References")]
    [SerializeField] private Card _cardPrefab;
    [SerializeField] private GridLayoutGroup _cardGridLayoutGroup;
    [SerializeField] private Sprite[] _sprites;
    
    private List<Sprite> _spritePairs;
    private Card _firstSelectedCard;
    private Card _secondSelectedCard;
    private int _matchCount;
    
    private void Start()
    {
        SetupGridLayout();
        InitilizeSprites();
        InitializeCards();
        _matchCount = 0; // Reset match count at the start of the game.
    }

    /// <summary>
    /// Configures the GridLayoutGroup based on the specified number of columns.
    /// </summary>
    private void SetupGridLayout()
    {
        if (_cardGridLayoutGroup == null)
        {
            Debug.LogError("Please assign a GridLayoutGroup to the CardController.");
            return;
        }
        // Set the grid to have a fixed number of columns.
        _cardGridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        _cardGridLayoutGroup.constraintCount = _columns;
    }

    private void InitilizeSprites()
    {
        _spritePairs = new List<Sprite>();
        // Duplicate each sprite to create pairs.
        for (int i = 0; i < _sprites.Length; i++)
        {
            _spritePairs.Add(_sprites[i]);
            _spritePairs.Add(_sprites[i]);
        }

        ShuffleSprites();
    }
    private void InitializeCards()
    {
        // Create a card for each sprite in the shuffled list.
        for (int i = 0; i < _spritePairs.Count; i++)
        {
            Card card = Instantiate(_cardPrefab, _cardGridLayoutGroup.transform);
            card.SetIconSprite(_spritePairs[i]);
            card._cardController = this;
        }
    }

    /// <summary>
    /// Shuffles the list of sprite pairs using the Fisher-Yates algorithm.
    /// </summary>
    private void ShuffleSprites()
    {
        int spriteCount = _spritePairs.Count;
        for (int i = 0; i < spriteCount; i++)
        {
            // Swap the current element with a random one that comes after it.
            Sprite temp = _spritePairs[i];
            int randomIndex = Random.Range(i, spriteCount);
            _spritePairs[i] = _spritePairs[randomIndex];
            _spritePairs[randomIndex] = temp;
        }
    }
    
    /// <summary>
    /// Called by a card when it is clicked. Manages the selection logic.
    /// </summary>
    /// <param name="card">The card that was selected.</param>
    public void SetSelectedCard(Card card)
    {
        if (!card._isSelected) 
        {
            card.ShowImage();
            
            // If this is the first card selected in a turn, store it.
            if (_firstSelectedCard == null)
            {
                _firstSelectedCard = card;
                return;
            }
            // If this is the second card, store it and check for a match.
            if (_secondSelectedCard == null)
            {
                _secondSelectedCard = card;
                StartCoroutine(CheckForMatch(_firstSelectedCard, _secondSelectedCard));
                // Reset selections for the next turn.
                _firstSelectedCard = null;
                _secondSelectedCard = null;
            }
        }
    }
    
    /// <summary>
    /// A coroutine that checks if two selected cards are a match.
    /// </summary>
    /// <param name="card1">The first selected card.</param>
    /// <param name="card2">The second selected card.</param>
    IEnumerator CheckForMatch(Card card1, Card card2)
    {
        yield return new WaitForSeconds(0.5f);
        if (card1.IconSprite == card2.IconSprite)
        {
            _matchCount++;
            if (_matchCount >= _spritePairs.Count/2)
            {
                // Play a "win" animation if all pairs are matched.
                PrimeTween.Sequence.Create()
                    .Chain(Tween.Scale(_cardGridLayoutGroup.transform, new Vector3(1.1f, 1.1f, 1.1f), 0.3f, Ease.OutBack))
                    .Chain(Tween.Scale(_cardGridLayoutGroup.transform, new Vector3(1f, 1f, 1f), 0.25f, Ease.InSine));
            }
        }
        else
        {
            // If they don't match, flip them back over.
            card1.HideImage();
            card2.HideImage();
        }
    }
    
}
