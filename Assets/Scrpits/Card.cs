using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PrimeTween; 
using PrimeTweenDemo;

public class Card : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private Sprite _hidenIconSprite;
    private Sprite _iconSprite;
    public bool _isSelected;
    public CardController _cardController;

    public Sprite IconSprite => _iconSprite;
    
    private void Start()
    {
        _iconImage.sprite = _hidenIconSprite;
    }
    public void OnCardClick()
    {
        _cardController.SetSelectedCard(this);
    }
    public void SetIconSprite(Sprite sprite)
    {
        _iconSprite = sprite;
    }

    public void ShowImage()
    {
        Tween.Rotation(transform, new Vector3(0, 180, 0), 0.2f);
        Tween.Delay(0.1f, () => _iconImage.sprite = _iconSprite);

        _isSelected = true;
    }
    
    public void HideImage()
    {
        Tween.Rotation(transform, new Vector3(0, 0, 0), 0.2f);
        Tween.Delay(0.1f, () =>
        {
            _iconImage.sprite = _hidenIconSprite;
            _isSelected = false;
        });
    }
}
