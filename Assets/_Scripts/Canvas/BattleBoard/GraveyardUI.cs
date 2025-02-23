using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraveyardUI : MonoBehaviour
{
    /*
    public GameObject graveyardPanel;
    public Transform player1Container;
    public Transform player2Container;
    public Button showGraveyardButton;

    private BoardManager boardManager;

    private void Awake()
    {
        if (graveyardPanel == null) Debug.LogError("graveyardPanel nie jest przypisany w Inspectorze!");
        if (player1Container == null)
        {
            Debug.LogWarning("player1Container NIE przypisany w Inspectorze, próbujê znaleŸæ...");
            player1Container = GameObject.Find("Player1Container")?.transform;
        }

        if (player2Container == null)
        {
            Debug.LogWarning("player2Container NIE przypisany w Inspectorze, próbujê znaleŸæ...");
            player2Container = GameObject.Find("Player2Container")?.transform;
        }
    }

    private void Start()
    {
        Debug.Log("GraveyardUI dzia³a poprawnie!");

        boardManager = FindObjectOfType<BoardManager>();
        showGraveyardButton.onClick.AddListener(ToggleGraveyardPanel);
        graveyardPanel.SetActive(false);
    }

    private void ToggleGraveyardPanel()
    {
        Debug.Log("Klikniêto przycisk Poka¿ cmentarz");

        graveyardPanel.SetActive(!graveyardPanel.activeSelf);
        Debug.Log("Aktywny? " + graveyardPanel.activeSelf);
        if (graveyardPanel.activeSelf)
        {
            Debug.Log("Aktualizowanie UI cmentarza...");
            UpdateGraveyardUI();
        }
    }

    private void UpdateGraveyardUI()
    {
        foreach (Transform child in player1Container) Destroy(child.gameObject);
        foreach (Transform child in player2Container) Destroy(child.gameObject);

        foreach (var item in boardManager.graveyardPlayer1)
        {
            CreateGraveyardEntry(item.Key, item.Value, player1Container);
        }

        foreach (var item in boardManager.graveyardPlayer2)
        {
            CreateGraveyardEntry(item.Key, item.Value, player2Container);
        }
    }

    private void CreateGraveyardEntry(string tokenName, int count, Transform container)
    {
        GameObject entry = new GameObject(tokenName);
        entry.transform.SetParent(container);

        Text text = entry.AddComponent<Text>();
        text.text = $"{tokenName}: {count}";
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        Button button = entry.AddComponent<Button>();
        button.onClick.AddListener(() => Debug.Log($"Klikniêto ¿eton na cmentarzu: {tokenName}"));
    }
    */
}
