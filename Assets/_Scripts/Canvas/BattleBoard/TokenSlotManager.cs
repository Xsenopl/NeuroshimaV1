using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class TokenSlotManager : MonoBehaviour
{
    public BoardManager boardManager;
    public GameObject player1SlotsPanel;
    public GameObject player2SlotsPanel;
    public List<Image> player1Slots;  // Sloty UI dla Gracza 1
    public List<Image> player2Slots;  // Sloty UI dla Gracza 2
    public Button endTurnButton;
    public Button undoButton;
    public GameObject trashSlotImage; // Obrazek potwierdzaj¹cy odrzucenie

    public TokenDatabase player1Database;
    public TokenDatabase player2Database;
    private List<TokenData> player1Pool = new List<TokenData>(); // ¯etony Gracza 1
    private List<TokenData> player2Pool = new List<TokenData>(); // ¯etony Gracza 2

    private Slot selectedSlot; // Wybrany slot (dla mechaniki odrzucania)
    private TokenData selectedToken = null; // Aktualnie wybrany ¿eton 
    private int turnCounter = 1;  // Licznik tur

    private void Start()
    {
        boardManager = FindObjectOfType<BoardManager>();
        //trashButton.gameObject.SetActive(false);
        //trashButton.onClick.AddListener(TrashSelectedToken);

        undoButton.onClick.AddListener(() => boardManager.UndoLastAction());
        endTurnButton.onClick.AddListener(EndTurn);
        InitializePools(); // Rozdzielamy ¿etony na graczy
        ClearSlots();

        AssignSlotListeners(player1Slots, 1);
        AssignSlotListeners(player2Slots, 2);

        DrawHeadquarter(1); // Gracz 1 dostaje sztab na start (z za³o¿enia zaczyna grê)
    }

    // Inicjalizacja puli ¿etonów z TokenDatabase
    private void InitializePools()
    {
        if (player1Database != null)
        {
            player1Pool = new List<TokenData>(player1Database.allTokens);
            //EnsureHeadquarterFirst(player1Pool);
        }
        else
        {
            Debug.LogError("Brak przypisanego TokenDatabase dla Gracza 1!");
        }

        if (player2Database != null)
        {
            player2Pool = new List<TokenData>(player2Database.allTokens);
            //EnsureHeadquarterFirst(player2Pool);
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


    // Czyœci sloty do domyœlnego wygl¹du
    public void ClearSlots()
    {
        foreach (var slot in player1Slots) slot.sprite = null;
        foreach (var slot in player2Slots) slot.sprite = null;
    }

    // Losowanie ¿etonów dla danego gracza
    private void DrawTokens(int count)
    {
        List<TokenData> pool = (boardManager.CurrentPlayer == 1) ? player1Pool : player2Pool;
        List<Image> slots = (boardManager.CurrentPlayer == 1) ? player1Slots : player2Slots;

        if (pool.Count == 0)
        {
            Debug.Log($"Gracz {boardManager.CurrentPlayer} nie ma wiêcej ¿etonów!");
            return;
        }

        int emptySlots = slots.Count(s => s.sprite == null);
        int tokensToDraw = Mathf.Min(emptySlots, count);

        if (tokensToDraw == 0)
        {
            Debug.Log($"Gracz {boardManager.CurrentPlayer} ma ju¿ komplet ¿etonów!");
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

        Debug.Log($"Gracz {boardManager.CurrentPlayer} wylosowa³ {emptySlots} nowe ¿etony!");
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
            Debug.LogError($"Brak wolnych slotów dla Gracza {player}!");
            return;
        }

        freeSlot.sprite = hq.sprite;
        freeSlot.gameObject.name = hq.tokenName;
        freeSlot.GetComponent<Slot>().assignedToken = hq;

        pool.Remove(hq);
        //Debug.Log($"Gracz {player} otrzyma³ sztab: {hq.tokenName}");
    }

    // Obs³uga koñca tury
    public void EndTurn()
    {
        if (HasThreeTokens()) { Debug.Log("Musisz najpierw odrzuciæ jakiœ ¿eton"); return; }

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
        Debug.Log($"Wybrano ¿eton: {token.tokenName}");
    }

    public TokenData GetSelectedToken()
    {
        if (selectedToken == null)
        {
            Debug.LogWarning("¯aden ¿eton nie zosta³ wybrany.");
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

//_______________ODRZUCANIE ¯ETONÓW__________________
    // Odrzucenie ¿etonu z rêki i dodanie na cmentarz
    public void DiscardToken(TokenData token, Slot slot)
    {
        int ownerPlayer = boardManager.GetTokenOwner(token.army);

        //Debug.Log($"Gracz {ownerPlayer} odrzuci³ ¿eton: {token.tokenName}.");
        boardManager.AddActionToStack(new ActionData(token, null, slot.GetComponent<Image>()));
        FindObjectOfType<StatsManager>().AddToGraveyard(token, ownerPlayer);

        slot.ClearSlot();
    }

    public void ConfirmDiscard()
    {
        if (selectedSlot == null || selectedSlot.assignedToken == null)
        {
            Debug.LogWarning("Brak wybranego ¿etonu do odrzucenia!");
            return;
        }

        TokenData tokenToDiscard = selectedSlot.assignedToken;
        DiscardToken(tokenToDiscard, selectedSlot);

        trashSlotImage.SetActive(false); // Ukryj obrazek, jeœli odrzucanie nie jest wymagane
        selectedSlot = null;
        selectedToken = null;
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
            Debug.Log("Nie ma ¿etonów na rêce");
            return;
        }

        trashSlotImage.SetActive(true);
        selectedSlot = slot;

        //Debug.Log($"Klikniêto ¿eton: {slot.assignedToken.tokenName}. Teraz mo¿na go odrzuciæ.");
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

        canvasGroup.interactable = isActive; // Blokuje klikniêcia
        canvasGroup.blocksRaycasts = isActive; // Blokuje przechodzenie klikniêæ
        canvasGroup.alpha = isActive ? 1f : 0.96f;
    }
    public void ClearSelectedToken()
    {
        selectedToken = null;
    }
}
