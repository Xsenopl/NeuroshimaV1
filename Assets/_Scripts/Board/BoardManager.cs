using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime;
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
    public Dictionary<Vector2Int, Token> tokenGrid = new Dictionary<Vector2Int, Token>();
    private Dictionary<Vector3Int, GameObject> occupiedTiles = new Dictionary<Vector3Int, GameObject>();  // Zajête heksy

    public Token selectedTokenForMove = null;
    private Stack<ActionData> actionStack = new Stack<ActionData>();
    private bool isUndoing = false; // true - akcja bez cofania, false - mo¿na

    private static readonly Vector2Int[] evenRowOffsets = {
        new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 1),
        new Vector2Int(-1, 0), new Vector2Int(-1, -1), new Vector2Int(0, -1)
    };

    private static readonly Vector2Int[] oddRowOffsets = {
        new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(0, 1),
        new Vector2Int(-1, 0), new Vector2Int(0, -1), new Vector2Int(1, -1)
    };

    private void Start()
    {
        // Debug.Log("Plansza za³adowana. Gotowa do umieszczania ¿etonów.");
        player1Army = "Borgo";
        player2Army = "Outpost";
        //Debug.Log($"Dla 1 jest {player1Army}.   Dla 2 jest {player2Army}");
        Debug.Log("Tura Gracza "+CurrentPlayer);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPosition.z = 0;  // Upewnia siê, ¿e jest na p³aszczyŸnie 2D
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
        Vector2Int hexCoords = (Vector2Int)hexPosition;

        if (!tilemap.HasTile(hexPosition)) return; //Poza plansz¹

        if (occupiedTiles.ContainsKey(hexPosition))  // Sprawdzenie, czy heks jest zajêty
        {
            //Debug.Log("To pole jest ju¿ zajête!");
            //return;
        }


        if (selectedTokenForMove != null)
        {
            Debug.LogWarning("Pierwszy If");
            if (selectedTokenForMove.CanMoveTo(hexCoords))
            {
                Debug.LogWarning("Pó³tora If");
                MoveToken(selectedTokenForMove, hexCoords);
            }
            selectedTokenForMove = null; // Reset wyboru po ruchu
            return;
        }
        if (tokenGrid.TryGetValue(hexCoords, out Token clickedToken))
        {
            if (clickedToken.isPlaced && clickedToken.tokenData.tokenFeatures.Contains(TokenFeatures.Moving))
            {
                selectedTokenForMove = clickedToken;
                Debug.Log($"Wybrano {clickedToken.tokenData.tokenName} do ruchu.");
            }
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
    public void PlaceToken(Vector3Int hexPosition, TokenData tokenData, Vector3Int? previousPosition = null)
    {
        Vector3 spawnPosition = HexToWorld(hexPosition);
        GameObject newTokenObj = Instantiate(tokenPrefab, spawnPosition, Quaternion.identity);
        Token newToken = newTokenObj.GetComponent<Token>();
        Vector2Int hexCords = (Vector2Int)hexPosition;
        tokenGrid[hexCords] = newToken;
        newToken.Initialize(tokenData, (Vector2Int)hexPosition, tokenGrid);
        newToken.InitializeRotationArea();
        
        occupiedTiles.Add(hexPosition, newTokenObj);

        Image originalSlot = FindSlotByToken(tokenData);

        if (!isUndoing)
        {
            if (originalSlot != null)
            {
                AddActionToStack(new ActionData(tokenData, hexPosition, originalSlot));
                originalSlot.GetComponent<Slot>().ClearSlot();
            }
            else if (previousPosition.HasValue) // Jeœli ¿eton siê przemieszcza
            {
                Debug.Log("Jako akcja ruchu");
                AddActionToStack(new ActionData(tokenData, hexPosition, previousPosition.Value));
            }
            else { Debug.Log("Cofniêcia dla PlaceToken nie dodano"); }
        }

        UpdateNeighbors((Vector2Int)hexPosition); // Aktualizacja s¹siedztwa

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
            UpdateNeighbors(token.hexCoords);
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

    // Zapisuje tokeny do tokenGrid
    public void RegisterToken(Token token)//, Vector2Int hexCoords
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int hexCoords = WorldToHex(worldPosition);
        token.hexCoords = (Vector2Int)hexCoords;
        tokenGrid[(Vector2Int)hexCoords] = token;
    }

    public void UpdateNeighbors(Vector2Int changedHex)
    {
        Vector2Int[] offsets = (changedHex.y % 2 == 0) ? evenRowOffsets : oddRowOffsets;

        foreach (var offset in offsets)
        {
            Vector2Int neighborPos = changedHex + offset;
            if (tokenGrid.TryGetValue(neighborPos, out Token neighborToken))
            {
                neighborToken.InitializeNeighbors(tokenGrid); // Aktualizacja s¹siedztwa s¹siada
            }
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

        if (lastAction.previousPosition.HasValue) // Cofanie ruchu ¿etonu na planszy
            {
                if (tokenGrid.TryGetValue((Vector2Int)lastAction.position, out Token token))
                {
                    Debug.Log($"Cofniêcie ruchu: {token.tokenData.tokenName} wraca na {lastAction.previousPosition}");

                    isUndoing = true;
                    MoveToken(token, (Vector2Int)lastAction.previousPosition);
                    isUndoing = false;
                }
            }
        //else if (lastAction.position != Vector3Int.zero) // Cofanie ¿etonu z planszy
        //{
        //    // Usuwa ¿eton z planszy
        //    if (tokenGrid.TryGetValue((Vector2Int)lastAction.position, out Token tokenObject))
        //    {
        //        //Destroy(tokenObject);
        //        //occupiedTiles.Remove(lastAction.position);
        //        //tokenGrid.Remove((Vector2Int)lastAction.position);

        //        RemoveToken(tokenObject.GetComponent<Token>());
        //    }


        //    // Przywracam ¿eton do jego oryginalnego slotu
        //    lastAction.originalSlot.sprite = lastAction.token.sprite;
        //    lastAction.originalSlot.gameObject.name = lastAction.token.tokenName;
        //    lastAction.originalSlot.GetComponent<Slot>().assignedToken = lastAction.token;

        //    Debug.Log($"Cofniêto akcjê: {lastAction.token.tokenName} wróci³ do slotu.");
        //}
        else if (lastAction.originalSlot != null) // Cofanie ¿etonu do slotu
        {
            List<Image> slots = (CurrentPlayer == 1) ? tokenManager.player1Slots : tokenManager.player2Slots;
            Image emptySlot = slots.FirstOrDefault(s => s.sprite == null);

            if (emptySlot != null)
            {
                emptySlot.sprite = lastAction.token.sprite;
                emptySlot.gameObject.name = lastAction.token.tokenName;
                emptySlot.GetComponent<Slot>().assignedToken = lastAction.token;

                Debug.Log($"Cofniêto pobranie ¿etonu: {lastAction.token.tokenName}");

                if(!lastAction.position.HasValue)
                {
                    // Usuniêcie ¿etonu z cmentarza
                    int ownerPlayer = GetTokenOwner(lastAction.token.army);
                    StatsManager statsManager = FindObjectOfType<StatsManager>();
                    if (statsManager != null) { statsManager.RemoveFromGraveyard(lastAction.token, ownerPlayer); }
                    Debug.Log($"Cofniêto odrzucony ¿eton: {lastAction.token.tokenName}");
                }

                // Usuniêcie ¿etonu z planszy
                else if(tokenGrid.TryGetValue((Vector2Int)lastAction.position, out Token token))
                {
                    RemoveToken(token);
                    Debug.Log($"Cofniêto akcjê: {lastAction.token.tokenName} wróci³ do slotu.");
                }
            }
            else
            {
                Debug.LogWarning("Brak wolnych slotów do przywrócenia ¿etonu.");
            }
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

    public void MoveToken(Token token, Vector2Int newHexCoords)
    {
        if (!tokenGrid.ContainsKey(token.hexCoords)) return; // Jeœli ¿eton nie istnieje w siatce, przerwij

        Vector2Int oldCoords = token.hexCoords;

        RemoveToken(token);
        token.hexCoords = newHexCoords;
        tokenGrid[newHexCoords] = token;
        PlaceToken((Vector3Int)newHexCoords, token.tokenData, (Vector3Int)oldCoords);

        Debug.Log($"¯eton {token.tokenData.tokenName} przesuniêty z {oldCoords} na {newHexCoords}");
    }

    private void OnTurnChanged()
    {
        Debug.Log($"Tura Gracza {CurrentPlayer}");
        //tokenManager.DrawTokens();
        tokenManager.UpdatePanelInteractivity();
        actionStack.Clear();
    }

}


/*
 // ________________Prace nad modu³ami________________
    public List<ModuleEffect> GetModuleEffectsForUnit(Token unit)
    {
        List<ModuleEffect> appliedEffects = new List<ModuleEffect>();

        foreach (var tokenEntry in tokenGrid)
        {
            Token potentialModule = tokenEntry.Value;
            if (potentialModule == null || potentialModule.tokenData.tokenType != TokenType.Module)
                continue;

            foreach (var effect in potentialModule.tokenData.moduleEffects)
            {
                AttackDirection rotatedDir = potentialModule.GetRotatedDirection(effect.direction);
                Vector2Int moduleEffectDir = GetHexDirection(potentialModule.hexCoords, rotatedDir);
                Vector2Int targetPos = potentialModule.hexCoords + moduleEffectDir;

                if (targetPos == unit.hexCoords)
                {
                    appliedEffects.Add(effect);
                    Debug.Log($"MODU£ {potentialModule.tokenData.tokenName} wp³ywa na {unit.tokenData.tokenName} z efektem {effect.effectType} +{effect.value}");
                }
            }
        }

        return appliedEffects;
    }
*/