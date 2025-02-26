using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class TokenSlotManager : MonoBehaviour
{
    public BoardManager boardManager;
    public List<Image> player1Slots;  // Sloty UI dla Gracza 1
    public List<Image> player2Slots;  // Sloty UI dla Gracza 2
    public Button endTurnButton;      // Przycisk "Koniec tury"
    public Button undoButton;         // Przycisk "cofnij"
    public GameObject discardConfirmationImage; // Obrazek potwierdzaj�cy odrzucenie

    public TokenDatabase player1Database;
    public TokenDatabase player2Database;
    private List<TokenData> player1Pool = new List<TokenData>(); // �etony Gracza 1
    private List<TokenData> player2Pool = new List<TokenData>(); // �etony Gracza 2

    private Slot selectedSlot; // Wybrany slot (dla mechaniki odrzucania)
    private TokenData selectedToken = null; // Aktualnie wybrany �eton 

    private void Start()
    {
        boardManager = FindObjectOfType<BoardManager>();
        //trashButton.gameObject.SetActive(false);
        //trashButton.onClick.AddListener(TrashSelectedToken);

        undoButton.onClick.AddListener(() => boardManager.UndoLastAction());
        endTurnButton.onClick.AddListener(EndTurn);  // Przypisanie przycisku do metody
        InitializePools(); // Rozdzielamy �etony na graczy
        ClearSlots();

        AssignSlotListeners(player1Slots, 1);
        AssignSlotListeners(player2Slots, 2);

        DrawHeadquarter(1); // Gracz 1 dostaje sztab na start
        DrawHeadquarter(2); // Gracz 2 dostaje sztab na start
    }

    // Inicjalizacja puli �eton�w z TokenDatabase
    private void InitializePools()
    {
        if (player1Database != null)
        {
            player1Pool = new List<TokenData>(player1Database.allTokens);
            EnsureHeadquarterFirst(player1Pool);
        }
        else
        {
            Debug.LogError("Brak przypisanego TokenDatabase dla Gracza 1!");
        }

        if (player2Database != null)
        {
            player2Pool = new List<TokenData>(player2Database.allTokens);
            EnsureHeadquarterFirst(player2Pool);
        }
        else
        {
            Debug.LogError("Brak przypisanego TokenDatabase dla Gracza 2!");
        }
    }

    public List<TokenData> GetPlayer1Pool() { return new List<TokenData>(player1Pool); }
    public List<TokenData> GetPlayer2Pool() { return new List<TokenData>(player2Pool); }
    public bool HasTokensLeftToDiscard()
    {
        List<Image> slots = (boardManager.CurrentPlayer == 1) ? player1Slots : player2Slots;
        return slots.Any(s => s.sprite != null);
    }
    public bool HasThreeTokens()
    {
        List<Image> slots = (boardManager.CurrentPlayer == 1) ? player1Slots : player2Slots;
        int tokensInSlots = slots.Count(s => s.sprite != null);
        return tokensInSlots >= 3;
    }


    // Czy�ci sloty do domy�lnego wygl�du
    public void ClearSlots()
    {
        foreach (var slot in player1Slots) slot.sprite = null;
        foreach (var slot in player2Slots) slot.sprite = null;
    }

    // Losowanie �eton�w dla danego gracza
    public void DrawTokens()
    {
        if (HasThreeTokens())
        {
            Debug.LogWarning("Najpierw odrzu� �eton!");
            return;
        }

        List<TokenData> pool = (boardManager.CurrentPlayer == 1) ? player1Pool : player2Pool;
        List<Image> slots = (boardManager.CurrentPlayer == 1) ? player1Slots : player2Slots;

        if (pool.Count == 0)
        {
            Debug.Log($"Gracz {boardManager.CurrentPlayer} nie ma wi�cej �eton�w!");
            return;
        }

        int emptySlots = slots.Count(s => s.sprite == null);
        int tokensToDraw = Mathf.Min(emptySlots, 3);

        if (tokensToDraw == 0)
        {
            Debug.Log($"Gracz {boardManager.CurrentPlayer} ma ju� komplet �eton�w!");
            return;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (tokensToDraw == 0) break;
            if (slots[i].sprite != null) continue;

            if (pool.Count == 0) break;

            int randomIndex = Random.Range(0, pool.Count);
            TokenData drawnToken = pool[randomIndex];

            slots[i].sprite = drawnToken.sprite;
            slots[i].gameObject.name = drawnToken.tokenName;
            slots[i].GetComponent<Slot>().assignedToken = drawnToken;

            pool.RemoveAt(randomIndex);
            tokensToDraw--;
        }

        Debug.Log($"Gracz {boardManager.CurrentPlayer} wylosowa� {emptySlots} nowe �etony!");
    }

    private void DrawHeadquarter(int player)
    {
        List<TokenData> pool = (player == 1) ? player1Pool : player2Pool;
        List<Image> slots = (player == 1) ? player1Slots : player2Slots;

        TokenData hq = pool.FirstOrDefault(t => t.tokenType == TokenType.Headquarter);
        if (hq == null)
        {
            Debug.LogError($"Brak sztabu w puli dla Gracza {player}!");
            return;
        }

        Image freeSlot = slots.FirstOrDefault(s => s.sprite == null);
        if (freeSlot == null)
        {
            Debug.LogError($"Brak wolnych slot�w dla Gracza {player}!");
            return;
        }

        freeSlot.sprite = hq.sprite;
        freeSlot.gameObject.name = hq.tokenName;
        freeSlot.GetComponent<Slot>().assignedToken = hq;

        pool.Remove(hq);
        Debug.Log($"Gracz {player} otrzyma� sztab: {hq.tokenName}");
    }

    // Sprawdza i przesuwa Headquarter na pocz�tek listy
    private void EnsureHeadquarterFirst(List<TokenData> pool)
    {
        TokenData hq = pool.FirstOrDefault(t => t.tokenType == TokenType.Headquarter);
        if (hq != null)
        {
            pool.Remove(hq);
            pool.Insert(0, hq);
        }
    }

    // Obs�uga ko�ca tury
    public void EndTurn()
    {
        if (HasThreeTokens()) { Debug.Log("Musisz najpierw odrzuci� jaki� �eton"); return; }

        boardManager.ChangeCurrentPlayer();
        DrawTokens();  // Losowanie nowych �eton�w
    }

    private void AssignSlotListeners(List<Image> slots, int player)
    {
        foreach (var slot in slots)
        {
            Slot slotComponent = slot.gameObject.AddComponent<Slot>();
            slotComponent.SetManager(this, player);
        }
    }

    public void SelectToken(TokenData token)
    {
        selectedToken = token;
        Debug.Log($"Wybrano �eton: {token.tokenName}");
    }

    public TokenData GetSelectedToken()
    {
        if (selectedToken == null)
        {
            Debug.LogWarning("�aden �eton nie zosta� wybrany.");
        }
        return selectedToken;
    }
    public TokenData GetTokenDataByName(string tokenName)
    {
        List<TokenData> allTokens = new List<TokenData>();
        if (player1Database != null) allTokens.AddRange(player1Database.allTokens);
        if (player2Database != null) allTokens.AddRange(player2Database.allTokens);

        return allTokens.Find(t => t.tokenName == tokenName);
    }

//____________PRACE NAD ODRZUCANIEM �ETON�W__________________

    // Odrzucenie �etonu z r�ki i dodanie na cmentarz
    public void DiscardToken(TokenData token, Slot slot)
    {
        int ownerPlayer = boardManager.GetTokenOwner(token.army);

        Debug.Log($"Gracz {ownerPlayer} odrzuci� �eton: {token.tokenName}.");

        FindObjectOfType<StatsManager>().AddToGraveyard(token, ownerPlayer);

        slot.ClearSlot();
    }

    public void ConfirmDiscard()
    {
        if (selectedSlot == null || selectedSlot.assignedToken == null)
        {
            Debug.LogWarning("Brak wybranego �etonu do odrzucenia!");
            return;
        }

        TokenData tokenToDiscard = selectedSlot.assignedToken;
        DiscardToken(tokenToDiscard, selectedSlot);

        discardConfirmationImage.SetActive(false); // Ukryj obrazek, je�li odrzucanie nie jest wymagane
        selectedSlot = null;
        selectedToken = null;
    }

    public void ShowDiscardConfirmation(Slot slot)
    {
        if (discardConfirmationImage == null)
        {
            Debug.LogError("TrashSlotImage nie jest przypisany!");
            return;
        }

        if (!HasTokensLeftToDiscard())
        {
            Debug.Log("Nie ma �eton�w na r�ce");
            return;
        }

        discardConfirmationImage.SetActive(true);
        selectedSlot = slot;

        Debug.Log($"Klikni�to �eton: {slot.assignedToken.tokenName}. Teraz mo�na go odrzuci�.");
    }

    public void RemoveTokenFromSlot(Image slot)
    {
        slot.sprite = null;
        slot.gameObject.name = "EmptySlot";
        slot.GetComponent<Slot>().assignedToken = null;
    }
    public void ClearSelectedToken()
    {
        selectedToken = null;
    }
}

/* Pierwsza pr�ba stworzenia cmentarza i cofni�cia akcji z niego
    public TokenData GetTokenDataByName(string tokenName, int player)
    {
        List<TokenData> pool = (player == 1) ? player1Pool : player2Pool;

        foreach (TokenData token in pool)
        {
            if (token.tokenName == tokenName)
            {
                return token;
            }
        }

        Debug.LogError($"Nie znaleziono �etonu o nazwie {tokenName} w puli Gracza {player}!");
        return null;
    }

    public void SelectTokenForTrash(TokenData token, Image slot)
    {
        selectedToken = token;
        selectedSlot = slot;
        trashButton.gameObject.SetActive(true);
    }

    private void TrashSelectedToken()
    {
        if (selectedToken != null && selectedSlot != null)
        {
            selectedSlot.sprite = null;
            selectedSlot.gameObject.name = "EmptySlot";
            selectedSlot.GetComponent<Slot>().assignedToken = null;

            boardManager.AddToGraveyard(selectedToken, boardManager.CurrentPlayer);

            selectedToken = null;
            selectedSlot = null;
            trashButton.gameObject.SetActive(false);
        }
    }

*/