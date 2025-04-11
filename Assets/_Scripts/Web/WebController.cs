using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public static class WebController
{
    public static readonly string SERVER_URL = "http://localhost:80/NeuroshimaHex";


    public static async void RegisterDuel(string army1, string army2)
    {
        await SendPostManager.RegisterDuel(army1, army2);
    }

    public static async void RegisterBoard(string boardJSON, bool isBeforeBattle = true)
    {
        string isBefore = isBeforeBattle.ToString();
        await SendPostManager.RegisterBoard(boardJSON, isBefore);
    }

    public static async void RegisterBattle(string battleEvents)
    {
        await SendPostManager.RegisterBattle(battleEvents);
    }

    



    //static async void Start()
    //{
    //    //StartCoroutine(GetRequest($"{SERVER_URL}/testowy2.php"));
    //    if (await SendPostManager.RegisterDuel("NeodŸungla", "Hegemonia"))
    //    {
    //        Debug.Log("Uda³o siê dodaæ duel");
    //    }
    //    else Debug.Log("Nie uda³o siê dodaæ duela");
    //}


    static IEnumerator GetRequest(string uri)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(uri);

        // Request and wait for the desired page.
        yield return webRequest.SendWebRequest();    
        
        string[] pages = uri.Split('/');
        int page = pages.Length - 1;
        switch (webRequest.result)
        {
            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.DataProcessingError:
                Debug.LogWarning(pages[page] + ": Error: " + webRequest.error);
                break;
            case UnityWebRequest.Result.ProtocolError:
                Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                break;
            case UnityWebRequest.Result.Success:
                Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                break;
        }
        
    }
}
