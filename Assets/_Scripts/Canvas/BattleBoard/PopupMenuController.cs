using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PopupMenuController : MonoBehaviour
{
    public GameObject popupMenu;
    public GameObject blocker;
    public GameObject statsWindow;

    public StatsManager statsManager;
    public TokenSlotManager tokenSlotManager;

    private int baseFontSize = 80;
    private int basePopupMenuPaddingWidth = 560;
    private Vector2 referenceResolution = new(1920, 1080);
    

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

        UpdatePadding(popupMenu, referenceResolution, basePopupMenuPaddingWidth);
        UpdateFontSizeInChildren(popupMenu, referenceResolution, baseFontSize);

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

    void UpdatePadding(GameObject verticalLayoutGroup, Vector2 baseResolution, int basePadding)
    {
        VerticalLayoutGroup popupMenuLayout = verticalLayoutGroup.GetComponent<VerticalLayoutGroup>();
        float scaleFactor = Screen.width / baseResolution.x;

        int newPadding = Mathf.RoundToInt(basePadding * scaleFactor);

        //popupMenuLayout.padding.top = newPadding;
        //popupMenuLayout.padding.bottom = newPadding;
        popupMenuLayout.padding.left = newPadding;
        popupMenuLayout.padding.right = newPadding;

        LayoutRebuilder.ForceRebuildLayoutImmediate(popupMenuLayout.GetComponent<RectTransform>());
    }

    public void UpdateFontSizeInChildren(GameObject parent, Vector2 baseResolution, int baseFontSize)
    {
        Text[] texts = parent.GetComponentsInChildren<Text>(true);
        float scaleFactor = Screen.width / baseResolution.x;

        int newFont = Mathf.RoundToInt(baseFontSize * scaleFactor);

        foreach (Text txt in texts)
        {
            txt.fontSize = newFont;
        }
    }
}

