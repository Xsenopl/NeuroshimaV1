using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationArea : MonoBehaviour
{
    private Token token;

    public void SetToken(Token assignedToken)
    {
        token = assignedToken;
    }

    void OnMouseDown()
    {
        if (token != null)
        {
            token.StartRotation(Input.mousePosition);
            Debug.Log("Klikniêto pole do obracania.");
        }
    }

    void OnMouseUp()
    {
        if (token != null)
        {
            token.StopRotation();
            Debug.Log("Przestano obracaæ ¿eton.");
        }
    }
}
