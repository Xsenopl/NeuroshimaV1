using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;


public class BoardManager : MonoBehaviour
{
    public Tilemap tilemap;  // Tilemapa reprezentuj¹ca planszê
    public GameObject tokenPrefab;
    public TokenSlotManager tokenManager;
    private Dictionary<Vector3Int, GameObject> occupiedTiles = new Dictionary<Vector3Int, GameObject>();  // Zajête heksy
    public Dictionary<Vector2Int, Token> tokenGrid = new Dictionary<Vector2Int, Token>();

    private void Start()
    {
        // Debug.Log("Plansza za³adowana. Gotowa do umieszczania ¿etonów.");
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
        tokenManager.RemoveTokenFromSlot(FindSlotByToken(tokenData));
        Debug.Log($"¯eton {tokenData.tokenName} umieszczony na {hexPosition}");
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
}

/* Po wierszach
        bool isEvenColumn = hexCoords.x % 2 == 0;

        switch (direction)
        {
            case AttackDirection.Up: return new Vector2Int(1, 0);
            case AttackDirection.Down: return new Vector2Int(-1, 0);
            case AttackDirection.UpLeft: return isEvenColumn ? new Vector2Int(-1, -1) : new Vector2Int(0, -1);
            case AttackDirection.UpRight: return isEvenColumn ? new Vector2Int(-1, 1) : new Vector2Int(0, 1);
            case AttackDirection.DownLeft: return isEvenColumn ? new Vector2Int(0, -1) : new Vector2Int(1, -1);
            case AttackDirection.DownRight: return isEvenColumn ? new Vector2Int(0, 1) : new Vector2Int(1, 1);
            default: return Vector2Int.zero;
        }
*/
