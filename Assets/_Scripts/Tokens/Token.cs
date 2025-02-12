using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Token : MonoBehaviour
{
    private bool isBeingRotated = false;
    private bool isPlaced = false;
    private Vector3 initialMousePosition;
    private float currentRotation = 0f;
    public GameObject rotationAreaPrefab;
    private GameObject rotationArea;

    public TokenData tokenData;
    private SpriteRenderer spriteRenderer;

    public Vector2Int hexCoords; // Wsp�rz�dne w uk�adzie heksagonalnym
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
        spriteRenderer.sprite = data.sprite;  // Za�adowanie grafiki z ScriptableObject
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
            Debug.Log("Rozpocz�to obracanie �etonu.");
        }
    }

    public void StopRotation()
    {
        isBeingRotated = false;

        // Zaokr�glenie k�ta obrotu do najbli�szej wielokrotno�ci 60 stopni
        float roundedAngle = Mathf.Round(currentRotation / 60f) * 60f;

        // Ustawienie obrotu na zaokr�glony k�t
        transform.rotation = Quaternion.Euler(0, 0, roundedAngle);
        currentRotation = roundedAngle;

        Debug.Log($"Obracanie zatrzymane. Aktualny k�t: {currentRotation}");
    }

    void RotateTokenWithMouse()
    {
        Vector3 mouseDelta = Input.mousePosition - initialMousePosition;
        float angle = Mathf.Atan2(mouseDelta.y, mouseDelta.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        currentRotation = angle;
    }

    void OnMouseDown()
    {
        if (isPlaced)
        {
            Debug.Log("�eton zatwierdzony.");
            Destroy(rotationArea);  // Usuni�cie pola do obracania
        }
        else
        {
            isPlaced = true;
            Debug.Log("�eton gotowy do zatwierdzenia. Kliknij ponownie, aby umie�ci� go na sta�e.");
        }
    }

    // Zapisuje ka�dy s�siaduj�cy token do listy s�siad�w
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

        Debug.Log($"�eton na {hexCoords} ma {neighbors.Count} s�siad�w.");
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.blue;
    //    Gizmos.DrawWireSphere(transform.position, neighborCheckRadius);
    //}
}