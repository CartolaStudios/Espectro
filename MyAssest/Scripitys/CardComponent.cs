using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardComponent : MonoBehaviour
{
    public bool selected;
    public bool isMatched; // Para rastrear se a carta foi combinada
    public CardData cardData; // Referência ao ScriptableObject CardData

    public Sprite cardImage; // Referência para armazenar a imagem da carta

    void Start()
    {
        // Assumindo que o componente Image está no mesmo GameObject
        cardImage = GetComponent<Image>().sprite;
    }

    
}