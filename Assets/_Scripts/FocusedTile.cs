using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FocusedTile : MonoBehaviour
{
    //Wersja podœwietlenia jednego kafelka   
    [SerializeField] private Tile _highlightFocusedTile;
    private Tile _originalTile = null;
    private Tilemap _tilemap = null;
    private Vector3Int _previousCellPosition = new Vector3Int();

    void Start()
    {
        if (_tilemap == null) { _tilemap = GetComponent<Tilemap>(); }
    }

    void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = _tilemap.WorldToCell(mouseWorldPos);
        cellPosition.z = 0;

        if(cellPosition != _previousCellPosition )
        {
            // Debug.Log(cellPosition);
            if ( _tilemap.HasTile(_previousCellPosition))
            {
                RemoveHighlightTile(_previousCellPosition);
            }

            // Add new highlighted tile
            if ( _tilemap.HasTile(cellPosition))
            {
                AddHighlightTile(cellPosition);  
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