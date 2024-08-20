using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardComponent : MonoBehaviour
{
    public bool selected;
    public bool isMatched; // Para rastrear se a carta foi combinada
    public CardData cardData; // Refer�ncia ao ScriptableObject CardData

    public Sprite cardImage; // Refer�ncia para armazenar a imagem da carta

    void Start()
    {
        // Assumindo que o componente Image est� no mesmo GameObject
        cardImage = GetComponent<Image>().sprite;
    }

    
}