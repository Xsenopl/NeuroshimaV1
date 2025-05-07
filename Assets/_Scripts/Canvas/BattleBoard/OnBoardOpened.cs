using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnBoardOpened : MonoBehaviour
{
    public GameController gameController;

    private void Awake()
    {
        if (GameController.instance == null)
            Instantiate(gameController);

        GameController.instance.AssignMainCameraToCanvas();
        GameController.instance.ShowGUI();
        GameController.instance.ShowTokenManager();
        GameController.instance.boardManager = FindAnyObjectByType<BoardManager>();

        GameController.instance.tokenManager.boardManager = GameController.instance.boardManager;

        GameController.instance.AssignArmies();
        GameController.instance.UpdateHqHP(null, null);

        //if (GameController.instance.boardManager != null)
        //{
        //    TokenSlotManager[] slots = GameController.instance.TokenManager.GetComponentsInChildren<TokenSlotManager>();    
        //    foreach (var slot in slots)
        //    {
        //        slot.boardManager = GameController.instance.boardManager;
        //    }
        //}
    }
}
