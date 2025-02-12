using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HexGrid : MonoBehaviour
{
    public Tilemap tilemap; // Tilemap do rysowania planszy
    public TileBase tile; // Kafelki u¿ywane do rysowania
    public int radius = 2; // Promieñ planszy (2 dla Neuroshima Hex)

    private void Start()
    {
        GenerateHexGrid();
    }

    void GenerateHexGrid()
    {
        tilemap.ClearAllTiles(); // Czyœci poprzednie kafelki

        List<Vector3Int> hexCoordinates = GetHexagonalShape(radius);

        foreach (Vector3Int hexPos in hexCoordinates)
        {
            tilemap.SetTile(hexPos, tile);
        }
    }

    List<Vector3Int> GetHexagonalShape(int radius)
    {
        List<Vector3Int> hexPositions = new List<Vector3Int>();

        for (int q = -radius; q <= radius; q++)
        {
            int r1 = Mathf.Max(-radius, -q - radius);
            int r2 = Mathf.Min(radius, -q + radius);
            for (int r = r1; r <= r2; r++)
            {
                hexPositions.Add(new Vector3Int(q, r, 0));
            }
        }

        return hexPositions;
    }
}

