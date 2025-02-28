using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class AppliedModuleEffect
{
    public ModuleEffect effect;
    public Token sourceModule; // Modu�, kt�ry doda� ten efekt

    public AppliedModuleEffect(ModuleEffect effect, Token sourceModule)
    {
        this.effect = effect;
        this.sourceModule = sourceModule;
    }
}

public class Token : MonoBehaviour
{

    public TokenData tokenData;
    public int currentHealth;
    public List<int> currentInitiatives;
    public List<DirectionalEffects> currentAttackEffects;

    private bool isPlaced = false;
    private bool isBeingRotated = false;
    private Vector3 initialMousePosition;
    private float currentRotation = 0f;
    public GameObject rotationAreaPrefab;
    private GameObject rotationArea;
    private CircleCollider2D circleCollider;

    private SpriteRenderer spriteRenderer;

    public Vector2Int hexCoords; // Wsp�rz�dne �etonu w uk�adzie heksagonalnym
    private List<Token> neighbors = new List<Token>();
    
    public List<AppliedModuleEffect> appliedModuleEffects { get; private set; } = new List<AppliedModuleEffect>();

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

    private void Start()
    {
        //UpdateNeighbors();
        BoardManager boardManager = FindObjectOfType<BoardManager>();
        if (boardManager != null)
        {
            boardManager.RegisterToken(this);
            circleCollider = GetComponent<CircleCollider2D>();
        }
    }

    public void Initialize(TokenData data)
    {
        tokenData = data;
        spriteRenderer.sprite = data.sprite;  // Za�adowanie grafiki z ScriptableObject
        currentHealth = data.health;
        currentInitiatives = new List<int>(data.initiatives);

        currentAttackEffects = new List<DirectionalEffects>();
        InitializeCurrentEffects();

        Debug.Log($"{tokenData.tokenName} zosta� zainicjalizowany. Pocz�tkowe attackEffects: " +
               $"{string.Join(" | ", currentAttackEffects.Select(e => $"Kierunek: {e.direction}, Ataki: {string.Join(", ", e.effects.Select(a => a.attackPower))}"))}");
    }

    private void InitializeCurrentEffects()
    {
        if (tokenData.attackEffects == null) return;
        
        foreach (var effect in tokenData.attackEffects)
        {
            if (effect.effects == null || effect.effects.Count == 0) continue; // Unika b��du dla pustych list

            // Sprawdzenie, czy `currentAttackEffects` zawiera ju� wpis dla danego kierunku
            var existingEffect = currentAttackEffects.FirstOrDefault(e => e.direction == effect.direction);

            if (existingEffect.effects != null && existingEffect.effects.Count > 0) // Je�li kierunek istnieje, to dodaje `TokenEffect`
            {
                foreach (var tokenEffect in effect.effects)
                {
                    TokenEffect newTokenEffect = new TokenEffect
                    {
                        attackPower = tokenEffect.attackPower,
                        isRanged = tokenEffect.isRanged,
                        abilities = (SpecialAbility[])tokenEffect.abilities.Clone()
                    };

                    existingEffect.effects.Add(newTokenEffect);
                }
            }
            else // Je�li nie istnieje, tworzy nowy wpis
            {
                DirectionalEffects newEffect = new DirectionalEffects
                {
                    direction = effect.direction,
                    effects = new List<TokenEffect>()
                };

                foreach (var tokenEffect in effect.effects)
                {
                    TokenEffect newTokenEffect = new TokenEffect
                    {
                        attackPower = tokenEffect.attackPower,
                        isRanged = tokenEffect.isRanged,
                        abilities = (SpecialAbility[])tokenEffect.abilities.Clone()
                    };

                    newEffect.effects.Add(newTokenEffect);
                }

                currentAttackEffects.Add(newEffect);
            }
        }    
    }

