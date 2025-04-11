using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PopupMenuController : MonoBehaviour
{
    public GameObject popupMenu;
    public GameObject blocker;
    public GameObject statsWindow;

    public StatsManager statsManager;
    public TokenSlotManager tokenSlotManager;

    void Start()
    {
        if (statsManager == null)
        {
            statsManager = FindObjectOfType<StatsManager>();

            if (statsManager == null)
            {
                Debug.LogError("StatsManager nie zosta³ znaleziony na scenie!");
            }
        }

        // Ukrycie menu na start
        popupMenu.SetActive(false);
        blocker.SetActive(false);
        statsWindow.SetActive(false);
    }

    public void OpenMenu()
    {
        blocker.SetActive(true);
        popupMenu.SetActive(true);
    }

    public void CloseMenu()
    {
        blocker.SetActive(false);
        popupMenu.SetActive(false);
    }

    public void OpenStats()
    {
        popupMenu.SetActive(false);
        statsWindow.SetActive(true);

        statsManager.InitializePools(tokenSlotManager.GetPlayer1Pool(), tokenSlotManager.GetPlayer2Pool());
    }

    public void CloseStats()
    {
        statsWindow.SetActive(false);
        popupMenu.SetActive(true);
    }

    public void GoToMainMenu()
    {
        //tokenSlotManager.ClearSlots();
        tokenSlotManager.ResetAll();
        statsManager.ClearGraveyard();
        StartCoroutine(MainMenuAsync());
    }

    private IEnumerator MainMenuAsync()
    {
        SceneManager.LoadSceneAsync(0);
        while (SceneManager.GetActiveScene().buildIndex == 0)
        {
            yield return null;
        }
        GameController.instance.HideGUI();
    }
}

