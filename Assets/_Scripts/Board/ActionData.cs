using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionData
{
    public TokenData tokenData; // ¯eton, który zosta³ umieszczony/odrzucony
    public Vector3Int? position; // Pozycja na planszy
    public Vector3Int? previousPosition; // Poprzednia pozycja na planszy
    public float? previousRotation;
    public int previousHealth;
    public List<Features> previousFeatures;
    public Image originalSlot; // Slot, z którego pobrano ¿eton

    // Konstruktor dla pobrania ¿etonu ze slotu
    public ActionData(TokenData tokenData, Vector3Int? position, Image originalSlot)
    {
        if (tokenData != null)
        {
            this.tokenData = tokenData;
            this.position = position;
            this.previousPosition = null;
            this.previousRotation = null;
            this.originalSlot = originalSlot;
        }
    }

    // Konstruktor dla ruchu ¿etonu
    public ActionData(Token token, Vector3Int previousPosition, float previousRotation)
    {
        this.tokenData = token.tokenData;
        this.previousPosition = previousPosition;
        this.previousRotation = previousRotation;
        this.previousHealth = token.currentHealth;
        this.previousFeatures = new List<Features>(token.currentFeatures);
        this.position = (Vector3Int?)token.hexCoords;
        this.originalSlot = null;
    }

}
