using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;


public class BoardManager : MonoBehaviour
{
    public Tilemap tilemap;  // Tilemapa reprezentuj�ca plansz�
    public GameObject tokenPrefab;
    //public TokenData selectedToken;
    public TokenSlotManager tokenManager;
    private Dictionary<Vector3Int, GameObject> occupiedTiles = new Dictionary<Vector3Int, GameObject>();  // Zaj�te heksy
    private Dictionary<Vector2Int, Token> tokenGrid = new Dictionary<Vector2Int, Token>();
    public float hexSize = 1.0f; // Rozmiar pojedynczego heksa (nale�y dopasowa� do Tilemap)

    private void Start()
    {
        // Debug.Log("Plansza za�adowana. Gotowa do umieszczania �eton�w.");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPosition.z = 0;  // Upewniamy si�, �e jest na p�aszczy�nie 2D
            SelectHex(worldPosition);
        }
    }

    // Zamiana pozycji �wiata na wsp�rz�dne heksagonalne
    public Vector3Int WorldToHex(Vector3 worldPosition)
    {
        return tilemap.WorldToCell(worldPosition);
    }

    // Zamiana wsp�rz�dnych heksagonalnych na pozycj� w �wiecie
    public Vector3 HexToWorld(Vector3Int hexPosition)
    {
        return tilemap.CellToWorld(hexPosition);
    }

    public Vector2Int WorldToHexCoords(Vector3 worldPos)
    {
        float q = (2.0f / 3.0f * worldPos.x) / hexSize;
        float r = (-1.0f / 3.0f * worldPos.x + Mathf.Sqrt(3) / 3.0f * worldPos.y) / hexSize;
        return CubeRound(q, r);
    }


    // Wybieranie heksu na podstawie pozycji �wiata
    public void SelectHex(Vector3 worldPosition)
    {
        Vector3Int hexPosition = WorldToHex(worldPosition);

        if (!tilemap.HasTile(hexPosition))
        {
            // Debug.Log("Klikni�to poza plansz�.");
            return;
        }

        if (occupiedTiles.ContainsKey(hexPosition))  // Sprawdzenie, czy heks jest zaj�ty
        {
            // Debug.Log("To pole jest ju� zaj�te!");
            return;
        }

        TokenData selectedToken = tokenManager.GetSelectedToken();
        if (selectedToken == null)
        {
            // Debug.Log("Nie wybrano �etonu!");
            return;
        }

        PlaceToken(hexPosition, selectedToken);

        // Po umieszczeniu �etonu zerujemy wybrany �eton
        tokenManager.ClearSelectedToken();
    }

    // Umieszczanie �etonu na heksie
    public void PlaceToken(Vector3Int hexPosition, TokenData tokenData)
    {
        Vector3 spawnPosition = HexToWorld(hexPosition);
        GameObject newToken = Instantiate(tokenPrefab, spawnPosition, Quaternion.identity);
        newToken.GetComponent<Token>().Initialize(tokenData);
        newToken.GetComponent<Token>().InitializeRotationArea();


        occupiedTiles.Add(hexPosition, newToken);
        tokenManager.RemoveTokenFromSlot(FindSlotByToken(tokenData));
        Debug.Log($"�eton {tokenData.tokenName} umieszczony na {hexPosition}");
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

    private Vector2Int CubeRound(float q, float r)
    {
        int rx = Mathf.RoundToInt(q);
        int ry = Mathf.RoundToInt(r);
        int rz = Mathf.RoundToInt(-q - r);

        float xDiff = Mathf.Abs(rx - q);
        float yDiff = Mathf.Abs(ry - r);
        float zDiff = Mathf.Abs(rz - (-q - r));

        if (xDiff > yDiff && xDiff > zDiff) rx = -ry - rz;
        else if (yDiff > zDiff) ry = -rx - rz;

        return new Vector2Int(rx, ry);
    }

    public void RegisterToken(Token token)//, Vector2Int hexCoords
    {
        Vector2Int hexCoords = WorldToHexCoords(token.transform.position);
        token.hexCoords = hexCoords;
        tokenGrid[hexCoords] = token;
        UpdateAllNeighbors();
    }

    public void UpdateAllNeighbors()
    {
        foreach (var token in tokenGrid.Values)
        {
            token.UpdateNeighbors(tokenGrid);
        }
    }
}





// Wersja umieszczania, gdzie jescze nie ma wyboru tokenu
/*
public void PlaceToken(Vector3Int hexPosition)
{
    if (selectedToken == null)
    {
        Debug.Log("Brak wybranego �etonu.");
        return;
    }

    Vector3 spawnPosition = HexToWorld(hexPosition);
    GameObject newToken = Instantiate(tokenPrefab, spawnPosition, Quaternion.identity);
    newToken.GetComponent<Token>().Initialize(selectedToken);

    occupiedTiles.Add(hexPosition, newToken);  // Dodanie zaj�tego pola do s�ownika

    Debug.Log($"�eton {selectedToken.tokenName} umieszczony na {hexPosition}");
}
*/
