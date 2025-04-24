using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController instance;
    public GameObject GUI;
    public GameObject TokenManager;

    public BoardManager boardManager;
    public TokenSlotManager tokenManager;

    public Text player1TilesCount;
    public Text player2TilesCount;

    public string selectedPlayer1Army;
    public string selectedPlayer2Army;

    public TokenDatabase player1Database;
    public TokenDatabase player2Database;

    [SerializeField]
    private string logedUser;
    private int currentPlayer;

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

    public void SetLogedUser(string user)
    {
        logedUser = user;
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
        tokenManager.UpdatePanelInteractivity(1);
        tokenManager.InitializePools(player1Database, player2Database);
    }

    public void EndTurn()
    {
        if (tokenManager.HasThreeTokens()) { Debug.Log("Musisz najpierw odrzuciæ jakiœ ¿eton"); return; }
        if (boardManager == null || tokenManager == null) return;

        (int player1HP, int player2HP) = boardManager.GetHQHealth();
        if (IsEndGame(player1HP, player2HP)) 
        {
            EndGame();
            return; 
        }

        currentPlayer = boardManager.ChangeCurrentPlayer();
        tokenManager.NextTurn();
        
        tokenManager.UpdatePanelInteractivity(currentPlayer);
        tokenManager.DrawTokensMediator(currentPlayer);

        UpdateTilesCount();
    }

    public void UpdateTilesCount()
    {
        if (tokenManager == null || player1TilesCount == null || player2TilesCount == null)
        {
            Debug.LogWarning("Brak TokenSlotManagera lub puli ¿etonów");
            return;
        }
        if (currentPlayer < 1 || currentPlayer > 2)
        {
            Debug.LogWarning("Dotyczy nieistniej¹cego gracza");
            return;
        }

        int count = currentPlayer == 1 ? tokenManager.GetPlayer1Pool().Count : tokenManager.GetPlayer2Pool().Count;

        _ = currentPlayer == 1 ? player1TilesCount.text = count.ToString() : player2TilesCount.text = count.ToString();
    }

    public void UndoLastAction()
    {
        if (boardManager == null) return;
        boardManager.UndoLastAction();
    }



    private bool IsEndGame(int hp1, int hp2)
    {
        Debug.Log($"{hp1}, {hp2}, tura {tokenManager.GetTurnCounter()}");
        if ((hp1 <= 0 || hp2 <= 0) && tokenManager.GetTurnCounter() > 2) return true;
        if (tokenManager.GetLastDraw()) return true;
        return false;
    }

    public void EndGame()
    {
        (int player1HP, int player2HP) = boardManager.GetHQHealth();
        GUI.GetComponent<PopupMenuController>().ShowEndGamePanel($"{selectedPlayer1Army} : {selectedPlayer2Army}\n{player1HP} : {player2HP}");
        WebController.SetDuelScore(player1HP, player2HP);
        Debug.LogWarning("EndGame");
    }
}
