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
        else
            Debug.Log("GameController istnieje");
        GameController.instance.ShowGUI();
    }
}
