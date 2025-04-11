using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime;

public class TokenSlotManager : MonoBehaviour
{
    public BoardManager boardManager;
    public GameObject player1SlotsPanel;
    public GameObject player2SlotsPanel;
    public List<Image> player1Slots;  // Sloty UI dla Gracza 1
    public List<Image> player2Slots;  // Sloty UI dla Gracza 2
    public Button endTurnButton;
    public Button undoButton;
    public GameObject trashSlotImage; // Obrazek potwierdzaj�cy odrzucenie

    public TokenDatabase player1Database;
    public TokenDatabase player2Database;
    private List<TokenData> player1Pool = new List<TokenData>(); // �etony Gracza 1
    private List<TokenData> player2Pool = new List<TokenData>(); // �etony Gracza 2

    StatsManager statsManager;
    private Slot selectedSlot; // Wybrany slot (dla mechaniki odrzucania)
    private TokenData selectedToken = null; // Aktualnie wybrany �eton 
    private int turnCounter = 1;  // Licznik tur

    private void Start()
    {
        //boardManager = FindObjectOfType<BoardManager>();
        //boardManager = GameController.instance.boardManager;
        statsManager = FindObjectOfType<StatsManager>();

        undoButton.onClick.AddListener(() => boardManager.UndoLastAction());
        endTurnButton.onClick.AddListener(EndTurn);

        //trashButton.gameObject.SetActive(false);
        //trashButton.onClick.AddListener(TrashSelectedToken);

        
        //InitializePools(); // Rozdzielamy �etony na graczy
        ClearSlots();

        AssignSlotListeners(player1Slots, 1);
        AssignSlotListeners(player2Slots, 2);

        //DrawHeadquarter(1); // Gracz 1 dostaje sztab na start (z za�o�enia zaczyna gr�)
    }

    // Inicjalizacja puli �eton�w z TokenDatabase
    public void InitializePools()
    {
        if (player1Database != null)
        {
            player1Pool = new List<TokenData>(player1Database.allTokens);
        }
        else
        {
            Debug.LogError("Brak przypisanego TokenDatabase dla Gracza 1!");
        }

        if (player2Database != null)
        {
            player2Pool = new List<TokenData>(player2Database.allTokens);
        }
        else
        {
            Debug.LogError("Brak przypisanego TokenDatabase dla Gracza 2!");
        }

        DrawHeadquarter(1);
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
    private void DrawTokens(int count)
    {
        List<TokenData> pool = (boardManager.CurrentPlayer == 1) ? player1Pool : player2Pool;
        List<Image> slots = (boardManager.CurrentPlayer == 1) ? player1Slots : player2Slots;

        if (pool.Count == 0)
        {
            Debug.Log($"Gracz {boardManager.CurrentPlayer} nie ma wi�cej �eton�w!");
            return;
        }

        int emptySlots = slots.Count(s => s.sprite == null);
        int tokensToDraw = Mathf.Min(emptySlots, count);

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
        //Debug.Log($"Gracz {player} otrzyma� sztab: {hq.tokenName}");
    }

    // Obs�uga ko�ca tury
    public void EndTurn()
    {
        if (HasThreeTokens()) { Debug.Log("Musisz najpierw odrzuci� jaki� �eton"); return; }

        turnCounter++;
        boardManager.ChangeCurrentPlayer();

        if (turnCounter >4) { DrawTokens(3); }
        else if (turnCounter == 1)
        {
            DrawHeadquarter(1);
        }
        else if (turnCounter == 2)
        {
            DrawHeadquarter(2);
        }
        else if (turnCounter == 3)
        {
            DrawTokens(1);
        }
        else if (turnCounter == 4)
        {
            DrawTokens(2);
        }
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
            return null;
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

//_______________ODRZUCANIE �ETON�W__________________
    // Odrzucenie �etonu z r�ki i dodanie na cmentarz
    public void DiscardToken(TokenData token)
    {
        int ownerPlayer = boardManager.GetTokenOwner(token.army);

        if (statsManager != null)
        {
            //Debug.Log($"Gracz {ownerPlayer} odrzuci� �eton: {token.tokenName}.");
            statsManager.AddToGraveyard(token, ownerPlayer);
        }
    }

    public void ConfirmDiscard()
    {
        if (selectedSlot == null || selectedSlot.assignedToken == null)
        {
            Debug.LogWarning("Brak wybranego �etonu do odrzucenia!");
            trashSlotImage.SetActive(false);
            return;
        }

        TokenData tokenToDiscard = selectedSlot.assignedToken;
        boardManager.AddActionToStack(new ActionData(tokenToDiscard, null, selectedSlot.GetComponent<Image>()));
        DiscardToken(tokenToDiscard);

        trashSlotImage.SetActive(false); // Ukryj obrazek, je�li odrzucanie nie jest wymagane

        ClearAllSelections();
    }

    public void ShowTrashConfirmation(Slot slot)
    {
        if (trashSlotImage == null)
        {
            Debug.LogError("TrashSlotImage nie jest przypisany!");
            return;
        }

        if (!HasTokensLeftToDiscard())
        {
            Debug.Log("Nie ma �eton�w na r�ce");
            return;
        }

        trashSlotImage.SetActive(true);
        selectedSlot = slot;

        //Debug.Log($"Klikni�to �eton: {slot.assignedToken.tokenName}. Teraz mo�na go odrzuci�.");
    }
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

//_______________�ETONY AKCJI__________________
    public void AfterUsingActionToken()
    {
        if (selectedToken == null) return;

        DiscardToken(selectedToken);
        if (selectedSlot != null && selectedSlot.GetComponent<Slot>() != null)
        {
            selectedSlot.GetComponent<Slot>().ClearSlot();
            Debug.Log("To si� wywo�uje");
        }
        ClearAllSelections();
        trashSlotImage.SetActive(false);
    }

    public bool HasSelectedActionToken()
    {
        return selectedToken != null && selectedToken.tokenType == TokenType.Action;
    }


    //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

    public void UpdatePanelInteractivity()
    {
        bool isPlayer1Turn = (boardManager.CurrentPlayer == 1);

        SetPanelInteractivity(player1SlotsPanel, isPlayer1Turn);
        SetPanelInteractivity(player2SlotsPanel, !isPlayer1Turn);
    }
    private void SetPanelInteractivity(GameObject panel, bool isActive)
    {
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }

        canvasGroup.interactable = isActive; // Blokuje klikni�cia
        canvasGroup.blocksRaycasts = isActive; // Blokuje przechodzenie klikni��
        canvasGroup.alpha = isActive ? 1f : 0.96f;
    }
    public void ClearSelectedToken()
    {
        selectedToken = null;
    }

    public void ClearAllSelections()
    {
        selectedSlot.ClearSlot();
        selectedToken = null;
    }

    public void ResetAll()
    {
        ClearAllSelections();
        ClearSlots();
        turnCounter = 1;
    }
}
