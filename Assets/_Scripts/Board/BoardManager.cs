using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;


public class BoardManager : MonoBehaviour
{
    private int _currentPlayer = 1; // 1 - Gracz 1, 2 - Gracz 2
    public int CurrentPlayer 
    {
        get => _currentPlayer;
        set
        {
            if (_currentPlayer != value)  // Sprawdzenie, czy wartoœæ faktycznie siê zmienia
            {
                _currentPlayer = value;
                OnTurnChanged();
            }
        }
    }
    public string player1Army;
    public string player2Army;
    public Tilemap tilemap;  // Tilemapa reprezentuj¹ca planszê
    public GameObject tokenPrefab;
    public TokenSlotManager tokenManager;
    private Dictionary<Vector3Int, GameObject> occupiedTiles = new Dictionary<Vector3Int, GameObject>();  // Zajête heksy
    public Dictionary<Vector2Int, Token> tokenGrid = new Dictionary<Vector2Int, Token>();

    private Stack<ActionData> actionStack = new Stack<ActionData>();

    private void Start()
    {
        // Debug.Log("Plansza za³adowana. Gotowa do umieszczania ¿etonów.");
        player1Army = "Borgo";
        player2Army = "Outpost";
        Debug.Log($"Dla 1 jest {player1Army}.   Dla 2 jest {player2Army}");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPosition.z = 0;  // Upewniamy siê, ¿e jest na p³aszczyŸnie 2D
            SelectHex(worldPosition);
        }
    }

    public void ChangeCurrentPlayer() { CurrentPlayer = _currentPlayer == 1 ? 2 : 1; }

    // Zamiana pozycji œwiata na wspó³rzêdne heksagonalne
    public Vector3Int WorldToHex(Vector3 worldPosition)
    {
        return tilemap.WorldToCell(worldPosition);
    }

    // Zamiana wspó³rzêdnych heksagonalnych na pozycjê w œwiecie
    public Vector3 HexToWorld(Vector3Int hexPosition)
    {
        return tilemap.CellToWorld(hexPosition);
    }

    // Wybieranie heksu na podstawie pozycji œwiata
    public void SelectHex(Vector3 worldPosition)
    {
        Vector3Int hexPosition = WorldToHex(worldPosition);

        if (!tilemap.HasTile(hexPosition))
        {
            // Debug.Log("Klikniêto poza plansz¹.");
            return;
        }

        if (occupiedTiles.ContainsKey(hexPosition))  // Sprawdzenie, czy heks jest zajêty
        {
            // Debug.Log("To pole jest ju¿ zajête!");
            return;
        }

        TokenData selectedToken = tokenManager.GetSelectedToken();
        if (selectedToken == null)
        {
            // Debug.Log("Nie wybrano ¿etonu!");
            return;
        }

        PlaceToken(hexPosition, selectedToken);

        // Po umieszczeniu ¿etonu zerujemy wybrany ¿eton
        tokenManager.ClearSelectedToken();
    }

    // Umieszczanie ¿etonu na heksie
    public void PlaceToken(Vector3Int hexPosition, TokenData tokenData)
    {
        Vector3 spawnPosition = HexToWorld(hexPosition);
        GameObject newToken = Instantiate(tokenPrefab, spawnPosition, Quaternion.identity);
        newToken.GetComponent<Token>().Initialize(tokenData);
        newToken.GetComponent<Token>().InitializeRotationArea();

        occupiedTiles.Add(hexPosition, newToken);

        Image originalSlot = FindSlotByToken(tokenData);
        actionStack.Push(new ActionData(tokenData, hexPosition, originalSlot));

        originalSlot.GetComponent<Slot>().ClearSlot();

        Debug.Log($"¯eton {tokenData.tokenName} umieszczony na {hexPosition}");
        tokenManager.trashSlotImage.SetActive(false);
    }

    public void RemoveToken(Token token)
    {
        if (token == null) return;

        // Sprawdzenie, czy ¿eton nadal istnieje w `tokenGrid`
        if (tokenGrid.ContainsKey(token.hexCoords))
        {
            tokenGrid.Remove(token.hexCoords); // Usuniêcie z mapy tokenów
            occupiedTiles.Remove((Vector3Int)token.hexCoords);
        }

        // Zniszczenie obiektu w œwiecie gry
        if (token.gameObject != null)
        {
            Destroy(token.gameObject);
        }
    }

    private Image FindSlotByToken(TokenData token)
    {
        foreach (var slot in tokenManager.player1Slots)
            if (slot.gameObject.name == token.tokenName)
                return slot;

        foreach (var slot in tokenManager.player2Slots)
            if (slot.gameObject.name == token.tokenName)
                return slot;

        return null;
    }

    // Zapisuje tokeny do tokenGrid i nakazuje dla nich aktualizacjê ich s¹siadów 
    public void RegisterToken(Token token)//, Vector2Int hexCoords
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int hexCoords = WorldToHex(worldPosition);
        token.hexCoords = (Vector2Int)hexCoords;
        tokenGrid[(Vector2Int)hexCoords] = token;
        UpdateAllNeighbors();
    }

    public void UpdateAllNeighbors()
    {
        foreach (var token in tokenGrid.Values)
        {
            token.UpdateNeighbors(tokenGrid);
        }
    }

    public void UndoLastAction()
    {
        if (actionStack.Count == 0)
        {
            Debug.Log("Brak akcji do cofniêcia!");
            return;
        }

        ActionData lastAction = actionStack.Pop();

        if (lastAction.position != Vector3Int.zero) // Cofanie ¿etonu z planszy
        {
            // Usuwa ¿eton z planszy
            if (occupiedTiles.TryGetValue(lastAction.position, out GameObject tokenObject))
            {
                Destroy(tokenObject);
                occupiedTiles.Remove(lastAction.position);
                tokenGrid.Remove((Vector2Int)lastAction.position);
            }

            // Przywracam ¿eton do jego oryginalnego slotu
            lastAction.originalSlot.sprite = lastAction.token.sprite;
            lastAction.originalSlot.gameObject.name = lastAction.token.tokenName;
            lastAction.originalSlot.GetComponent<Slot>().assignedToken = lastAction.token;

            Debug.Log($"Cofniêto akcjê: {lastAction.token.tokenName} wróci³ do slotu.");
        }
        else // Cofanie odrzuconego ¿etonu
        {
            List<Image> slots = (CurrentPlayer == 1) ? tokenManager.player1Slots : tokenManager.player2Slots;
            Image emptySlot = slots.FirstOrDefault(s => s.sprite == null);

            if (emptySlot != null)
            {
                emptySlot.sprite = lastAction.token.sprite;
                emptySlot.gameObject.name = lastAction.token.tokenName;
                emptySlot.GetComponent<Slot>().assignedToken = lastAction.token;

                Debug.Log($"Cofniêto odrzucony ¿eton: {lastAction.token.tokenName}");

                // Usuniêcie ¿etonu z cmentarza
                int ownerPlayer = GetTokenOwner(lastAction.token.army);
                StatsManager statsManager = FindObjectOfType<StatsManager>();
                if (statsManager != null) { statsManager.RemoveFromGraveyard(lastAction.token, ownerPlayer); }           

                if (tokenManager.HasThreeTokens())
                {
                    Debug.Log("Gracz ma 3 ¿etony. Musi odrzuciæ jeden.");
                }
            }
            else
            {
                Debug.LogWarning("Brak wolnych slotów do przywrócenia ¿etonu.");
            }
        }
    }

    public Vector2Int GetHexDirection(Vector2Int hexCoords, AttackDirection direction)
    {
        bool isEvenColumn = hexCoords.y % 2 == 0;

        switch (direction)
        {
            case AttackDirection.Up: return new Vector2Int(1, 0);
            case AttackDirection.Down: return new Vector2Int(-1, 0);
            case AttackDirection.UpLeft: return isEvenColumn ? new Vector2Int(0, -1) : new Vector2Int(1, -1);
            case AttackDirection.UpRight: return isEvenColumn ? new Vector2Int(0, 1) : new Vector2Int(1, 1);
            case AttackDirection.DownLeft: return isEvenColumn ? new Vector2Int(-1, -1) : new Vector2Int(0, -1);
            case AttackDirection.DownRight: return isEvenColumn ? new Vector2Int(-1, 1) : new Vector2Int(0, 1);
            default: return Vector2Int.zero;
        }
    }

    public bool IsValidPosition(Vector2Int pos)
    {
        Vector3Int tilePos = new Vector3Int(pos.x, pos.y, 0);
        return tilemap.HasTile(tilePos); // Sprawdza, czy na tilemapie istnieje kafelek na tej pozycji
    }

    public void AddActionToStack(ActionData action)
    {
        actionStack.Push(action);
    }

    public int GetTokenOwner(string army)
    {
        if (army == player1Army) return 1;
        if (army == player2Army) return 2;

        Debug.LogWarning($"Nie znaleziono w³aœciciela dla armii: {army}");
        return CurrentPlayer;
    }

    public int GetHighestInitiative()
    {
        int maxInitiative = 0;

        foreach (var tokenEntry in tokenGrid)
        {
            Token token = tokenEntry.Value;
            if (token == null || token.currentInitiatives.Count == 0) continue;

            int highestUnitInitiative = token.currentInitiatives.Max();
            if (highestUnitInitiative > maxInitiative)
            {
                maxInitiative = highestUnitInitiative;
            }
        }

        return maxInitiative;
    }

    private void OnTurnChanged()
    {
        Debug.Log($"Tura Gracza {CurrentPlayer}");
        tokenManager.DrawTokens();
        tokenManager.UpdatePanelInteractivity();
        actionStack.Clear();
    }
}

