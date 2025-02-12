using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class TokenSlotManager : MonoBehaviour
{
    public List<Image> player1Slots;  // Sloty UI dla Gracza 1
    public List<Image> player2Slots;  // Sloty UI dla Gracza 2
    public Button endTurnButton;      // Przycisk "Koniec tury"

    public TokenDatabase player1Database;
    public TokenDatabase player2Database;
    private List<TokenData> player1Pool = new List<TokenData>(); // ¯etony Gracza 1
    private List<TokenData> player2Pool = new List<TokenData>(); // ¯etony Gracza 2

    private int currentPlayer = 2; // 1 - Gracz 1, 2 - Gracz 2
    private TokenData selectedToken = null; // Aktualnie wybrany ¿eton
    private bool player1HasHeadquarter = false;
    private bool player2HasHeadquarter = false;

    private void Start()
    {
        endTurnButton.onClick.AddListener(EndTurn);  // Przypisanie przycisku do metody
        InitializePools(); // Rozdzielamy ¿etony na graczy
        ClearSlots();

        AssignSlotListeners(player1Slots, 1);
        AssignSlotListeners(player2Slots, 2);

        DrawHeadquarter(1); // Gracz 1 dostaje sztab na start
        DrawHeadquarter(2); // Gracz 2 dostaje sztab na start
    }

    // Inicjalizacja puli ¿etonów z TokenDatabase
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

    // Czyœci sloty do domyœlnego wygl¹du
    public void ClearSlots()
    {
        foreach (var slot in player1Slots) slot.sprite = null;
        foreach (var slot in player2Slots) slot.sprite = null;
    }

    // Losowanie ¿etonów dla danego gracza
    private void DrawTokens()
    {
        List<TokenData> pool = (currentPlayer == 1) ? player1Pool : player2Pool;
        List<Image> slots = (currentPlayer == 1) ? player1Slots : player2Slots;

        if (pool.Count == 0)
        {
            Debug.Log($"Gracz {currentPlayer} nie ma wiêcej ¿etonów!");
            return;
        }

        int emptySlots = slots.Count(s => s.sprite == null);
        int tokensToDraw = Mathf.Min(emptySlots, 3);

        if (tokensToDraw == 0)
        {
            Debug.Log($"Gracz {currentPlayer} ma ju¿ komplet ¿etonów!");
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

        Debug.Log($"Gracz {currentPlayer} wylosowa³ {emptySlots} nowe ¿etony!");
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
        Debug.Log($"Gracz {player} otrzyma³ sztab: {hq.tokenName}");
    }

    // Sprawdza i przesuwa Headquarter na pocz¹tek listy
    private void EnsureHeadquarterFirst(List<TokenData> pool)
    {
        TokenData hq = pool.FirstOrDefault(t => t.tokenType == TokenType.Headquarter);
        if (hq != null)
        {
            pool.Remove(hq);
            pool.Insert(0, hq);
        }
    }

    // Obs³uga koñca tury
    public void EndTurn()
    {
        Debug.Log($"Tura Gracza {currentPlayer} zakoñczona.");
        currentPlayer = (currentPlayer == 1) ? 2 : 1;  // Zmiana gracza
        DrawTokens();  // Losowanie nowych ¿etonów
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



// Inicjalizacja puli ¿etonów z TokenDatabase - stan sprzed pracy nad "Wybierz sztab jako pierwszy"
/*
private void InitializePools()
{
    if (player1Database != null)
        player1Pool = new List<TokenData>(player1Database.allTokens);
    else
        Debug.LogError("Brak przypisanego TokenDatabase dla Gracza 1!");

    if (player2Database != null)
        player2Pool = new List<TokenData>(player2Database.allTokens);
    else
        Debug.LogError("Brak przypisanego TokenDatabase dla Gracza 2!");
}
*/


// Losowanie ¿etonów dla danego gracza  - stan sprzed pracy nad "Wybierz sztab jako pierwszy"
/*
    private void DrawTokens()
    {
        List<TokenData> pool = (currentPlayer == 1) ? player1Pool : player2Pool;
        List<Image> slots = (currentPlayer == 1) ? player1Slots : player2Slots;
        bool hasHQ = (currentPlayer == 1) ? player1HasHeadquarter : player2HasHeadquarter;

        if (pool.Count == 0)
        {
            Debug.Log($"Gracz {currentPlayer} nie ma wiêcej ¿etonów!");
            return;
        }

        // Sprawdzamy, ile slotów jest pustych
        int emptySlots = 0;
        foreach (var slot in slots)
        {
            if (slot.sprite == null)
                emptySlots++;
        }

        // Okreœlamy, ile ¿etonów wylosowaæ
        int tokensToDraw = Mathf.Min(emptySlots, 3); // Maksymalnie 3 nowe ¿etony

        if (tokensToDraw == 0)
        {
            Debug.Log($"Gracz {currentPlayer} ma ju¿ komplet ¿etonów!");
            return;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (tokensToDraw == 0) break;
            if (slots[i].sprite != null) continue; // Pomijamy zajête sloty

            if (pool.Count == 0) break; // Jeœli pula siê skoñczy, przerywamy

            int randomIndex = Random.Range(0, pool.Count);
            TokenData drawnToken = pool[randomIndex];

            slots[i].sprite = drawnToken.sprite;
            slots[i].gameObject.name = drawnToken.tokenName;
            slots[i].GetComponent<Slot>().assignedToken = drawnToken;

            pool.RemoveAt(randomIndex);
            tokensToDraw--;
        }

        Debug.Log($"Gracz {currentPlayer} wylosowa³ {tokensToDraw} nowe ¿etony!");
    }
 */