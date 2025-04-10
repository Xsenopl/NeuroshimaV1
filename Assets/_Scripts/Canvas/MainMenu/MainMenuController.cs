using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject singlePanel;
    public GameObject multiPanel;
    public GameObject optionsPanel;
    public GameObject tutorialPanel;

    public string player1Army = "Hegemonia";
    public string player2Army = "Moloch";

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

    public void CreateNewDuel()
    {
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
