using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

// Flat-topped
public class BattleFieldHexGrid : MonoBehaviour
{
	[SerializeField] private Tilemap _tilemap;
	public TileBase[] _AllTiles { get; private set; }


    void Start()
	{
		AssignCubeCoordinatesToTiles();
    }

	void Update()
	{ }

	void AssignCubeCoordinatesToTiles()
	{
		BoundsInt bounds = _tilemap.cellBounds;
		_AllTiles = _tilemap.GetTilesBlock(bounds);

		int i = 0;
		for (int x = bounds.xMin; x <= bounds.xMax; x++)
		{
			for (int y = bounds.yMin; y <= bounds.yMax; y++)
			{
				TileBase tile = _tilemap.GetTile(new Vector3Int(x, y, 0));
				if (tile != null)
				{
					// Convert Tilemap coordinates (x, y) to axial coordinates (q, r)
					int s = x - (y - (y & 1)) / 2;
					int q = y;
					int r = -s - q;

					//Debug.Log($"Tile at ({x}, {y}) has cube coordinates (q: {q}, r: {r}, s: {s})");
					// Cube coordinates for further use.

					if (_AllTiles[i] == null)
					{
						//Debug.Log($"Nie ma wartoœci { i++}  ----  (q: {q}, r: {r}, s: {s})");

					}
					// else { Debug.Log($"Dla (q: {q}, r: {r}, s: {s})" + _AllTiles[i++].ToString()); }

                }
            }
		}
	}
}
