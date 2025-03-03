using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionData
{
    public TokenData token; // ¯eton, który zosta³ umieszczony
    public Vector3Int? position; // Pozycja na planszy
    public Vector3Int? previousPosition; // Poprzednia pozycja na planszy
    public Image originalSlot; // Slot, z którego pobrano ¿eton

    // Konstruktor dla pobrania ¿etonu ze slotu
    public ActionData(TokenData token, Vector3Int? position, Image originalSlot)
    {
        if (token != null)
        {
            this.token = token;
            this.position = position;
            this.previousPosition = null;
            this.originalSlot = originalSlot;
        }
    }
    // Konstruktor dla ruchu ¿etonu
    public ActionData(TokenData token, Vector3Int position, Vector3Int previousPosition)
    {
        this.token = token;
        this.position = position;
        this.previousPosition = previousPosition;
        this.originalSlot = null;
    }

}
