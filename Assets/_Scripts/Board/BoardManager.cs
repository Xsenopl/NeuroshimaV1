using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;


public class BoardManager : MonoBehaviour
{
    public Tilemap tilemap;  // Tilemapa reprezentuj�ca plansz�
    public GameObject tokenPrefab;
    public TokenSlotManager tokenManager;
    private Dictionary<Vector3Int, GameObject> occupiedTiles = new Dictionary<Vector3Int, GameObject>();  // Zaj�te heksy
    private Dictionary<Vector2Int, Token> tokenGrid = new Dictionary<Vector2Int, Token>();

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

    // Zapisuje tokeny do tokenGrid i nakazuje dla nich aktualizacj� ich s�siad�w 
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
}
