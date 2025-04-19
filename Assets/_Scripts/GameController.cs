using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;
    public GameObject GUI;
    public GameObject TokenManager;

    public BoardManager boardManager;
    public TokenSlotManager tokenManager;

    public string selectedPlayer1Army;
    public string selectedPlayer2Army;

    public TokenDatabase player1Database;
    public TokenDatabase player2Database;

    private void Awake()
    {
        if (instance != null)
            return;
        instance = this;

        DontDestroyOnLoad(gameObject);
    }


    void Start()
    {
        tokenManager = GetComponentInChildren<TokenSlotManager>(true);
        tokenManager.player1Database = player1Database;
    }

    public void ShowGUI() { GUI.SetActive(true); }
    public void HideGUI() { GUI.SetActive(false); }

    public void AssignMainCameraToCanvas()
    {
        if (GUI.TryGetComponent<Canvas>(out var canvas))
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
        }
        else
        {
            Debug.LogWarning("Brak komponentu Canvas GUI!");
        }
    }

    public void ShowTokenManager() { TokenManager.SetActive(true); }
    public void HideTokenManager() { TokenManager.SetActive(false); }

    public void AssignArmies()
    {
        tokenManager.UpdatePanelInteractivity();
        tokenManager.InitializePools(player1Database, player2Database);
    }


    void Update()
    {
        
    }

}
