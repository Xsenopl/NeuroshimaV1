using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    private TokenSlotManager tokenSlotManager;
    public TokenData assignedToken;

    public void SetManager(TokenSlotManager _manager)
    {
        tokenSlotManager = _manager;
        GetComponent<Button>().onClick.AddListener(OnSlotClick);
    }

    // Przywrócenie slotu do stanu domyœlnego
    public void ClearSlot()
    {
        assignedToken = null;
        Image img = GetComponent<Image>();
        img.sprite = null;
        img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
        gameObject.name = "EmptySlot";
    }

    private void OnSlotClick()
    {
        if (assignedToken == null) return;

        tokenSlotManager.ShowTrashConfirmation(this);

        if (tokenSlotManager.HasThreeTokens())                     // Jeœli trzeba odrzuciæ ¿eton
        {       
            //Debug.Log($"Odrzucono ¿eton: {assignedToken.tokenName}");
            return;
        }
        else if(tokenSlotManager.HasTokensLeftToDiscard())             // Jeœli mo¿na odrzuciæ ¿eton
        { 
            //Debug.Log($"Wybrano ¿eton do zagrania: {assignedToken.tokenName}");
            tokenSlotManager.SelectToken(assignedToken);
        }
    }
}
