using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject singlePanel;
    public GameObject multiPanel;
    public GameObject optionsPanel;
    public GameObject tutorialPanel;

    public string player1Army;
    public string player2Army;

    private Sprite player1ArmySprite;
    private Sprite player2ArmySprite;

    private void Start()
    {
        //player1Army = "Outpost";
        //player2Army = "Borgo";
    }

    public void ShowMainPanel() { mainPanel.SetActive(true); }
    public void HideMainPanel() { mainPanel.SetActive(false); }

    public void ShowSinglePanel() { singlePanel.SetActive(true); }
    public void HideSinglePanel() { singlePanel.SetActive(false); }

    public void ShowMultiPanel() { multiPanel.SetActive(true); }
    public void HideMultiPanel() { multiPanel.SetActive(false); }

    public void ShowOptionsPanel() { optionsPanel.SetActive(true); }
    public void HideOptionsPanel() { optionsPanel.SetActive(false); }

    public void ShowTutorialPanel() { tutorialPanel.SetActive(true); }
    public void HideTutorialPanel() { tutorialPanel.SetActive(false); }

    public void ExitGame() { Application.Quit(); }

    public void SetArmyImage(int player, Sprite armyImg, string armyName)
    {
        if (player != 1 && player != 2) { return; }

        if (player == 1)
        {
            //player1Army = armyName;
            player1ArmySprite = armyImg;
        }
        else
        {
            //player2Army = armyName;
            player2ArmySprite = armyImg;
        }
    }

    public void CreateNewDuel()
    {
        if (string.IsNullOrEmpty(player1Army) || string.IsNullOrEmpty(player2Army)) return;

        GameController.instance.selectedPlayer1Army = player1Army;
        GameController.instance.selectedPlayer2Army = player2Army;
        // Przypisanie dla ka¿dej armii
        GameController.instance.player1Database = Resources.Load<TokenDatabase>($"Armies/{player1Army}");
        GameController.instance.player2Database = Resources.Load<TokenDatabase>($"Armies/{player2Army}");

        if (GameController.instance.player1Database == null)
            { Debug.Log($"Armia {player1Army} nie istnieje"); return; }
        if (GameController.instance.player2Database == null)
            { Debug.Log($"Armia {player2Army} nie istnieje"); return; }

        GameController.instance.SetStatsPanelArmies(player1ArmySprite, player2ArmySprite, player1Army, player2Army);

        StartCoroutine(NewGameAsync());

    }

    private IEnumerator NewGameAsync()
    {
        SceneManager.LoadSceneAsync(1);
        while(SceneManager.GetActiveScene().buildIndex == 0)
        {
            yield return null;
        }
        GameController.instance.ShowGUI();
    }
}
