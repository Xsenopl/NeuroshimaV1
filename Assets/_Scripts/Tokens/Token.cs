using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Token : MonoBehaviour
{
    public TokenData tokenData;
    public int currentHealth;
    public List<int> currentInitiatives;
    public List<DirectionalFeatures> currentAttackEffects;
    public List<Features> currentFeatures = new List<Features>();
    public Dictionary<Vector2Int, bool> neighborStatus = new Dictionary<Vector2Int, bool>(); // S¹siedztwo (hexCoords -> zajêty / pusty)
    public Vector2Int hexCoords; // Wspó³rzêdne ¿etonu w uk³adzie heksagonalnym
    public bool isPlaced = false;

    private bool isBeingRotated = false;
    private Vector3 initialMousePosition;
    public float currentRotation = 0f;
    public GameObject rotationAreaPrefab;
    private GameObject rotationArea;
    private CircleCollider2D circleCollider;

    private SpriteRenderer spriteRenderer;

    private static readonly Vector2Int[] evenRowOffsets = {
        new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 1),
        new Vector2Int(-1, 0), new Vector2Int(-1, -1), new Vector2Int(0, -1)
    };

    private static readonly Vector2Int[] oddRowOffsets = {
        new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(0, 1),
        new Vector2Int(-1, 0), new Vector2Int(0, -1), new Vector2Int(1, -1)
    };

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        circleCollider = GetComponent<CircleCollider2D>();
    }

    void Start()
    { }

    public void Initialize(TokenData data, Vector2Int position, Dictionary<Vector2Int, Token> tokenGrid = null)
    {
        tokenData = data;
        spriteRenderer.sprite = data.sprite;  // Za³adowanie grafiki z ScriptableObject
        hexCoords = position;
        currentHealth = data.health;
        currentInitiatives = new List<int>(data.initiatives);

        currentAttackEffects = new List<DirectionalFeatures>();
        InitializeCurrentEffects();
        InitializeCurrentFeatures();
        InitializeNeighbors(tokenGrid);

        Debug.Log($"{tokenData.tokenName} zosta³ zainicjalizowany. Pocz¹tkowe attackEffects: " +
               $"{string.Join(" | ", currentAttackEffects.Select(e => $"Kierunek: {e.direction}, Ataki: {string.Join(", ", e.attacks.Select(a => a.attackPower))}"))}");
    }

    private void InitializeCurrentEffects()
    {
        if (tokenData.directionFeatures == null) return;

        foreach (var effect in tokenData.directionFeatures)
        {
            if (effect.attacks == null || effect.attacks.Count == 0) continue; // Unika b³êdu dla pustych list

            // Sprawdzenie, czy `currentAttackEffects` zawiera ju¿ wpis dla danego kierunku
            var existingEffect = currentAttackEffects.FirstOrDefault(e => e.direction == effect.direction);

            if (existingEffect.attacks != null && existingEffect.attacks.Count > 0) // Jeœli kierunek istnieje, to dodaje `AttackFeatures`
            {
                foreach (var tokenEffect in effect.attacks)
                {
                    AttackFeatures newTokenEffect = new AttackFeatures
                    {
                        attackPower = tokenEffect.attackPower,
                        isRanged = tokenEffect.isRanged,
                    };

                    existingEffect.attacks.Add(newTokenEffect);
                }
            }
            else // Jeœli nie istnieje, tworzy nowy wpis
            {
                DirectionalFeatures newEffect = new DirectionalFeatures
                {
                    direction = effect.direction,
                    attacks = new List<AttackFeatures>()
                };

                foreach (var tokenEffect in effect.attacks)
                {
                    AttackFeatures newTokenEffect = new AttackFeatures
                    {
                        attackPower = tokenEffect.attackPower,
                        isRanged = tokenEffect.isRanged,
                    };

                    newEffect.attacks.Add(newTokenEffect);
                }

                currentAttackEffects.Add(newEffect);
            }
        }
    }

    public void InitializeCurrentFeatures()
    {
        currentFeatures.Clear();
        foreach (var feature in tokenData.tokenFeatures)
        {
            currentFeatures.Add(new Features
            {
                feature = feature.feature,
                quantity = feature.quantity
            });
        }
    }

    // Inicjalizacja s¹siedztwa na podstawie tokenGrid
    public void InitializeNeighbors(Dictionary<Vector2Int, Token> tokenGrid)
    {
        if (tokenGrid == null) { Debug.Log("nie ma przyjació³"); return; }
        neighborStatus.Clear();
        Vector2Int[] offsets = (hexCoords.y % 2 == 0) ? evenRowOffsets : oddRowOffsets;

        foreach (var offset in offsets)
        {
            Vector2Int neighborPos = hexCoords + offset;
            bool isOccupied = tokenGrid.ContainsKey(neighborPos);
            neighborStatus[neighborPos] = isOccupied;
        }
        //Debug.Log($"{tokenData.name} s¹siadów: {neighborStatus.Values.Count(t =>t)}");
    }

    void Update()
    {
        // Rotowanie ¿etonem globalne
        // Sprawdzenie, czy klikniêcie by³o wewn¹trz Circle Collidera
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0;

        
        if (Input.GetMouseButtonDown(0)) // Klikniêcie LPM
        {
            if (circleCollider.OverlapPoint(mouseWorldPosition))
            {
                // Klikniêto w œrodek ¿etonu -> zatwierdzamy
                if (!isPlaced)
                {
                    Destroy(rotationArea);
                    ConfirmPlacement();
                }
            }
            else
            {
                // Klikniêto poza Colliderem -> rozpocznij obracanie
                if (!isPlaced)
                {
                    isBeingRotated = true;
                    initialMousePosition = mouseWorldPosition; // Zapamiêtujemy pocz¹tkow¹ pozycjê myszy
                }
            }
        }
        if (Input.GetMouseButtonUp(0)) // Puszczenie LPM
        {
            if (isBeingRotated)
            {
                StopRotation();
                isBeingRotated = false; // Koniec obracania
            }
        }


        if (isBeingRotated && !isPlaced)
        {
            RotateTokenWithMouse();
        }
    }

    public void InitializeRotationArea()
    {
        if (rotationAreaPrefab != null)
        {
            rotationArea = Instantiate(rotationAreaPrefab, transform.position, Quaternion.identity);
            rotationArea.transform.SetParent(transform);
            rotationArea.GetComponent<RotationArea>().SetToken(this);
        }
    }

