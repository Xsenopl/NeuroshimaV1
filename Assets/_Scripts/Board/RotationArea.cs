using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationArea : MonoBehaviour
{
    private Token _token;

    public void SetToken(Token assignedToken)
    {
        _token = assignedToken;
    }

    void OnMouseDown()
    {
        if (_token != null)
        {
            _token.StartRotation(Input.mousePosition);
            Debug.Log("Klikni�to pole do obracania.");
        }
    }

    void OnMouseUp()
    {
        if (_token != null)
        {
            _token.StopRotation();
            Debug.Log("Przestano obraca� �eton.");
        }
    }
}
