using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    private TokenSlotManager manager;
    private int playerID;
    public TokenData assignedToken;

    public void SetManager(TokenSlotManager _manager, int _playerID)
    {
        manager = _manager;
        playerID = _playerID;
        GetComponent<Button>().onClick.AddListener(OnSlotClick);
    }

    private void OnSlotClick()
    {
        if (assignedToken != null)
        {
            manager.SelectToken(assignedToken);
        }
    }
}
