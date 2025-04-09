using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public static class SendPostManager
{
    public static readonly string SERVER_URL = "http://localhost:80/NeuroshimaHex";

    static async Task<bool> SendPostRequest(string url, Dictionary<string, string> data)
    {
        using (UnityWebRequest req = UnityWebRequest.Post(url, data))
        {
            req.SendWebRequest();

            while (!req.isDone) await Task.Delay(100);

            //Task is done
            if (req.error != null || !string.IsNullOrWhiteSpace(req.error) || HasErrorMessage(req.downloadHandler.text)) 
            { 
                return false;
            }

            switch (req.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogWarning(": Error: " + req.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(": HTTP Error: " + req.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(": Skutek SendPostRequesta: " + req.downloadHandler.text);
                    break;
            }

            //On Success
            return true;
        }
    }

    public static async Task<bool> RegisterDuel(string army1, string army2)
    {
        string REG_DUEL_URL = $"{SERVER_URL}/register_duel.php";

        return await SendPostRequest(REG_DUEL_URL, new Dictionary<string, string>()
        {
            {"gracz1_armia", army1 },
            {"gracz2_armia", army2 },
        });
    }

    public static async Task<bool> RegisterBoard(string boardJSON, string isBeforeBattle)
    {
        string REG_BOARD_URL = $"{SERVER_URL}/register_board.php";

        return await SendPostRequest(REG_BOARD_URL, new Dictionary<string, string>()
        {
            {"board", boardJSON },
            {"isBeforeBattle", isBeforeBattle}
        });
    }

    public static async Task<bool> RegisterBattle(string battleEvents)
    {
        string REG_BOARD_URL = $"{SERVER_URL}/register_battle.php";

        return await SendPostRequest(REG_BOARD_URL, new Dictionary<string, string>()
        {
            {"battle", battleEvents}
        });
    }

    static bool HasErrorMessage(string msg) => int.TryParse(msg, out var error);

}
