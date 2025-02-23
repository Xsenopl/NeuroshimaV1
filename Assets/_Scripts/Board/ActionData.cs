using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionData
{
    public TokenData token; // �eton, kt�ry zosta� umieszczony
    public Vector3Int position; // Pozycja na planszy
    public Image originalSlot; // Slot, z kt�rego pobrano �eton

    public ActionData(TokenData token, Vector3Int position, Image originalSlot)
    {
        if (token != null)
        {
            this.token = token;
            this.position = position;
            this.originalSlot = originalSlot;
        }
    }
}
