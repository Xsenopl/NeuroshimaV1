using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Token : MonoBehaviour
{
    public int currentHealth;
    public List<int> currentInitiatives;

    private bool isBeingRotated = false;
    private bool isPlaced = false;
    private Vector3 initialMousePosition;
    private float currentRotation = 0f;
    public GameObject rotationAreaPrefab;
    private GameObject rotationArea;

    public TokenData tokenData;
    private SpriteRenderer spriteRenderer;

    public Vector2Int hexCoords; // Wspó³rzêdne w uk³adzie heksagonalnym
    private List<Token> neighbors = new List<Token>();

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
    }

    private void Start()
    {
        //UpdateNeighbors();
        BoardManager boardManager = FindObjectOfType<BoardManager>();
        if (boardManager != null)
        {
            boardManager.RegisterToken(this);
        }
    }

    public void Initialize(TokenData data)
    {
        tokenData = data;
        spriteRenderer.sprite = data.sprite;  // Za³adowanie grafiki z ScriptableObject
        currentHealth = data.health; // Kopiowanie zdrowia
        currentInitiatives = new List<int>(data.initiatives); // Kopiowanie inicjatyw
    }

    void Update()
    {
        if (isBeingRotated)
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
            Debug.Log("Pole do obracania utworzone.");
        }
    }

    public void StartRotation(Vector3 mousePosition)
    {
        if (!isPlaced)
        {
            isBeingRotated = true;
            initialMousePosition = mousePosition;
            Debug.Log("Rozpoczêto obracanie ¿etonu.");
        }
    }

    public void StopRotation()
    {
        isBeingRotated = false;

        // Zaokr¹glenie k¹ta obrotu do najbli¿szej wielokrotnoœci 60 stopni
        float roundedAngle = Mathf.Round(currentRotation / 60f) * 60f;

        // Ustawienie obrotu na zaokr¹glony k¹t
        transform.rotation = Quaternion.Euler(0, 0, roundedAngle);
        currentRotation = roundedAngle;

        Debug.Log($"Obracanie zatrzymane. Aktualny k¹t: {currentRotation}");
    }

    void RotateTokenWithMouse()
    {
        Vector3 mouseDelta = Input.mousePosition - initialMousePosition;
        float angle = Mathf.Atan2(mouseDelta.y, mouseDelta.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        currentRotation = angle;
    }

    public AttackDirection GetRotatedDirection(AttackDirection baseDirection)
    {
        // Kolejnoœæ kierunków zgodnie z ruchem wskazówek zegara w uk³adzie Flat-Top
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

        Debug.Log($"Kierunek: {directions[newIndex]}");
        return directions[newIndex];
    }

    void OnMouseDown()
    {
        if (isPlaced)
        {
            Debug.Log("¯eton zatwierdzony.");
            Destroy(rotationArea);  // Usuniêcie pola do obracania
        }
        else
        {
            isPlaced = true;
            Debug.Log("¯eton gotowy do zatwierdzenia. Kliknij ponownie, aby umieœciæ go na sta³e.");
        }
    }

    // Zapisuje ka¿dy s¹siaduj¹cy token do listy s¹siadów
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

        Debug.Log($"¯eton na {hexCoords} ma {neighbors.Count} s¹siadów.");
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