//_______________ROTOWANIE ¯ETONEM GLOBALNE__________________
    void RotateTokenWithMouse()
    {
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0;

        // Oblicza k¹t miêdzy myszk¹ a œrodkiem ¿etonu
        Vector3 direction = mouseWorldPosition - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Obrót ¿etonu
        transform.rotation = Quaternion.Euler(0, 0, angle);
        currentRotation = angle;
    }
    private void OnMouseDown()
    {
        if (isPlaced)
        {
            Debug.Log("¯eton ju¿ zatwierdzony.");
        }
    }

    void OnMouseUp()
    {
        if (isBeingRotated && !isPlaced)
        {
            isBeingRotated = false;
            StopRotation();
        }
    }

    public void StartRotationMode()
    {
        Debug.Log($"{tokenData.tokenName} jest w trybie rotacji.");
    }


    public void StartRotation(Vector3 mousePosition)    //Tylko RotationArea
    {
        initialMousePosition = mousePosition;
        isBeingRotated = true;
        Debug.Log("Rozpoczêto obracanie ¿etonu.");
    }

    public void StopRotation()
    {
        // Zaokr¹glenie k¹ta do najbli¿szych 60 stopni
        float roundedAngle = Mathf.Round(currentRotation / 60f) * 60f;
        transform.rotation = Quaternion.Euler(0, 0, roundedAngle);
        currentRotation = roundedAngle;

        //Debug.Log($"Obracanie zatrzymane. K¹t: {currentRotation}");
    }
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

