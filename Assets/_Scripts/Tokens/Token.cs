using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using static UnityEngine.RuleTile.TilingRuleOutput;

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
    private CircleCollider2D circleCollider;

    public TokenData tokenData;
    private SpriteRenderer spriteRenderer;

    public Vector2Int hexCoords; // Wspó³rzêdne ¿etonu w uk³adzie heksagonalnym
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
        spriteRenderer.sprite = data.sprite;  // Za³adowanie grafiki z ScriptableObject
        currentHealth = data.health;
        currentInitiatives = new List<int>(data.initiatives);
    }

    void Update()
    {
        // Rotowanie ¿etonem globalne
        // Sprawdzenie, czy klikniêcie by³o wewn¹trz Circle Collidera
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0;

        if (Input.GetMouseButtonDown(0)) // Klikniêcie myszk¹
        {
            if (circleCollider.OverlapPoint(mouseWorldPosition))
            {
                // Klikniêto w œrodek ¿etonu -> zatwierdzamy
                if (!isPlaced)
                {
                    isPlaced = true;
                    Destroy(rotationArea);
                    Debug.Log("¯eton umieszczony.");
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



    // Próby rotowania globalnego
    void RotateTokenWithMouse2()
    {
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0;

        // Oblicz k¹t miêdzy myszk¹ a œrodkiem ¿etonu
        Vector3 direction = mouseWorldPosition - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Obrót ¿etonu
        transform.rotation = Quaternion.Euler(0, 0, angle);
        currentRotation = angle;
    }
    void OnMouseDown()
    {
        if (isPlaced)
        {
            Debug.Log("¯eton ju¿ zatwierdzony.");
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
        Debug.Log("Rozpoczêto obracanie ¿etonu.");
    }

    public void StopRotation()
    {
        // Zaokr¹glenie k¹ta do najbli¿szych 60 stopni
        float roundedAngle = Mathf.Round(currentRotation / 60f) * 60f;
        transform.rotation = Quaternion.Euler(0, 0, roundedAngle);
        currentRotation = roundedAngle;

        Debug.Log($"Obracanie zatrzymane. K¹t: {currentRotation}");
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

    void RotateTokenWithMouse()
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

    // Zapisuje ka¿dy s¹siaduj¹cy token do listy s¹siadów       --      nie dzia³a z powodu mechaniki Cofniêcia akcji
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

        // Debug.Log($"¯eton na {hexCoords} ma {neighbors.Count} s¹siadów.");
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