using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using JetBrains.Annotations;



public class MemoryCombatGame : MonoBehaviour
{
    public List<Button> cardButtons; // Lista de botões que representam as cartas na mesa
    public List<CardData> availableCards; // Lista de todas as cartas disponíveis no jogo
    public Sprite cardBackImage; // Imagem de fundo das cartas
    public Hero[] playerHeroes; // Heróis do jogador
    public Hero[] enemyHeroes; // Heróis do inimigo
    bool turniEnenym;
    public List<CardData> activeCards; // Cartas ativas na rodada
    public CardData firstSelectedCard;
    private Button firstSelectedButton;
    public int playerEnergy = 3; // Energia inicial do jogador
    public int enemyEnergy = 3; // Energia inicial do inimigo
    public List<CardData> revealedCards = new List<CardData>(); // Cartas que foram reveladas
    public float enemyTurnDelay = 1f;
    private bool isCheckingPair = false; // Flag para evitar a seleção de mais cartas enquanto o par é verificado
    public AudioClip clicksond, AcertoSond, errorSond, TrocarCardsSonds;
    public AudioSource sond;
    public List<CardData> limitedPairs;
    private bool replace;
    void Start()
    {
        SetupGame();
        RandomizeFistPlayer();
    }

    void SetupGame()
    {
        sond.PlayOneShot(TrocarCardsSonds);
        activeCards = new List<CardData>();

        // Limita as cartas a apenas um par por tipo (sem repetições de mais de duas cópias)
        replace = true;
        foreach (CardData card in availableCards)
        {
            if (limitedPairs.Count(c => c == card) < 2) // Verifica se já existem dois exemplares dessa carta
            {
                limitedPairs.Add(card); // Adiciona a carta uma vez para formar um par
                limitedPairs.Add(card); // Adiciona novamente para completar o par
            }
        }

        // Embaralha as cartas para distribuição aleatória
        List<CardData> shuffledCards = new List<CardData>(limitedPairs);
        shuffledCards.Shuffle();

        // Verifica se há um número par de cartas
        if (shuffledCards.Count % 2 != 0)
        {
            Debug.LogError("O número de cartas disponíveis não permite a formação de pares corretos.");
            return;
        }

        // Distribui as cartas para os botões
        for (int i = 0; i < cardButtons.Count; i++)
        {
            CardData card = shuffledCards[i];
            activeCards.Add(card);

            Button button = cardButtons[i];
            button.image.sprite = cardBackImage; // Define a imagem de fundo do botão
            button.interactable = true;

            CardComponent cardComponent = button.GetComponent<CardComponent>();
            if (cardComponent != null)
            {
                button.onClick.AddListener(() => OnCardClicked(button, cardComponent.cardData));
                cardComponent.cardData = card;
                cardComponent.isMatched = false;
            }
            else
            {
                Debug.LogError("O botão não possui um CardComponent.");
            }
        }
        ReplaceDuplicateCards();
        Invoke("ReplaceDuplicateCards", 0.1f);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            EnsureAllCardsHavePairs();

        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            for (int i = 0; i < cardButtons.Count; i++)
            {
                cardButtons[i].image.sprite = cardButtons[i].GetComponent<CardComponent>().cardData.frontImage;
            }

        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ReplaceDuplicateCards();

        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            ShuffleCardPositions();

        }
    }
    void ShuffleCardPositions()
    {
        // Cria uma lista temporária para armazenar os CardData atuais
        List<CardData> currentCardDataList = new List<CardData>();

        // Preenche a lista com os CardData dos CardComponent
        foreach (Button button in cardButtons)
        {
            CardComponent cardComponent = button.GetComponent<CardComponent>();
            if (cardComponent != null)
            {
                currentCardDataList.Add(cardComponent.cardData);
            }
        }

        // Embaralha a lista de CardData
        for (int i = 0; i < currentCardDataList.Count; i++)
        {
            CardData temp = currentCardDataList[i];
            int randomIndex = Random.Range(i, currentCardDataList.Count);
            currentCardDataList[i] = currentCardDataList[randomIndex];
            currentCardDataList[randomIndex] = temp;
        }

        // Atribui os CardData embaralhados de volta aos CardComponent
        for (int i = 0; i < cardButtons.Count; i++)
        {
            CardComponent cardComponent = cardButtons[i].GetComponent<CardComponent>();
            if (cardComponent != null)
            {
                cardComponent.cardData = currentCardDataList[i];
            }
        }
    }



    //>>>>>>>>>>>>>
    void ReplaceDuplicateCards()
    {
        // Dicionário para contar quantas vezes cada carta aparece
        Dictionary<CardData, int> cardCounts = new Dictionary<CardData, int>();

        // Contar todas as cartas
        foreach (Button button in cardButtons)
        {
            CardComponent cardComponent = button.GetComponent<CardComponent>();
            if (cardComponent != null)
            {
                if (cardCounts.ContainsKey(cardComponent.cardData))
                {
                    cardCounts[cardComponent.cardData]++;
                }
                else
                {
                    cardCounts[cardComponent.cardData] = 1;
                }
            }
        }

        // Lista para botões que precisam de novos pares
        List<Button> buttonsNeedingNewPairs = new List<Button>();

        // Verificar se há duplicatas e marcar as que precisam de substituição
        foreach (var pair in cardCounts)
        {
            if (pair.Value > 2) // Mais de dois iguais
            {
                int excessCount = pair.Value - 2;

                foreach (Button button in cardButtons)
                {
                    CardComponent cardComponent = button.GetComponent<CardComponent>();
                    if (cardComponent != null && cardComponent.cardData == pair.Key && excessCount > 0)
                    {
                        buttonsNeedingNewPairs.Add(button);
                        excessCount--;
                    }
                }
            }
        }

        // Substituir cartas duplicadas por novas cartas
        foreach (Button button in buttonsNeedingNewPairs)
        {
            CardComponent cardComponent = button.GetComponent<CardComponent>();

            // Procurar uma nova carta que não esteja em uso
            CardData newCard = GetUnusedCardData();

            if (newCard != null)
            {
                cardComponent.cardData = newCard;

                // Agora precisamos garantir que há um par para essa nova carta
                Button pairButton = FindButtonWithoutPair(newCard);

                if (pairButton != null)
                {
                    CardComponent pairCardComponent = pairButton.GetComponent<CardComponent>();
                    pairCardComponent.cardData = newCard;
                }
            }
            else
            {
                Debug.LogError("Não há mais cartas disponíveis para criar novos pares.");
            }
        }
        EnsureAllCardsHavePairs();
    }
    //>>>>>>>>>>
    void EnsureAllCardsHavePairs()
    {
        // Dicionário para contar quantas vezes cada carta aparece
        Dictionary<CardData, int> cardCounts = new Dictionary<CardData, int>();

        // Contar todas as cartas
        foreach (Button button in cardButtons)
        {
            CardComponent cardComponent = button.GetComponent<CardComponent>();
            if (cardComponent != null)
            {
                if (cardCounts.ContainsKey(cardComponent.cardData))
                {
                    cardCounts[cardComponent.cardData]++;
                }
                else
                {
                    cardCounts[cardComponent.cardData] = 1;
                }
            }
        }

        // Lista para botões que precisam de ajuste
        List<Button> buttonsNeedingAdjustment = new List<Button>();

        // Verificar se todos têm um par
        foreach (var pair in cardCounts)
        {
            if (pair.Value % 2 != 0)
            {
                // Se houver um número ímpar de cartas, encontrar o botão correspondente
                foreach (Button button in cardButtons)
                {
                    CardComponent cardComponent = button.GetComponent<CardComponent>();
                    if (cardComponent != null && cardComponent.cardData == pair.Key)
                    {
                        buttonsNeedingAdjustment.Add(button);
                        if (buttonsNeedingAdjustment.Count == 2)
                        {
                            break;
                        }
                    }
                }

                // Se dois botões precisam de ajuste, fazer com que eles formem um par
                if (buttonsNeedingAdjustment.Count == 2)
                {
                    CardComponent card1 = buttonsNeedingAdjustment[0].GetComponent<CardComponent>();
                    CardComponent card2 = buttonsNeedingAdjustment[1].GetComponent<CardComponent>();

                    // Atualizar o segundo botão para ter o mesmo cardData do primeiro
                    card2.cardData = card1.cardData;
                    //card1.SetCardImage(card1.cardData.frontImage);
                    //card2.SetCardImage(card1.cardData.frontImage);

                    buttonsNeedingAdjustment.Clear();
                }
            }
        }

        // Verificar se há duplicatas extras e reembaralhar se necessário
        bool hasExtraDuplicates = false;
        foreach (var pair in cardCounts)
        {
            if (pair.Value > 2)
            {
                hasExtraDuplicates = true;
                break;
            }
        }
        ShuffleCardPositions();
    }


















    CardData GetUnusedCardData()
    {
        // Encontrar uma carta que não esteja sendo usada no jogo
        foreach (CardData card in availableCards)
        {
            if (!IsCardInUse(card))
            {
                return card;
            }
        }
        return null; // Não há cartas disponíveis
    }

    bool IsCardInUse(CardData card)
    {
        // Verificar se a carta já está em uso nos botões
        foreach (Button button in cardButtons)
        {
            CardComponent cardComponent = button.GetComponent<CardComponent>();
            if (cardComponent != null && cardComponent.cardData == card)
            {
                return true;
            }
        }
        return false;
    }

    Button FindButtonWithoutPair(CardData cardData)
    {
        // Encontra um botão que não tenha par para a nova carta
        foreach (Button button in cardButtons)
        {
            CardComponent cardComponent = button.GetComponent<CardComponent>();
            if (cardComponent != null && cardComponent.cardData == cardData && !cardComponent.isMatched)
            {
                return button;
            }
        }
        return null;
    }







    















    public void RandomizeFistPlayer()
    {
        // Sortear quem começa
        if (Random.Range(0, 2) == 0)
        {
            Debug.Log("Jogador começa!");
        }
        else
        {
            Debug.Log("Inimigo começa!");
            StartEnemyTurn();
        }
    }






    void StartEnemyTurn()
    {
        enemyEnergy++;
        StartCoroutine(EnemyTurn());
    }

    IEnumerator EnemyTurn()
    {
        Debug.Log("Turno do Inimigo!");
        while (enemyEnergy > 0)
        {
            Button firstButton = null, secondButton = null;
            CardComponent firstCardComponent = null, secondCardComponent = null;

            // Selecionar a primeira carta que não tenha sido pareada
            firstCardComponent = GetUnmatchedCardComponent(out firstButton);
            if (firstCardComponent != null)
            {
                RevealCard(firstButton, firstCardComponent.cardData);
                yield return new WaitForSeconds(enemyTurnDelay);
            }

            // Selecionar a segunda carta que não tenha sido pareada e que não seja a primeira
            secondCardComponent = GetUnmatchedCardComponent(out secondButton);
            if (secondCardComponent != null && firstCardComponent != secondCardComponent)
            {
                RevealCard(secondButton, secondCardComponent.cardData);
                yield return new WaitForSeconds(enemyTurnDelay);
            }
            else
            {
                // Se não conseguiu selecionar uma segunda carta válida, sai do loop
                Debug.LogError("Não foi possível encontrar uma segunda carta válida.");

                enemyEnergy--;
                playerEnergy++;
                firstCardComponent.isMatched = false;
                secondCardComponent.isMatched = false;
                ResetSelectedCards(firstSelectedButton, secondButton);
                yield break;
            }

            // Verifica se as cartas combinam
            if (firstCardComponent.cardData == secondCardComponent.cardData)
            {
                if (IsMatchingHero(firstCardComponent.cardData))
                {
                    Attack(firstCardComponent.cardData.cardType);
                }

                DisableMatchedCards(firstButton, secondButton);
                firstCardComponent.isMatched = true;
                secondCardComponent.isMatched = true;
            }
            else
            {
                enemyEnergy--;
                ResetSelectedCards(firstButton, secondButton);
                firstCardComponent.isMatched = false;
                secondCardComponent.isMatched = false;
            }

            if (enemyEnergy == 0)
            {
                Debug.Log("Inimigo ficou sem energia, turno do jogador!");
                playerEnergy++;
                yield break;
            }
        }
    }

    CardComponent GetUnmatchedCardComponent(out Button button)
    {
        List<Button> interactableButtons = new List<Button>();

        foreach (Button btn in cardButtons)
        {
            var cardComponent = btn.GetComponent<CardComponent>();
            if (btn.interactable && !cardComponent.isMatched)
            {
                interactableButtons.Add(btn);
            }
        }

        if (interactableButtons.Count == 0)
        {
            button = null;
            Debug.Log("Não há botões interativos disponíveis.");
            return null;
        }

        int randomIndex = Random.Range(0, interactableButtons.Count);
        button = interactableButtons[randomIndex];
        return button.GetComponent<CardComponent>();
    }





    void OnCardClicked(Button button, CardData card)
    {
        if (isCheckingPair || !button.interactable || playerEnergy <= 0 || turniEnenym)
            return;
    
            if (firstSelectedCard == null)
            {
                firstSelectedCard = card;
                firstSelectedButton = button;
                RevealCard(button, card);
            }
            else if (firstSelectedButton != button)
            {
                RevealCard(button, card);
                StartCoroutine(WaitAndCheckMatch(button, card));
            }

       
       
    }

    IEnumerator WaitAndCheckMatch(Button secondButton, CardData secondCard)
    {
        isCheckingPair = true; // Ativa a flag para evitar seleção de cartas durante a verificação
        yield return new WaitForSeconds(1f);

        CardComponent firstCardComponent = firstSelectedButton.GetComponent<CardComponent>();
        CardComponent secondCardComponent = secondButton.GetComponent<CardComponent>();

        if (firstSelectedCard == secondCard)
        {
            sond.PlayOneShot(AcertoSond);

            Attack(secondCard.cardType);
            DisableMatchedCards(firstSelectedButton, secondButton);
            firstCardComponent.isMatched = true;
            secondCardComponent.isMatched = true;
        }
        else
        {
            playerEnergy--;
            ResetSelectedCards(firstSelectedButton, secondButton);
            if (playerEnergy <= 0)
            {
                StartEnemyTurn();
            }
        }

        firstSelectedCard = null;
        firstSelectedButton = null;
        isCheckingPair = false; // Desativa a flag após a verificação

        bool allMatched = true;
        foreach (Button btn in cardButtons)
        {
            if (!btn.GetComponent<CardComponent>().isMatched)
            {
                allMatched = false;
                break;
            }
        }

        if (allMatched)
        {
            Debug.Log("Todas as cartas foram combinadas!");
            SetupGame(); // Recomeça o jogo com novas cartas
        }
    }


    void RevealCard(Button button, CardData card)
    {
        sond.PlayOneShot(clicksond);
        button.image.sprite = card.frontImage;
    }

    void Attack(CardType cardType)
    {
        foreach (var hero in playerHeroes)
        {
            if (hero.heroType == cardType)
            {
                Debug.Log($"{hero.heroName} atacou com {hero.attackPower} de poder!");
                return;
            }
        }

        foreach (var hero in enemyHeroes)
        {
            if (hero.heroType == cardType)
            {
                Debug.Log($"Inimigo atacou com {hero.attackPower} de poder!");
                return;
            }
        }
    }

    void DisableMatchedCards(Button button1, Button button2)
    {
        if (button1 != null && button2 != null)
        {
            button1.interactable = false;
            button2.interactable = false;
        }
        else
        {
            Debug.LogError("Tentativa de desativar botões que são nulos.");
        }
    }

    void ResetSelectedCards(Button button1, Button button2)
    {
        sond.PlayOneShot(errorSond);

        firstSelectedCard = null;
        firstSelectedButton = null;
        if (button1)
        {
            button1.image.sprite = cardBackImage;
        }
        if (button2)
        {
            button2.image.sprite = cardBackImage;
        }
    }

    Button GetButtonForCard(CardData card)
    {
        foreach (Button btn in cardButtons)
        {
            if (btn.GetComponent<CardComponent>().cardData == card)
                return btn;
        }
        return null;
    }

    CardData GetRandomCard(out Button button)
    {
        List<Button> interactableButtons = new List<Button>();

        // Cria uma lista de botões interativos
        foreach (Button btn in cardButtons)
        {
            if (btn.interactable)
            {
                interactableButtons.Add(btn);
            }
        }

        // Se não há botões interativos, retorna null e loga um erro
        if (interactableButtons.Count == 0)
        {
            button = null;
            Debug.Log("Não há botões interativos disponíveis.");
            return null;
        }

        // Escolhe um botão aleatoriamente da lista de botões interativos
        int randomIndex = Random.Range(0, interactableButtons.Count);
        button = interactableButtons[randomIndex];
        return button.GetComponent<CardComponent>().cardData;
    }
    bool IsMatchingHero(CardData card)
    {
        return true;
    }
}



public static class ListExtensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        System.Random rng = new System.Random();
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
        }
    }
}