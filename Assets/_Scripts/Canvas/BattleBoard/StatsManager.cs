using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsManager : MonoBehaviour
{
    public GameObject tokenImagePrefab;
    public Transform poolContentP1; // Content w PoolScrollSection dla Gracza 1
    public Transform poolContentP2;
    public Transform graveyardContentP1; // Content w GraveyardScrollSection dla Gracza 1
    public Transform graveyardContentP2;
    public TokenSlotManager tokenSlotManager;

    private Dictionary<string, int> player1Pool = new Dictionary<string, int>(); // Pula Gracza 1
    private Dictionary<string, int> player2Pool = new Dictionary<string, int>(); // Pula Gracza 2
    private Dictionary<string, int> player1Graveyard = new Dictionary<string, int>(); // Cmentarz Gracza 1
    private Dictionary<string, int> player2Graveyard = new Dictionary<string, int>(); // Cmentarz Gracza 2

    void Awake()
    {
        //if (FindObjectsOfType<StatsManager>().Length > 1)
        //{
        //    Destroy(gameObject);
        //    return;
        //}

        //DontDestroyOnLoad(gameObject);
    }

    public void InitializePools(List<TokenData> p1Pool, List<TokenData> p2Pool)
    {
        // Czyszczenie poprzednich danych
        player1Pool.Clear();
        player2Pool.Clear();

        // Przekszta³cenie listy w s³ownik {Nazwa ¿etonu, Liczba w puli}
        CountTokens(p1Pool, player1Pool);
        CountTokens(p2Pool, player2Pool);

        PopulateScrollView(player1Pool, poolContentP1);
        PopulateScrollView(player2Pool, poolContentP2);
        PopulateScrollView(player1Graveyard, graveyardContentP1);
        PopulateScrollView(player2Graveyard, graveyardContentP2);
    }

    private void CountTokens(List<TokenData> tokenList, Dictionary<string, int> poolDictionary)
    {
        foreach (var token in tokenList)
        {
            if (poolDictionary.ContainsKey(token.tokenName))
            {
                poolDictionary[token.tokenName]++;
            }
            else
            {
                poolDictionary[token.tokenName] = 1;
            }
        }
    }

    private void PopulateScrollView(Dictionary<string, int> tokenPool, Transform content)
    {
        // Usuwanie starych obiektów przed ponownym generowaniem
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        foreach (var token in tokenPool)
        {
            GameObject newTokenImage = Instantiate(tokenImagePrefab, content);
            Image tokenImage = newTokenImage.GetComponent<Image>();
            Text quantityText = newTokenImage.transform.GetChild(0).GetComponent<Text>(); // Pobranie Text (Quantity)

            TokenData tokenData = tokenSlotManager.GetTokenDataByName(token.Key);
            if (tokenData != null)
            {
                tokenImage.sprite = tokenData.sprite; // Ustawienie obrazka
            }

            quantityText.text = token.Value.ToString(); // Ustawienie liczby ¿etonów

            // Dodanie eventu klikniêcia na ¿eton
            Button button = newTokenImage.GetComponent<Button>();
            button.onClick.AddListener(() => Debug.Log($"Klikniêto: {token.Key}"));
        }
    }

    public void AddToGraveyard(TokenData tokenData, int player)
    {
        Dictionary<string, int> graveyard = (player == 1) ? player1Graveyard : player2Graveyard;

        if (graveyard.ContainsKey(tokenData.tokenName))
        {
            graveyard[tokenData.tokenName]++;
        }
        else
        {
            graveyard[tokenData.tokenName] = 1;
        }

        // Aktualizacja wyœwietlania cmentarza
        PopulateScrollView(graveyard, (player == 1) ? graveyardContentP1 : graveyardContentP2);
    }
    public void RemoveFromGraveyard(TokenData tokenData, int player)
    {
        Dictionary<string, int> graveyard = (player == 1) ? player1Graveyard : player2Graveyard;

        if (graveyard.ContainsKey(tokenData.tokenName))
        {
            graveyard[tokenData.tokenName]--;

            if (graveyard[tokenData.tokenName] <= 0)
            {
                graveyard.Remove(tokenData.tokenName);
            }
        }

        // Aktualizacja wyœwietlania cmentarza
        PopulateScrollView(graveyard, (player == 1) ? graveyardContentP1 : graveyardContentP2);
    }
}
