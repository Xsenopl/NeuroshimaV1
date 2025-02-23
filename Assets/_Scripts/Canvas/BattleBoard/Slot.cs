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

    // Przywr�cenie slotu do stanu domy�lnego
    public void ClearSlot()
    {
        this.assignedToken = null;
        this.GetComponent<Image>().sprite = null;
        this.gameObject.name = "EmptySlot";
    }

    private void OnSlotClick()
    {
        if (assignedToken == null) return;

        manager.ShowDiscardConfirmation(this);

        if (manager.HasThreeTokens())                     // Je�li trzeba odrzuci� �eton
        {
            //manager.DiscardToken(assignedToken, this);          
            Debug.Log($"Odrzucono �eton: {assignedToken.tokenName}");

            return;
        }
        else if(manager.HasTokensLeftToDiscard())             // Je�li mo�na odrzuci� �eton
        { 
            Debug.Log($"Wybrano �eton do zagrania: {assignedToken.tokenName}");
            manager.SelectToken(assignedToken);
        }
    }
}
