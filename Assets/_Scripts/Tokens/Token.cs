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

    public Vector2Int hexCoords; // Wsp�rz�dne �etonu w uk�adzie heksagonalnym
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
        spriteRenderer.sprite = data.sprite;  // Za�adowanie grafiki z ScriptableObject
        currentHealth = data.health;
        currentInitiatives = new List<int>(data.initiatives);
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

        // Obliczamy przesuni�cie w indeksach tablicy
        int shift = Mathf.RoundToInt(currentRotation / 60f) % 6;
        if (shift < 0) shift += 6; // Obs�uga negatywnych k�t�w

        // Znajdujemy indeks bazowego kierunku
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