/* Stara wersja rotowania ¿etonem
        public void StartRotationPREW(Vector3 mousePosition)
        {
            if (!isPlaced)
            {
                isBeingRotated = true;
                initialMousePosition = mousePosition;
            }
        }

        public void StopRotationPREW()
        {
            //isBeingRotated = false;

            //// Zaokr¹glenie k¹ta obrotu do najbli¿szej wielokrotnoœci 60 stopni
            //float roundedAngle = Mathf.Round(currentRotation / 60f) * 60f;

            //// Ustawienie obrotu na zaokr¹glony k¹t
            //transform.rotation = Quaternion.Euler(0, 0, roundedAngle);
            //currentRotation = roundedAngle;

            //Debug.Log($"Obracanie zatrzymane. Aktualny k¹t: {currentRotation}");

            isBeingRotated = false;
            currentRotation = transform.rotation.eulerAngles.z; // Zapamiêtanie aktualnego k¹ta
            float roundedAngle = Mathf.Round(currentRotation / 60f) * 60f; // Zaokr¹glenie do 60 stopni
            transform.rotation = Quaternion.Euler(0, 0, roundedAngle);

            Debug.Log($"Obracanie zatrzymane. Zaokr¹glony k¹t: {roundedAngle}");
        }

        void RotateTokenWithMousePREW()
        {
            //Vector3 mouseDelta = Input.mousePosition - initialMousePosition;
            //float angle = Mathf.Atan2(mouseDelta.y, mouseDelta.x) * Mathf.Rad2Deg;
            //transform.rotation = Quaternion.Euler(0, 0, angle);
            //currentRotation = angle;

            Vector3 currentMousePosition = Input.mousePosition;

            // Obliczenie k¹ta pocz¹tkowego i aktualnego wzglêdem pozycji ¿etonu
            Vector3 tokenScreenPosition = Camera.main.WorldToScreenPoint(transform.position);
            float initialAngle = Mathf.Atan2(initialMousePosition.y - tokenScreenPosition.y, initialMousePosition.x - tokenScreenPosition.x) * Mathf.Rad2Deg;
            float currentAngle = Mathf.Atan2(currentMousePosition.y - tokenScreenPosition.y, currentMousePosition.x - tokenScreenPosition.x) * Mathf.Rad2Deg;

            // Obliczenie ró¿nicy k¹ta
            float angleDelta = Mathf.DeltaAngle(initialAngle, currentAngle);

            // Aktualizacja k¹ta
            transform.rotation = Quaternion.Euler(0, 0, currentRotation + angleDelta);
        }

        void OnMouseDownPREV()
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                //if (isPlaced)
                //{
                //    Debug.Log("¯eton zatwierdzony.");
                //    Destroy(rotationArea);  // Usuniêcie pola do obracania
                //}
                //else
                //{
                //    isPlaced = true;
                //    Debug.Log("¯eton gotowy do zatwierdzenia. Kliknij ponownie, aby umieœciæ go na sta³e.");
                //}
                this.isPlaced = true;
                Debug.Log("¯eton zatwierdzony.");
                Destroy(rotationArea);  // Usuniêcie pola do obracania
            }
        }
    */

    public AttackDirection GetRotatedDirection(AttackDirection baseDirection)
    {
        AttackDirection[] directions = {
        AttackDirection.Up,
        AttackDirection.UpLeft,
        AttackDirection.DownLeft,
        AttackDirection.Down,
        AttackDirection.DownRight,
        AttackDirection.UpRight
        };

        // Obliczamy przesuniêcie w indeksach tablicy
        int shift = Mathf.RoundToInt(currentRotation / 60f) % 6;
        if (shift < 0) shift += 6; // Obs³uga negatywnych k¹tów

        // Znajdujemy indeks bazowego kierunku
        int baseIndex = Array.IndexOf(directions, baseDirection);
        if (baseIndex == -1) return baseDirection; // Jeœli kierunku nie znaleziono, zwróæ bazowy

        // Nowy indeks po obrocie
        int newIndex = (baseIndex + shift) % 6;

        //Debug.Log($"Kierunek: {directions[newIndex]}");
        return directions[newIndex];
    }

    public bool CanMove()
    {
        Features moveFeature = currentFeatures.Find(f => f.feature == TokenFeatures.Moving);
        return moveFeature.quantity > 0;
    }
    public bool CanMoveTo(Vector2Int newHexCoords)
    {
        return isPlaced && neighborStatus.ContainsKey(newHexCoords) && !neighborStatus[newHexCoords];
    }

    // Zmniejsza liczbê ruchów po przemieszczeniu
    public void UseMove()
    {
        int index = currentFeatures.FindIndex(f => f.feature == TokenFeatures.Moving);
        if (index >= 0)
        {
            currentFeatures[index] = new Features
            {
                feature = TokenFeatures.Moving,
                quantity = Mathf.Max(0, currentFeatures[index].quantity - 1)
            };
        }
    }
    // Resetuje liczbê ruchów
    public void ResetMoves()
    {
        int index = currentFeatures.FindIndex(f => f.feature == TokenFeatures.Moving);
        Debug.Log(index);
        if (index >= 0 && index < tokenData.tokenFeatures.Count)
        {
            currentFeatures[index] = new Features
            {
                feature = TokenFeatures.Moving,
                quantity = tokenData.tokenFeatures[index].quantity
            };
        }
    }

    public void ConfirmPlacement()
    {
        isPlaced = true;
        Debug.Log($"¯eton {tokenData.tokenName} umieszczony na {hexCoords}");

        if (tokenData.moduleEffects.Count > 0)
        {
            BoardManager boardManager = FindObjectOfType<BoardManager>();
            if (boardManager != null)
            {
                boardManager.SetModuleEffectsForAll(this, true);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            // Debug.Log($"{tokenData.tokenName} ma 0  hp.");
        }
    }

    public static explicit operator Token(GameObject v)
    {
        throw new NotImplementedException();
    }

    public List<Vector2Int> GetNeighborPositions()
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        foreach (var neighbor in neighborStatus)
        {
            if (neighbor.Value)
            {
                neighbors.Add(neighbor.Key);
            }
        }

        return neighbors;
    }

    //_______________MODU£Y__________________
    public void ApplyEffectToTarget(Token target, ModuleEffect effect)
    {
        switch (effect.effectType)
        {
            case ModuleEffectType.MeleeDamageBoost:
                target.BoostMeleeDamage(effect.value);
                break;
            case ModuleEffectType.RangedDamageBoost:
                target.BoostRangedDamage(effect.value);
                break;
            case ModuleEffectType.HealthBoost:
                target.IncreaseHealth(effect.value);
                break;
            case ModuleEffectType.InitiativeBoost:
                target.IncreaseInitiative(effect.value);
                break;
            case ModuleEffectType.InitiativeReduction:
                target.DecreaseInitiative(effect.value);
                break;
            case ModuleEffectType.ExtraInitiative:
                target.GainExtraInitiative(effect.value);
                break;
            case ModuleEffectType.GiveMovement:
                target.GainExtraMovement(effect.value);
                break;
            default:
                Debug.Log($"Efekt {effect.effectType} bêdzie implementowany w przysz³oœci.");
                break;
        }
    }
    public void RemoveEffectFromTarget(Token target, ModuleEffect effect)
    {
        switch (effect.effectType)
        {
            case ModuleEffectType.MeleeDamageBoost:
                target.BoostMeleeDamage(-effect.value);
                break;
            case ModuleEffectType.RangedDamageBoost:
                target.BoostRangedDamage(-effect.value);
                break;
            case ModuleEffectType.HealthBoost:
                target.IncreaseHealth(-effect.value);
                break;
            case ModuleEffectType.InitiativeBoost:
                target.DecreaseInitiative(effect.value);
                break;
            case ModuleEffectType.InitiativeReduction:
                target.IncreaseInitiative(effect.value); ;
                break;
            case ModuleEffectType.ExtraInitiative:
                target.RemoveLowestInitiative();
                break;
            case ModuleEffectType.GiveMovement:
                target.RemoveExtraMovement(effect.value);
                break;
        }
    }

    public void BoostMeleeDamage(int amount)
    {
        for (int i = 0; i < currentAttackEffects.Count; i++)
        {
            if (!currentAttackEffects[i].attacks.Any(a => a.isRanged))
            {
                for (int j = 0; j < currentAttackEffects[i].attacks.Count; j++)
                {
                    AttackFeatures modifiedAttack = currentAttackEffects[i].attacks[j]; // Pobranie kopii obiektu
                    modifiedAttack.attackPower += amount; // Modyfikacja
                    currentAttackEffects[i].attacks[j] = modifiedAttack; // Zapisanie zmodyfikowanego obiektu
                }
            }
        }
    }
    public void BoostRangedDamage(int amount)
    {
        for (int i = 0; i < currentAttackEffects.Count; i++)
        {
            if (currentAttackEffects[i].attacks.Any(a => a.isRanged))
            {
                for (int j = 0; j < currentAttackEffects[i].attacks.Count; j++)
                {
                    AttackFeatures modifiedAttack = currentAttackEffects[i].attacks[j]; // Pobranie kopii obiektu
                    modifiedAttack.attackPower += amount; // Modyfikacja
                    currentAttackEffects[i].attacks[j] = modifiedAttack; // Zapisanie zmodyfikowanego obiektu
                }
            }
        }
    }

    public void IncreaseHealth(int amount)
    {
        currentHealth += amount;
    }

    public void IncreaseInitiative(int amount)
    {
        if (currentInitiatives.Count == 0) return;

        int zeroCount = currentInitiatives.Count(i => i == 0);
        bool hasZeroInTokenData = tokenData.initiatives.Contains(0);

        if (zeroCount >= 2)
        {
            if (hasZeroInTokenData)
            {
                // Jeœli `tokenData.initiatives` zawiera zero, zwiêksza tylko wartoœci wiêksze od zera
                for (int i = 0; i < currentInitiatives.Count; i++)
                {
                    if (currentInitiatives[i] > 0)
                    {
                        currentInitiatives[i] += amount;
                    }
                }
            }
            else
            {
                // Jeœli `tokenData.initiatives` nie zawiera zera, zwiêksza jedno zero i ka¿d¹ wartoœæ > 0
                bool oneZeroChanged = false;
                for (int i = 0; i < currentInitiatives.Count; i++)
                {
                    if (currentInitiatives[i] == 0 && !oneZeroChanged)
                    {
                        currentInitiatives[i] += amount;
                        oneZeroChanged = true;
                    }
                    else if (currentInitiatives[i] > 0)
                    {
                        currentInitiatives[i] += amount;
                    }
                }
            }
        }
        else
        {
            // Normalnie zwiêkszamy ka¿d¹ inicjatywê
            for (int i = 0; i < currentInitiatives.Count; i++)
            {
                currentInitiatives[i] += amount;
            }
        }
    }
    public void DecreaseInitiative(int amount)
    {
        if (currentInitiatives.Count == 0) return;

        for (int i = 0; i < currentInitiatives.Count; i++)
        {
            currentInitiatives[i] = Mathf.Max(0, currentInitiatives[i] - amount);
        }
    }


    public void GainExtraInitiative(int amount)
    {
        if (currentInitiatives.Count == 0) return;

        int minInitiative = currentInitiatives.Min();
        int newInitiative = Mathf.Max(0, minInitiative - 1); // Nowa inicjatywa, ale nie mo¿e spaœæ poni¿ej 0

        currentInitiatives.Add(newInitiative);
    }
    public void RemoveLowestInitiative()
    {
        if (currentInitiatives.Count > 0)
        {
            int minInitiative = currentInitiatives.Min();

            // Usuwa najmniejsz¹ inicjatywê, jeœli jednostka ma inne lub min != 0
            if (minInitiative != 0 || currentInitiatives.Count > 1)
            {
                currentInitiatives.Remove(minInitiative);
            }
        }
    }

    public void GainExtraMovement(int amount)
    {
        int index = currentFeatures.FindIndex(f => f.feature == TokenFeatures.Moving);
        
        if (index >= 0) // Jeœli jednostka ma Moving
        {
            currentFeatures[index] = new Features
            {
                feature = TokenFeatures.Moving,
                quantity = currentFeatures[index].quantity + amount
            };
        }
        else    // Jeœli jednostka nie ma Moving
        {
            currentFeatures.Add(new Features
            {
                feature = TokenFeatures.Moving,
                quantity = amount
            });
        }
    }
    public void RemoveExtraMovement(int amount)
    {
        int index = currentFeatures.FindIndex(f => f.feature == TokenFeatures.Moving);

        if (index >= 0)
        {
            int baseMovement = tokenData.tokenFeatures
                .Where(f => f.feature == TokenFeatures.Moving)
                .Select(f => f.quantity)
                .DefaultIfEmpty(0)
                .First(); // Pobiera wartoœæ bazow¹ Moving z TokenData (jeœli istnieje)

            int newQuantity = Math.Max(0, currentFeatures[index].quantity - amount);

            if (newQuantity == 0 && baseMovement == 0)
            {
                // Jeœli jednostka nie mia³a Moving w TokenData, usuwamy cechê
                currentFeatures.RemoveAt(index);
            }
            else
            {
                // Jeœli jednostka mia³a Moving w TokenData, nie mo¿e spaœæ poni¿ej bazowej wartoœci
                currentFeatures[index] = new Features
                {
                    feature = TokenFeatures.Moving,
                    quantity = newQuantity
                };
            }
        }
    }
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

}