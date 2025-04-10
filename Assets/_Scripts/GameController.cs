using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;
    public GameObject GUI;

    public BoardManager boardManager;

    private void Awake()
    {
        if (instance != null)
            return;
        instance = this;

        DontDestroyOnLoad(gameObject);
    }


    void Start()
    {
        
    }

    public void ShowGUI() { GUI.SetActive(true); }
    public void HideGUI() { GUI.SetActive(false); }


    void Update()
    {
        
    }

}
