using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Unity.Burst.Intrinsics.Arm;

public class FocusedTile : MonoBehaviour
{
    //Wersja podœwietlenia jednego kafelka   
    [SerializeField] private Tile _highlightFocusedTile;
    private Tile _originalTile = null;
    private Tilemap _tilemap = null;
    private Vector3Int _previousCellPosition = new Vector3Int();

    Vector3Int[] directions = { //Góra, dó³, prawaG, lewaG, 
                new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0),
                new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0),
                new Vector3Int(1, -1, 0), new Vector3Int(-1, 1, 0)
            };

    // Start is called before the first frame update
    void Start()
    {
        if (_tilemap == null) { _tilemap = GetComponent<Tilemap>(); }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = _tilemap.WorldToCell(mouseWorldPos);
        cellPosition.z = 0;

        if(cellPosition != _previousCellPosition )
        {
            // Debug.Log(cellPosition);
            // Remove previous highlighted tile
            if ( _tilemap.HasTile(_previousCellPosition))
            {
                RemoveHighlightTile(_previousCellPosition);
                //RemoveHighlightTile(_previousCellPosition + directions[0]);
                //RemoveHighlightTile(_previousCellPosition + directions[1]);
            }

            // Add new highlighted tile
            if ( _tilemap.HasTile(cellPosition))
            {
                AddHighlightTile(cellPosition);
                //AddHighlightTile(cellPosition + directions[0]);
                //AddHighlightTile(cellPosition + directions[1]);
                //AddHighlightTile(cellPosition + directions[2]);
                //AddHighlightTile(cellPosition + directions[4]);
                //AddHighlightTile(cellPosition + directions[5]);
                
            }
            _previousCellPosition = cellPosition;
        }
    }

    private void AddHighlightTile(Vector3Int cellPosition)
    {
        _originalTile = _tilemap.GetTile<Tile>(cellPosition);
        if(_originalTile != null )
        {
            _highlightFocusedTile.sprite = _originalTile.sprite;
            _tilemap.SetTile(cellPosition, _highlightFocusedTile);
        }
    }

    private void RemoveHighlightTile(Vector3Int previouscellPosition)
    {
        TileBase currentTile = _tilemap.GetTile(previouscellPosition);
        if( currentTile == _highlightFocusedTile && _originalTile != null ) 
        { 
            _tilemap.SetTile(previouscellPosition, _originalTile);
        }
    }






}
/*  // Wersja z s¹siednimi kafelkami 
    public Tilemap tilemap;
    public TileBase highlightTile;
    private TileBase originalTile;
    private Vector3Int lastMouseGridPosition;
    private bool hasMouseGridPosition = false;

    void Update()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int gridPosition = tilemap.WorldToCell(worldPosition);
        gridPosition.z = 0;

        if (gridPosition != lastMouseGridPosition || !hasMouseGridPosition)
        {
            ClearHighlight();
            HighlightTiles(gridPosition);
            lastMouseGridPosition = gridPosition;
            hasMouseGridPosition = true;
        }
    }

    void ClearHighlight()
    {
        // Clear all highlighted tiles
        BoundsInt bounds = tilemap.cellBounds;
        for (int x = bounds.xMin; x <= bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y <= bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile(pos);
                if (tile == highlightTile)
                {
                    tilemap.SetTile(pos, originalTile); // Restore original tile
                }
            }
        }
    }

    void HighlightTiles(Vector3Int gridPosition)
    {
        TileBase tile = tilemap.GetTile(gridPosition);
        if (tile != null)
        {
            originalTile = tile;
            HighlightTile(gridPosition);

            int q = gridPosition.x - (gridPosition.y - (gridPosition.y & 1)) / 2;
            int r = gridPosition.y;

            // Directions in axial coordinates for flat-topped hexes
            Vector3Int[] directions = {
                new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0),
                new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0),
                new Vector3Int(1, -1, 0), new Vector3Int(-1, 1, 0)
            };

            foreach (var direction in directions)
            {
                Vector3Int neighborPosition = gridPosition + direction;
                TileBase neighborTile = tilemap.GetTile(neighborPosition);
                if (neighborTile != null)
                {
                    HighlightTile(neighborPosition);
                }
            }
        }
    }

    void HighlightTile(Vector3Int gridPosition)
    {
        tilemap.SetTile(gridPosition, highlightTile);
    }
*/

/* //Wersja z wszystkimi kafelkami po linii prostej
    private Tilemap _tilemap = null;
    public Tile _highlightFocusedTile;

    private Vector3Int lastMouseGridPosition;
    private bool hasMouseGridPosition = false;

    void Start()
    {
        if (_tilemap == null) { _tilemap = GetComponent<Tilemap>(); }
    }

    void Update()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = _tilemap.WorldToCell(worldPosition);
        cellPosition.z = 0;

        if (cellPosition != lastMouseGridPosition || !hasMouseGridPosition)
        {
            RemoveHighlightTile(lastMouseGridPosition);
            HighlightTiles(cellPosition);
            lastMouseGridPosition = cellPosition;
            hasMouseGridPosition = true;
        }
    }

    private Tile _originalTile = null;

    private void RemoveHighlightTile(Vector3Int previouscellPosition)
    {
        TileBase currentTile = _tilemap.GetTile(previouscellPosition);
        if (currentTile == _highlightFocusedTile && _originalTile != null)
        {
            _tilemap.SetTile(previouscellPosition, _originalTile);
        }
    }


    void HighlightTiles(Vector3Int cellPosition)
    {
        TileBase tile = _tilemap.GetTile(cellPosition);
        if (tile != null)
        {
            _originalTile = _tilemap.GetTile<Tile>(cellPosition);                                                   //dod
            AddHighlightTile(cellPosition);

            int s = cellPosition.x - (cellPosition.y - (cellPosition.y & 1)) / 2;
            int q = cellPosition.y;
            int r = -s - q;

            // Directions in cube coordinates for flat-topped hexes
            Vector3Int[] directions = {
                new Vector3Int(0, -1, 1), new Vector3Int(-1, 0, 1), new Vector3Int(-1, 1, 0), 
                new Vector3Int(0, 1, -1), new Vector3Int(1, 0, -1),new Vector3Int(1, -1, 0)                
            };

            foreach (var direction in directions)
            {
                HighlightLine(cellPosition, direction);
            }
        }
    }

    void HighlightTile(Vector3Int cellPosition)
    {
        //_tilemap.SetTile(cellPosition, _highlightFocusedTile);


    }

    void HighlightLine(Vector3Int start, Vector3Int direction)
    {
        Vector3Int current = start;
        while (true)
        {
            current += direction;
            TileBase tile = _tilemap.GetTile(current);
            if (tile == null)
            {
                break;
            }
            //_originalTile = _tilemap.GetTile<Tile>(current);
            AddHighlightTile(current);
        }
    }

    private void AddHighlightTile(Vector3Int cellPosition)
    {
        _originalTile = _tilemap.GetTile<Tile>(cellPosition);
        if (_originalTile != null)
        {
            _highlightFocusedTile.sprite = _originalTile.sprite;
            _tilemap.SetTile(cellPosition, _highlightFocusedTile);
        }
    }
*/


