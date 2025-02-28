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

        UpdateModuleEffects();

        Debug.Log($"¯eton {tokenData.tokenName} umieszczony na {hexPosition}");
        tokenManager.trashSlotImage.SetActive(false);
    }

    public void RemoveToken(Token token)
    {
        if (token == null) return;

        // Sprawdzenie, czy ¿eton nadal istnieje w `tokenGrid`
        if (tokenGrid.ContainsKey(token.hexCoords))
        {
            // Sprawdzenie, czy jest modu³em
            if (token.tokenData.tokenType == TokenType.Module)
            {
                foreach (var tokenEntry in tokenGrid.Values)
                {
                    RemoveModuleEffectsFromUnit(tokenEntry, token);
                }
            }

            tokenGrid.Remove(token.hexCoords); // Usuniêcie z mapy tokenów
            occupiedTiles.Remove((Vector3Int)token.hexCoords);
        }

        // Zniszczenie obiektu w œwiecie gry
        if (token.gameObject != null)
        {
            Destroy(token.gameObject);
            UpdateModuleEffects();
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

    public List<Token> GetModulesAffectingUnit(Token unit)
    {
        List<Token> affectingModules = new List<Token>();

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
                    affectingModules.Add(potentialModule);
                }
            }
        }

        return affectingModules;
    }

    public void UpdateModuleEffects()
    {
        // Najpierw usuwamy wszystkie istniej¹ce efekty modu³ów
        foreach (var tokenEntry in tokenGrid)
        {
            Token unit = tokenEntry.Value;
            if (unit == null || unit.tokenData.tokenType == TokenType.Module) continue;

            unit.appliedModuleEffects.Clear(); // Resetowanie wszystkich efektów modu³ów
            unit.Initialize(unit.tokenData); // Resetowanie statystyk jednostki do wartoœci bazowych
        }

        // Teraz ponownie stosujemy efekty modu³ów
        foreach (var tokenEntry in tokenGrid)
        {
            Token unit = tokenEntry.Value;
            if (unit == null || unit.tokenData.tokenType == TokenType.Module) continue;

            List<Token> affectingModules = GetModulesAffectingUnit(unit);

            foreach (var module in affectingModules)
            {
                List<ModuleEffect> applicableEffects = GetApplicableModuleEffectsForUnit(module, unit);
                ApplyModuleEffectsToUnit(unit, applicableEffects, module);
            }
        }
    }

    public List<ModuleEffect> GetApplicableModuleEffectsForUnit(Token module, Token unit)
    {
        List<ModuleEffect> applicableEffects = new List<ModuleEffect>();

        foreach (var effect in module.tokenData.moduleEffects)
        {
            AttackDirection rotatedDir = module.GetRotatedDirection(effect.direction);
            Vector2Int moduleEffectDir = GetHexDirection(module.hexCoords, rotatedDir);
            Vector2Int targetPos = module.hexCoords + moduleEffectDir;

            if (targetPos == unit.hexCoords)
            {
                applicableEffects.Add(effect);
            }
        }

        return applicableEffects;
    }

    public void ApplyModuleEffectsToUnit(Token unit, List<ModuleEffect> effects, Token module)
    {
        if (unit == null) return;

        foreach (var effect in effects)
        {
            unit.appliedModuleEffects.Add(new AppliedModuleEffect(effect, module)); // Zapisujemy efekt dla tego modu³u

            switch (effect.effectType)
            {
                case ModuleEffectType.MeleeDamageBoost:
                    for (int i = 0; i < unit.currentAttackEffects.Count; i++)
                    {
                        for (int j = 0; j < unit.currentAttackEffects[i].effects.Count; j++)
                        {
                            if (!unit.currentAttackEffects[i].effects[j].isRanged)
                            {
                                unit.currentAttackEffects[i].effects[j] = new TokenEffect
                                {
                                    attackPower = unit.currentAttackEffects[i].effects[j].attackPower + effect.value,
                                    isRanged = unit.currentAttackEffects[i].effects[j].isRanged,
                                    abilities = (SpecialAbility[])unit.currentAttackEffects[i].effects[j].abilities.Clone()
                                };
                            }
                        }
                    }
                    break;

                case ModuleEffectType.RangedDamageBoost:
                    for (int i = 0; i < unit.currentAttackEffects.Count; i++)
                    {
                        for (int j = 0; j < unit.currentAttackEffects[i].effects.Count; j++)
                        {
                            if (unit.currentAttackEffects[i].effects[j].isRanged)
                            {
                                unit.currentAttackEffects[i].effects[j] = new TokenEffect
                                {
                                    attackPower = unit.currentAttackEffects[i].effects[j].attackPower + effect.value,
                                    isRanged = unit.currentAttackEffects[i].effects[j].isRanged,
                                    abilities = (SpecialAbility[])unit.currentAttackEffects[i].effects[j].abilities.Clone()
                                };
                            }
                        }
                    }
                    break;
            }
        }
    }

    public void RemoveModuleEffectsFromUnit(Token unit, Token module)
    {
        if (unit == null) return;

        // Znalezienie efektów, które by³y dodane przez ten konkretny modu³
        var effectsToRemove = unit.appliedModuleEffects.Where(e => e.sourceModule == module).ToList();

        foreach (var appliedEffect in effectsToRemove)
        {
            switch (appliedEffect.effect.effectType)
            {
                case ModuleEffectType.MeleeDamageBoost:
                    for (int i = 0; i < unit.currentAttackEffects.Count; i++)
                    {
                        for (int j = 0; j < unit.currentAttackEffects[i].effects.Count; j++)
                        {
                            if (!unit.currentAttackEffects[i].effects[j].isRanged)
                            {
                                unit.currentAttackEffects[i].effects[j] = new TokenEffect
                                {
                                    attackPower = unit.currentAttackEffects[i].effects[j].attackPower - appliedEffect.effect.value,
                                    isRanged = unit.currentAttackEffects[i].effects[j].isRanged,
                                    abilities = (SpecialAbility[])unit.currentAttackEffects[i].effects[j].abilities.Clone()
                                };
                            }
                        }
                    }
                    break;

                case ModuleEffectType.RangedDamageBoost:
                    for (int i = 0; i < unit.currentAttackEffects.Count; i++)
                    {
                        for (int j = 0; j < unit.currentAttackEffects[i].effects.Count; j++)
                        {
                            if (unit.currentAttackEffects[i].effects[j].isRanged)
                            {
                                unit.currentAttackEffects[i].effects[j] = new TokenEffect
                                {
                                    attackPower = unit.currentAttackEffects[i].effects[j].attackPower - appliedEffect.effect.value,
                                    isRanged = unit.currentAttackEffects[i].effects[j].isRanged,
                                    abilities = (SpecialAbility[])unit.currentAttackEffects[i].effects[j].abilities.Clone()
                                };
                            }
                        }
                    }
                    break;
            }

            unit.appliedModuleEffects.Remove(appliedEffect);
        }
    }


}