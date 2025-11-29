using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class SaveData
{

    // The number of successfully matched pairs.
    public int matchCount;
  
    public int NoMatchCount;

    // A list containing the state of each card on the board.
    public List<CardState> cardStates = new List<CardState>();
}


[Serializable]
public class CardState
{
    // The name of the sprite assigned to the card's face.
    public string spriteName;
    
    // Whether the card has been successfully matched.
    public bool isMatched;
}