    void Update()
    {
        // Rotowanie �etonem globalne
        // Sprawdzenie, czy klikni�cie by�o wewn�trz Circle Collidera
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0;

        if (Input.GetMouseButtonDown(0)) // Klikni�cie myszk�
        {
            if (circleCollider.OverlapPoint(mouseWorldPosition))
            {
                // Klikni�to w �rodek �etonu -> zatwierdzamy
                if (!isPlaced)
                {
                    isPlaced = true;
                    Destroy(rotationArea);
                    Debug.Log("�eton umieszczony.");
                }
            }
            else
            {
                // Klikni�to poza Colliderem -> rozpocznij obracanie
                if (!isPlaced)
                {
                    isBeingRotated = true;
                    initialMousePosition = mouseWorldPosition; // Zapami�tujemy pocz�tkow� pozycj� myszy
                }
            }
        }
        if (Input.GetMouseButtonUp(0)) // Puszczenie przycisku myszy
        {
            if (isBeingRotated)
            {
                StopRotation();
                isBeingRotated = false; // Koniec obracania
            }
        }


        if (isBeingRotated && !isPlaced)
        {
            //RotateTokenWithMouse();
            RotateTokenWithMouse2();
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



    // Pr�by rotowania globalnego
    void RotateTokenWithMouse2()
    {
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0;

        // Oblicz k�t mi�dzy myszk� a �rodkiem �etonu
        Vector3 direction = mouseWorldPosition - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Obr�t �etonu
        transform.rotation = Quaternion.Euler(0, 0, angle);
        currentRotation = angle;
    }
    void OnMouseDown()
    {
        if (isPlaced)
        {
            Debug.Log("�eton ju� zatwierdzony.");
            return;
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

    public void StartRotation(Vector3 mousePosition)
    {
        initialMousePosition = mousePosition;
        isBeingRotated = true;
        Debug.Log("Rozpocz�to obracanie �etonu.");
    }

    public void StopRotation()
    {
        // Zaokr�glenie k�ta do najbli�szych 60 stopni
        float roundedAngle = Mathf.Round(currentRotation / 60f) * 60f;
        transform.rotation = Quaternion.Euler(0, 0, roundedAngle);
        currentRotation = roundedAngle;

        Debug.Log($"Obracanie zatrzymane. K�t: {currentRotation}");
    }


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

        //// Zaokr�glenie k�ta obrotu do najbli�szej wielokrotno�ci 60 stopni
        //float roundedAngle = Mathf.Round(currentRotation / 60f) * 60f;

        //// Ustawienie obrotu na zaokr�glony k�t
        //transform.rotation = Quaternion.Euler(0, 0, roundedAngle);
        //currentRotation = roundedAngle;

        //Debug.Log($"Obracanie zatrzymane. Aktualny k�t: {currentRotation}");

        isBeingRotated = false;
        currentRotation = transform.rotation.eulerAngles.z; // Zapami�tanie aktualnego k�ta
        float roundedAngle = Mathf.Round(currentRotation / 60f) * 60f; // Zaokr�glenie do 60 stopni
        transform.rotation = Quaternion.Euler(0, 0, roundedAngle);

        Debug.Log($"Obracanie zatrzymane. Zaokr�glony k�t: {roundedAngle}");
    }

    void RotateTokenWithMouse()
    {
        //Vector3 mouseDelta = Input.mousePosition - initialMousePosition;
        //float angle = Mathf.Atan2(mouseDelta.y, mouseDelta.x) * Mathf.Rad2Deg;
        //transform.rotation = Quaternion.Euler(0, 0, angle);
        //currentRotation = angle;

        Vector3 currentMousePosition = Input.mousePosition;

        // Obliczenie k�ta pocz�tkowego i aktualnego wzgl�dem pozycji �etonu
        Vector3 tokenScreenPosition = Camera.main.WorldToScreenPoint(transform.position);
        float initialAngle = Mathf.Atan2(initialMousePosition.y - tokenScreenPosition.y, initialMousePosition.x - tokenScreenPosition.x) * Mathf.Rad2Deg;
        float currentAngle = Mathf.Atan2(currentMousePosition.y - tokenScreenPosition.y, currentMousePosition.x - tokenScreenPosition.x) * Mathf.Rad2Deg;

        // Obliczenie r�nicy k�ta
        float angleDelta = Mathf.DeltaAngle(initialAngle, currentAngle);

        // Aktualizacja k�ta
        transform.rotation = Quaternion.Euler(0, 0, currentRotation + angleDelta);
    }

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

        // Oblicza przesuni�cie w indeksach tablicy
        int shift = Mathf.RoundToInt(currentRotation / 60f) % 6;
        if (shift < 0) shift += 6; // Obs�uga negatywnych k�t�w

        // Znajduje indeks bazowego kierunku
        int baseIndex = Array.IndexOf(directions, baseDirection);
        if (baseIndex == -1) return baseDirection; // Je�li kierunku nie znaleziono, zwr�� bazowy

        // Nowy indeks po obrocie
        int newIndex = (baseIndex + shift) % 6;

        //Debug.Log($"Kierunek: {directions[newIndex]}");
        return directions[newIndex];
    }

    void OnMouseDownPREV()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            //if (isPlaced)
            //{
            //    Debug.Log("�eton zatwierdzony.");
            //    Destroy(rotationArea);  // Usuni�cie pola do obracania
            //}
            //else
            //{
            //    isPlaced = true;
            //    Debug.Log("�eton gotowy do zatwierdzenia. Kliknij ponownie, aby umie�ci� go na sta�e.");
            //}
            this.isPlaced = true;
            Debug.Log("�eton zatwierdzony.");
            Destroy(rotationArea);  // Usuni�cie pola do obracania
        }
    }

    // Zapisuje ka�dy s�siaduj�cy token do listy s�siad�w       --      nie dzia�a z powodu mechaniki Cofni�cia akcji
    public void UpdateNeighbors(Dictionary<Vector2Int, Token> tokenGrid)
    {
        neighbors.Clear();

        Vector2Int[] offsets = (hexCoords.y % 2 == 0) ? evenRowOffsets : oddRowOffsets;

        foreach (var offset in offsets)
        {
            Vector2Int neighborPos = hexCoords + offset;
            if (tokenGrid.TryGetValue(neighborPos, out Token neighbor))
            {
                neighbors.Add(neighbor);
            }
        }

        // Debug.Log($"�eton na {hexCoords} ma {neighbors.Count} s�siad�w.");
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            // Debug.Log($"{tokenData.tokenName} ma 0  hp.");
        }
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.blue;
    //    Gizmos.DrawWireSphere(transform.position, neighborCheckRadius);
    //}
}