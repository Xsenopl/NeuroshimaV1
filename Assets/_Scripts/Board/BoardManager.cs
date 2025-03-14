using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;


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
    public PanelConfirmationController panelConfirmationController;
    public Dictionary<Vector2Int, Token> tokenGrid = new Dictionary<Vector2Int, Token>(); // Zajête heksy

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

        // Jeœli wybrano ¿eton do ruchu, ale klikniêto w inne pole – przesuñ go
        if (selectedTokenForMove != null)
        {
            Debug.LogWarning("¯eton w trakcie ruchu");
            if (selectedTokenForMove.CanMoveTo(hexCoords))
            {
                Debug.LogWarning("Mo¿e siê ruszyæ na to pole");
                MoveToken(selectedTokenForMove, hexCoords);
            }
            selectedTokenForMove = null; // Reset wyboru po ruchu
            return;
        }
        // Jeœli klikniêto ¿eton z Features – ustaw go jako aktywny do ruchu
        if (tokenGrid.TryGetValue(hexCoords, out Token clickedToken))
        {
            if (clickedToken.isPlaced && clickedToken.currentFeatures.Count > 0)
            {
                panelConfirmationController.ShowPanel(
                    () => ConfirmTokenSelection(clickedToken),
                    () => Debug.Log($"Anulowano wybór {clickedToken.tokenData.tokenName}")
                );
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

    public Token InstantiateToken(Vector3Int hexPosition)
    {
        Vector3 spawnPosition = HexToWorld(hexPosition);
        GameObject newTokenObj = Instantiate(tokenPrefab, spawnPosition, Quaternion.identity);
        Token newToken = newTokenObj.GetComponent<Token>();
        return newToken;
    }

    // Umieszczanie ¿etonu na heksie
    public void PlaceToken(Vector3Int hexPosition, TokenData tokenData, float? previousRotation = null)
    {
        Token newToken = InstantiateToken(hexPosition);
        Vector2Int hexCords = (Vector2Int)hexPosition;
        tokenGrid[hexCords] = newToken;
        newToken.Initialize(tokenData, hexCords, tokenGrid);

        if (previousRotation.HasValue)
        {
            newToken.transform.rotation = Quaternion.Euler(0, 0, previousRotation.Value);
            newToken.currentRotation = previousRotation.Value;
            Debug.Log($"Rotacja 1 {newToken.transform.rotation} i current to: {newToken.currentRotation}");
            newToken.ConfirmPlacement(); // ¯eton jest od razu zatwierdzony
        }
        else
        {
            newToken.InitializeRotationArea();
        }

        // Dodanie efektu modu³u, dla tej jednostki 
        List<Vector2Int> activeNeighbors = newToken.GetNeighborPositions();
        SetModuleEffectsForThis(newToken.hexCoords, activeNeighbors, true);

        Image originalSlot = FindSlotByToken(tokenData);

        if (!isUndoing)
        {
            if (originalSlot != null)   // Jeœli ¿eton jest wziêty ze slotu
            {
                AddActionToStack(new ActionData(tokenData, hexPosition, originalSlot));
                originalSlot.GetComponent<Slot>().ClearSlot();
            }
            else { Debug.Log("Cofniêcia dla PlaceToken nie dodano"); }
        }

        UpdateNeighbors((Vector2Int)hexPosition); // Aktualizacja s¹siedztwa u s¹siadów

        tokenManager.trashSlotImage.SetActive(false);
    }

    public void PlaceTokenMove(Vector3Int hexPosition, Token existingToken)
    {
        Vector3 spawnPosition = HexToWorld(hexPosition);

        Debug.Log(spawnPosition);

        // Przenosi istniej¹cy obiekt ¿etonu (zamiast tworzyæ nowy)
        existingToken.transform.position = spawnPosition;
        Vector2Int hexCoords = (Vector2Int)hexPosition;

        // Aktualizuje pozycjê w siatce tokenów
        tokenGrid[hexCoords] = existingToken;
        existingToken.hexCoords = hexCoords;

        existingToken.InitializeNeighbors(tokenGrid);
        existingToken.InitializeRotationArea();
        existingToken.isPlaced = false;

        // Aktualizuje s¹siedztwo po ruchu
        UpdateNeighbors(hexCoords);

        Debug.Log($"¯eton {existingToken.tokenData.tokenName} zosta³ przeniesiony na {hexPosition} bez resetowania jego wartoœci.");
    }

    public void RemoveToken(Token token)
    {
        if (token == null) return;

        // Sprawdzenie, czy ¿eton nadal istnieje w `tokenGrid`
        if (tokenGrid.ContainsKey(token.hexCoords))
        {
            SetModuleEffectsForAll(token, false);
            // Usuniêcie efektu modu³u, dla tej jednostki 
            List<Vector2Int> activeNeighbors = token.GetNeighborPositions();
            SetModuleEffectsForThis(token.hexCoords, activeNeighbors, false);

            tokenGrid.Remove(token.hexCoords); // Usuniêcie z mapy tokenów
        }

        // Zniszczenie obiektu w œwiecie gry
        if (token.gameObject != null)
        {
            UpdateNeighbors(token.hexCoords);
            Destroy(token.gameObject);
        }
    }
    public void DetachToken(Token token)
    {
        if (token == null) return;

        // Sprawdzenie, czy ¿eton nadal istnieje w `tokenGrid`
        if (tokenGrid.ContainsKey(token.hexCoords))
        {
            SetModuleEffectsForAll(token, false);
            // Usuniêcie efektu modu³u, dla tej jednostki 
            List<Vector2Int> activeNeighbors = token.GetNeighborPositions();
            SetModuleEffectsForThis(token.hexCoords, activeNeighbors, false);

            tokenGrid.Remove(token.hexCoords); // Usuniêcie z mapy tokenów
        }

        // Bez zniszczenia obiektu w œwiecie gry
        if (token.gameObject != null)
        {
            UpdateNeighbors(token.hexCoords);
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

        // Cofanie ruchu ¿etonu na planszy (poprzez stworzenie nowego obiektu na starym miejscu)
        if (lastAction.previousPosition.HasValue) 
        {
            isUndoing = true;
            if (tokenGrid.TryGetValue((Vector2Int)lastAction.position, out Token token))
            {
                Debug.Log($"Cofniêcie ruchu: {token.tokenData.tokenName} wraca na {lastAction.previousPosition}");

                RemoveToken(token);
            }
            PlaceToken((Vector3Int)lastAction.previousPosition.Value, lastAction.tokenData, lastAction.previousRotation);

            Vector2Int hexCoords = (Vector2Int)lastAction.previousPosition;
            Token newToken = tokenGrid[hexCoords];

            newToken.tokenData = lastAction.tokenData;
            newToken.hexCoords = hexCoords;
            newToken.currentHealth = lastAction.previousHealth;
            newToken.currentFeatures = new List<Features>(lastAction.previousFeatures);
            newToken.transform.rotation = Quaternion.Euler(0, 0, lastAction.previousRotation.Value);
            // Dodanie efektu modu³u, dla tej jednostki 
            List<Vector2Int> activeNeighbors = newToken.GetNeighborPositions();
            SetModuleEffectsForThis(newToken.hexCoords, activeNeighbors, true);
            Debug.Log($"Rotacja 2 {newToken.transform.rotation}");

            isUndoing = false;
        }
        // Cofanie ¿etonu do slotu
        else if (lastAction.originalSlot != null) 
        {
            List<Image> slots = (CurrentPlayer == 1) ? tokenManager.player1Slots : tokenManager.player2Slots;
            Image emptySlot = slots.FirstOrDefault(s => s.sprite == null);

            if (emptySlot != null)
            {
                emptySlot.sprite = lastAction.tokenData.sprite;
                emptySlot.gameObject.name = lastAction.tokenData.tokenName;
                emptySlot.GetComponent<Slot>().assignedToken = lastAction.tokenData;

                Debug.Log($"Cofniêto pobranie ¿etonu: {lastAction.tokenData.tokenName}");

                if(!lastAction.position.HasValue)
                {
                    // Usuniêcie ¿etonu z cmentarza
                    int ownerPlayer = GetTokenOwner(lastAction.tokenData.army);
                    StatsManager statsManager = FindObjectOfType<StatsManager>();
                    if (statsManager != null) { statsManager.RemoveFromGraveyard(lastAction.tokenData, ownerPlayer); }
                    Debug.Log($"Cofniêto odrzucony ¿eton: {lastAction.tokenData.tokenName}");
                }

                // Usuniêcie ¿etonu z planszy
                else if(tokenGrid.TryGetValue((Vector2Int)lastAction.position, out Token token))
                {
                    RemoveToken(token);
                    Debug.Log($"Cofniêto akcjê: {lastAction.tokenData.tokenName} wróci³ do slotu.");
                }
            }
            else
            {
                Debug.LogWarning("Brak wolnych slotów do przywrócenia ¿etonu.");
            }
        }
        else
        {
            Debug.LogWarning("Trzecia opcja cofania");
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

    // Akcja zatwierdzaj¹ca wybór ¿etonu (na razie tylko do ruchu) do ruchu
    private void ConfirmTokenSelection(Token token)
    {
        selectedTokenForMove = token;
        Debug.Log($"Zatwierdzono wybór {token.tokenData.tokenName}.");
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
        if (!token.CanMove()) return;   // Jeœli ¿eton nie ma ruchów, przerwij

        Vector2Int oldCoords = token.hexCoords;
        float oldRotation = token.transform.rotation.eulerAngles.z;

        Debug.Log($"Stara rotacja: {oldRotation}");

        DetachToken(token);
        token.hexCoords = newHexCoords;
        tokenGrid[newHexCoords] = token;

        PlaceTokenMove((Vector3Int)newHexCoords, token);

        AddActionToStack(new ActionData(token, (Vector3Int)oldCoords, oldRotation));

        token.UseMove();
        // Dodanie efektu modu³u, dla tej jednostki 
        List<Vector2Int> activeNeighbors = token.GetNeighborPositions();
        SetModuleEffectsForThis(token.hexCoords, activeNeighbors, true);


        Debug.Log($"¯eton {token.tokenData.tokenName} przesuniêty z {oldCoords} na {newHexCoords}");
    }

    private void OnTurnChanged()
    {
        Debug.Log($"Tura Gracza {CurrentPlayer}");
        foreach (var token in tokenGrid.Values)
        {
            token.ResetMoves();
        }
        //tokenManager.DrawTokens();
        tokenManager.UpdatePanelInteractivity();
        actionStack.Clear();
    }

    //_______________MODU£Y__________________

    //Druga wersja uproszczenia - dzia³a, choæ nie jestem pewny w 100%
    public void SetModuleEffectsForAll(Token moduleToken, bool add)
    {
        if (moduleToken.tokenData.moduleEffects == null || moduleToken.tokenData.moduleEffects.Count == 0) return;

        foreach (var effect in moduleToken.tokenData.moduleEffects)
        {
            foreach (var direction in effect.directions)
            {
                AttackDirection rotatedDir = moduleToken.GetRotatedDirection(direction);
                Vector2Int targetPos = moduleToken.hexCoords + GetHexDirection(moduleToken.hexCoords, rotatedDir);

                ProcessModuleEffects(moduleToken, targetPos, add);
            }
        }
    }

    public void SetModuleEffectsForThis(Vector2Int target, List<Vector2Int> neighborModules, bool add)
    {
        foreach (var modulePos in neighborModules)
        {
            if (!tokenGrid.TryGetValue(modulePos, out Token moduleToken)) continue;
            if (moduleToken.tokenData.moduleEffects == null || moduleToken.tokenData.moduleEffects.Count == 0) continue;
            
            ProcessModuleEffects(moduleToken, target, add);              
        }
    }

    private void ProcessModuleEffects(Token moduleToken, Vector2Int targetPos, bool add)
    {
        if (tokenGrid.TryGetValue(targetPos, out Token targetToken))
        {
            foreach (var effect in moduleToken.tokenData.moduleEffects)
            {
                foreach (var direction in effect.directions)
                {
                    AttackDirection rotatedDir = moduleToken.GetRotatedDirection(direction);
                    Vector2Int effectTarget = moduleToken.hexCoords + GetHexDirection(moduleToken.hexCoords, rotatedDir);

                    if (effectTarget == targetPos) // Jeœli efekt modu³u dzia³a na target
                    {
                        bool isEnemy = targetToken.tokenData.army != moduleToken.tokenData.army;
                        if ((effect.enemyTarger && isEnemy) || (!effect.enemyTarger && !isEnemy))
                        {
                            if (add)
                                moduleToken.ApplyEffectToTarget(targetToken, effect);
                            else
                                moduleToken.RemoveEffectFromTarget(targetToken, effect);
                        }
                    }
                }
            }
        }
    }
}

/* Wersja bez rozdzia³u czêœci wspólnej pomiêdzy SetModuleEffectsForAll i SetModuleEffectsForThis - dzia³a
    public void SetModuleEffectsForAll(Token moduleToken, bool add)
    {
        if (moduleToken.tokenData.moduleEffects == null || moduleToken.tokenData.moduleEffects.Count == 0) return;

        foreach (var effect in moduleToken.tokenData.moduleEffects)
        {
            foreach (var direction in effect.directions)
            {
                // Ustalanie rzeczywistego kierunku efektu uwzglêdniaj¹c obrót modu³u
                AttackDirection rotatedDir = moduleToken.GetRotatedDirection(direction);
                Vector2Int targetPos = moduleToken.hexCoords + GetHexDirection(moduleToken.hexCoords, rotatedDir);

                Debug.Log($"Kierunek rotacji: {rotatedDir} Kierunek modu³u: {targetPos.x},{targetPos.y}");

                if (tokenGrid.TryGetValue(targetPos, out Token targetToken))
                {
                    bool isEnemy = targetToken.tokenData.army != moduleToken.tokenData.army;
                    if ((effect.enemyTarger && isEnemy) || (!effect.enemyTarger && !isEnemy))
                    {
                        if (add)
                            moduleToken.ApplyEffectToTarget(targetToken, effect);
                        else
                            moduleToken.RemoveEffectFromTarget(targetToken, effect);
                    }
                }
            }
        }
    }

    public void SetModuleEffectsForThis(Vector2Int target, List<Vector2Int> neighborModules, bool add)
    {
        foreach (var modulePos in neighborModules)
        {
            if (!tokenGrid.TryGetValue(modulePos, out Token moduleToken)) continue; // Sprawdza, czy w danym s¹siednim polu jest modu³

            if (moduleToken.tokenData.moduleEffects == null || moduleToken.tokenData.moduleEffects.Count == 0) continue;

            foreach (var effect in moduleToken.tokenData.moduleEffects)
            {
                foreach (var direction in effect.directions)
                {
                    // Ustalanie rzeczywistego kierunku efektu uwzglêdniaj¹c obrót modu³u
                    AttackDirection rotatedDir = moduleToken.GetRotatedDirection(direction);
                    Vector2Int effectTarget = modulePos + GetHexDirection(modulePos, rotatedDir);

                    if (effectTarget == target) // Jeœli efekt modu³u dzia³a na target
                    {
                        bool isEnemy = tokenGrid[target].tokenData.army != moduleToken.tokenData.army;
                        if ((effect.enemyTarger && isEnemy) || (!effect.enemyTarger && !isEnemy))
                        {
                            if (add)
                                moduleToken.ApplyEffectToTarget(tokenGrid[target], effect);
                            else
                                moduleToken.RemoveEffectFromTarget(tokenGrid[target], effect);
                        }
                    }
                }
            }
        }
    }
*/
/*  Trzecia wersja uproszczenia. Nie nadaje efektów, jednostce, która wesz³a w zasiêg efektu i nie usuwa, gdy wysz³a
    public void SetModuleEffectsForAll(Token moduleToken, bool add)
    {
        if (moduleToken.tokenData.moduleEffects == null || moduleToken.tokenData.moduleEffects.Count == 0) return;

        ProcessModuleEffects(moduleToken, moduleToken.hexCoords, add);
    }

    public void SetModuleEffectsForThis(Vector2Int target, List<Vector2Int> neighborModules, bool add)
    {
        foreach (var modulePos in neighborModules)
        {
            if (!tokenGrid.TryGetValue(modulePos, out Token moduleToken)) continue;
            if (moduleToken.tokenData.moduleEffects == null || moduleToken.tokenData.moduleEffects.Count == 0) continue;

            ProcessModuleEffects(moduleToken, target, add);
        }
    }

    private void ProcessModuleEffects(Token moduleToken, Vector2Int basePos, bool add)
    {
        foreach (var effect in moduleToken.tokenData.moduleEffects)
        {
            foreach (var direction in effect.directions)
            {
                AttackDirection rotatedDir = moduleToken.GetRotatedDirection(direction);
                Vector2Int effectTarget = basePos + GetHexDirection(basePos, rotatedDir);

                if (tokenGrid.TryGetValue(effectTarget, out Token targetToken))
                {
                    bool isEnemy = targetToken.tokenData.army != moduleToken.tokenData.army;
                    if ((effect.enemyTarger && isEnemy) || (!effect.enemyTarger && !isEnemy))
                    {
                        if (add)
                            moduleToken.ApplyEffectToTarget(targetToken, effect);
                        else
                            moduleToken.RemoveEffectFromTarget(targetToken, effect);
                    }
                }
            }
        }
    }
*/