/* Pierwsza próba stworzenia cmentarza i cofniêcia akcji z niego
    public Dictionary<string, int> graveyardPlayer1 = new Dictionary<string, int>();
    public Dictionary<string, int> graveyardPlayer2 = new Dictionary<string, int>();

    public void AddToGraveyard(TokenData tokenData, int player)
    {
        Dictionary<string, int> graveyard = (player == 1) ? graveyardPlayer1 : graveyardPlayer2;

        if (graveyard.ContainsKey(tokenData.tokenName))
        {
            graveyard[tokenData.tokenName]++;
        }
        else
        {
            graveyard[tokenData.tokenName] = 1;
        }

        Debug.Log($"¯eton {tokenData.tokenName} dodany do cmentarza gracza {player}.");
    }

    public void RemoveFromGraveyard(int player)
    {
        Dictionary<string, int> graveyard = (player == 1) ? graveyardPlayer1 : graveyardPlayer2;

        if (graveyard.Count == 0)
        {
            Debug.Log("Cmentarz jest pusty.");
            return;
        }

        string lastKey = graveyard.Keys.Last();

        // ZnajdŸ pierwszy pusty slot
        List<Image> slots = (player == 1) ? tokenManager.player1Slots : tokenManager.player2Slots;
        Image emptySlot = slots.FirstOrDefault(s => s.sprite == null);

        if (emptySlot != null)
        {
            TokenData restoredToken = tokenManager.GetTokenDataByName(lastKey, player);

            emptySlot.sprite = restoredToken.sprite;
            emptySlot.gameObject.name = restoredToken.tokenName;
            emptySlot.GetComponent<Slot>().assignedToken = restoredToken;

            graveyard[lastKey]--;
            if (graveyard[lastKey] <= 0) graveyard.Remove(lastKey);

            Debug.Log($"Cofniêto akcjê: ¯eton {lastKey} wróci³ do slotu gracza {player}.");
        }
        else
        {
            Debug.Log("Brak wolnych slotów do przywrócenia ¿etonu!");
        }
    }
    public void DestroyToken(Token token)
    {
        if (token == null) return;

        AddToGraveyard(token.tokenData, CurrentPlayer);

        if (token.gameObject != null)
        {
            Destroy(token.gameObject);
        }
    }

*/