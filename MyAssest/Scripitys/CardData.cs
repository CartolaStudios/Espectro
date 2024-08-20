using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "NewCard", menuName = "MemoryGame/Card")]
public class CardData : ScriptableObject
{
    public Sprite frontImage; // Imagem da frente da carta
    public CardType cardType; // Tipo da carta (Sangue, Sucata, Magia)
}

    public enum CardType
{
    Sangue,
    Sucata,
    Magia
}