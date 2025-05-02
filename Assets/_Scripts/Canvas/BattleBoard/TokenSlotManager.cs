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
    public Button undoButton;
    public GameObject trashSlotImage; // Obrazek potwierdzaj¹cy odrzucenie
    public StatsManager statsManager;

    public TokenDatabase player1Database;
    public TokenDatabase player2Database;
    private List<TokenData> player1Pool = new List<TokenData>(); // ¯etony Gracza 1
    private List<TokenData> player2Pool = new List<TokenData>(); // ¯etony Gracza 2

    [SerializeField] private Slot selectedSlot; // Wybrany slot (dla mechaniki odrzucania)
    [SerializeField] private TokenData selectedToken = null; // Aktualnie wybrany ¿eton 
    [SerializeField]
    private int turnCounter = 1;  // Licznik tur
    private bool lastDraw = false;

    private void Awake()
    {
        if (player1Slots == null || player1Slots.Count == 0)
            Debug.LogWarning("player1Slots nie zosta³y przypisane w Inspectorze!");

        if (player2Slots == null || player2Slots.Count == 0)
            Debug.LogWarning("player2Slots nie zosta³y przypisane w Inspectorze!");

        AssignSlotListeners(player1Slots, 1);
        AssignSlotListeners(player2Slots, 2);
    }

    private void Start()
    {
        //undoButton.onClick.AddListener(() => boardManager.UndoLastAction());
    }

    // Inicjalizacja puli ¿etonów z TokenDatabase
    public void InitializePools(TokenDatabase player1Army, TokenDatabase player2Army)
    {
        player1Database = player1Army;
        player2Database = player2Army;

        if (player1Database != null)
        {
            player1Pool = new List<TokenData>(player1Database.allTokens);
        }
        else
        {
            Debug.LogError("Brak przypisanej armii dla Gracza 1!");
        }

        if (player2Database != null)
        {
            player2Pool = new List<TokenData>(player2Database.allTokens);
        }
        else
        {
            Debug.LogError("Brak przypisanej armii dla Gracza 2!");
        }

        ClearSlots();
        DrawHeadquarter(1);
    }

    public int GetTurnCounter() { return turnCounter; }
    public bool GetLastDraw() {  return lastDraw; }
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
        //foreach (var slot in player1Slots) slot.sprite = null;
        //foreach (var slot in player1Slots) slot.sprite = null;
        foreach (var slot in player1Slots) { 
            slot.sprite = null; 
            Color c = slot.color;
            c.a = 0f;
            slot.color = c;
        }
        foreach (var slot in player2Slots) {
            slot.sprite = null;
            Color c = slot.color;
            c.a = 0f;
            slot.color = c;
        }
    }

    public void DrawTokensMediator(int player)
    {
        if (turnCounter > 4) { DrawTokens(3, player); }
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
            DrawTokens(1, player);
        }
        else if (turnCounter == 4)
        {
            DrawTokens(2, player);
        }
    }

    // Losowanie ¿etonów dla danego gracza
    private void DrawTokens(int count, int player)
    {
        List<TokenData> pool = (player == 1) ? player1Pool : player2Pool;
        List<Image> slots = (player == 1) ? player1Slots : player2Slots;

        if (pool.Count == 0)
        {
            Debug.Log($"Gracz {player} nie ma wiêcej ¿etonów!");
            return;
        }

        int emptySlots = slots.Count(s => s.sprite == null);
        int tokensToDraw = Mathf.Min(emptySlots, count);

        if (tokensToDraw == 0)
        {
            Debug.Log($"Gracz {player} ma ju¿ komplet ¿etonów!");
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
            slots[i].color = new Color(slots[i].color.r, slots[i].color.g, slots[i].color.b, 1f);
            slots[i].gameObject.name = drawnToken.tokenName;
            slots[i].GetComponent<Slot>().assignedToken = drawnToken;

            pool.RemoveAt(randomIndex);
            tokensToDraw--;
        }

        Debug.Log($"Gracz {boardManager.CurrentPlayer} wylosowa³ {emptySlots} nowe ¿etony!");
        if (pool.Count == 0) lastDraw = true;
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
        freeSlot.color = new Color(freeSlot.color.r, freeSlot.color.g, freeSlot.color.b, 1f);
        freeSlot.gameObject.name = hq.tokenName;
        freeSlot.GetComponent<Slot>().assignedToken = hq;

        pool.Remove(hq);
        //Debug.Log($"Gracz {player} otrzyma³ sztab: {hq.tokenName}");
    }

    public void NextTurn() { turnCounter++; }
    public int GetCurrentTurn() {  return turnCounter; }


    private void AssignSlotListeners(List<Image> slots, int player)
    {
        foreach (var slot in slots)
        {
            Slot slotComponent = slot.gameObject.AddComponent<Slot>();
            slotComponent.SetManager(this);
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

//_______________ODRZUCANIE ¯ETONÓW__________________
    // Odrzucenie ¿etonu z rêki i dodanie na cmentarz
    public void DiscardToken(TokenData token)
    {
        int ownerPlayer = boardManager.GetTokenOwner(token.army);

        if (statsManager != null)
        {
            //Debug.Log($"Gracz {ownerPlayer} odrzuci³ ¿eton: {token.tokenName}.");
            statsManager.AddToGraveyard(token, ownerPlayer);
        }
    }

    public void ConfirmDiscard()
    {
        if (selectedSlot == null || selectedSlot.assignedToken == null)
        {
            Debug.LogWarning("Brak wybranego ¿etonu do odrzucenia!");
            trashSlotImage.SetActive(false);
            return;
        }

        TokenData tokenToDiscard = selectedSlot.assignedToken;
        boardManager.AddActionToStack(new ActionData(tokenToDiscard, null, selectedSlot.GetComponent<Image>()));
        DiscardToken(tokenToDiscard);

        trashSlotImage.SetActive(false); // Ukryj obrazek, jeœli odrzucanie nie jest wymagane

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
            Debug.Log("Nie ma ¿etonów na rêce");
            return;
        }

        trashSlotImage.SetActive(true);
        selectedSlot = slot;

        //Debug.Log($"Klikniêto ¿eton: {slot.assignedToken.tokenName}. Teraz mo¿na go odrzuciæ.");
    }
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

//_______________¯ETONY AKCJI__________________
    public void AfterUsingActionToken()
    {
        if (selectedToken == null) return;

        DiscardToken(selectedToken);
        if (selectedSlot != null && selectedSlot.GetComponent<Slot>() != null)
        {
            selectedSlot.GetComponent<Slot>().ClearSlot();
            Debug.Log("To siê wywo³uje");
        }
        ClearAllSelections();
        trashSlotImage.SetActive(false);
    }

    public bool HasSelectedActionToken()
    {
        return selectedToken != null && selectedToken.tokenType == TokenType.Action;
    }


    //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

    public void UpdatePanelInteractivity(int player)
    {
        bool isPlayer1Turn = (player == 1);

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

    public void ClearAllSelections()
    {
        if(selectedSlot != null)
            selectedSlot.ClearSlot();
        selectedToken = null;
    }

    public void ResetAll()
    {
        ClearAllSelections();
        ClearSlots();
        turnCounter = 1;
        lastDraw = false;
    }
}
