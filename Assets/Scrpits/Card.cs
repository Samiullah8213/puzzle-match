using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

/// <summary>
/// Represents a single card in the game.
/// Handles visual state (flipping) and player interaction.
/// </summary>
public class Card : MonoBehaviour
{
    // --- FIELDS ---

    [Header("References")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private Sprite _hidenIconSprite;
    [SerializeField] private AudioClip _flipSound;
    [SerializeField] private AudioSource _audioSource;
    
    // The sprite for the card's face.
    private Sprite _iconSprite;
    // A reference to the main game controller.
    public CardController _cardController;

    // --- PROPERTIES ---

    /// <summary>
    /// Gets the sprite assigned to this card's face.
    /// </summary>
    public Sprite IconSprite => _iconSprite;
    
    /// <summary>
    /// Gets whether this card has been successfully matched.
    /// </summary>
    public bool IsMatched { get; private set; }
    

    /// <summary>
    /// Called when the card is clicked by the player.
    /// </summary>
    public void OnCardClick()
    {
        // Prevent clicking if the card is already matched or if the controller is busy.
        if (IsMatched || !_cardController.CanSelectCards())
        {
            return;
        }
        _cardController.SetSelectedCard(this);
    }
    
    /// <summary>
    /// Assigns a sprite to the card's face.
    /// </summary>
    public void SetIconSprite(Sprite sprite)
    {
        _iconSprite = sprite;
    }

    public void SetDefaultIcon()
    {
        _iconImage.sprite = _hidenIconSprite;
    }
    /// <summary>
    /// Animates the card turning over to show its face.
    /// </summary>
    public void ShowImage()
    {
        if (_flipSound != null)
            _audioSource?.PlayOneShot(_flipSound);
        
        Tween.Rotation(transform, new Vector3(0, 180, 0), 0.2f);
        Tween.Delay(0.1f, () => _iconImage.sprite = _iconSprite);
    }
    
    /// <summary>
    /// Animates the card turning over to hide its face.
    /// </summary>
    public void HideImage()
    {
        Tween.Rotation(transform, new Vector3(0, 0, 0), 0.2f);
        Tween.Delay(0.1f, () => _iconImage.sprite = _hidenIconSprite);
    }

    /// <summary>
    /// Sets the card's state to matched and instantly flips it face-up without animation.
    /// Used when loading a saved game.
    /// </summary>
    public void SetMatchedState()
    {
        IsMatched = true;
        transform.rotation = Quaternion.Euler(0, 180, 0);
        _iconImage.sprite = _iconSprite;
    }

    public void ResetCardState()
    {
        IsMatched = false;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        _iconImage.sprite = _hidenIconSprite;
    }
}