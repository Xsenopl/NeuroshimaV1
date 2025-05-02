using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class SendPostManager
{
    public static readonly string SERVER_URL = "http://localhost:80/NeuroshimaHex";

    static async Task<(bool success, string returnMessage)> SendPostRequest(string url, Dictionary<string, string> data)
    {
        using (UnityWebRequest req = UnityWebRequest.Post(url, data))
        {
            req.SendWebRequest();

            while (!req.isDone) await Task.Delay(100);

            //Task is done
            if (req.error != null || !string.IsNullOrWhiteSpace(req.error) || HasErrorMessage(req.downloadHandler.text)) 
            { 
                return (false, req.downloadHandler.text);
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
            return (true, req.downloadHandler.text);
        }
    }

    public static async Task<bool> RegisterUser(string email, string username, string password)
    {
        string REG_REGISTER_USER_URL = $"{SERVER_URL}/register_user.php";

        return (await SendPostRequest(REG_REGISTER_USER_URL, new Dictionary<string, string>()
        {
            {"email", email },
            {"username", username },
            {"password", password },
        })).success;
    }
    public static async Task<(bool success, string username)> LoginUser(string email, string password)
    {
        string REG_LOGIN_USER_URL = $"{SERVER_URL}/login_user.php";

        return await SendPostRequest(REG_LOGIN_USER_URL, new Dictionary<string, string>()
        {
            {"email", email },
            {"password", password },
        });
    }

    public static async Task<bool> RegisterDuel(string army1, string army2)
    {
        string REG_DUEL_URL = $"{SERVER_URL}/register_duel.php";

        return (await SendPostRequest(REG_DUEL_URL, new Dictionary<string, string>()
        {
            {"player1_army", army1 },
            {"player2_army", army2 },
        })).success;
    }

    public static async Task<bool> RegisterBoard(string boardJSON, string isBeforeBattle)
    {
        string REG_BOARD_URL = $"{SERVER_URL}/register_board.php";

        return (await SendPostRequest(REG_BOARD_URL, new Dictionary<string, string>()
        {
            {"board", boardJSON },
            {"isBeforeBattle", isBeforeBattle}
        })).success;
    }

    public static async Task<bool> RegisterBattle(string battleEvents)
    {
        string REG_BOARD_URL = $"{SERVER_URL}/register_battle.php";

        return (await SendPostRequest(REG_BOARD_URL, new Dictionary<string, string>()
        {
            {"battle", battleEvents}
        })).success;
    }

    public static async Task<bool> SetDuelScore(string score1, string score2)
    {
        string REG_DUEL_URL = $"{SERVER_URL}/update_duel.php";

        return (await SendPostRequest(REG_DUEL_URL, new Dictionary<string, string>()
        {
            {"player1_score", score1 },
            {"player2_score", score2 },
        })).success;
    }

    static bool HasErrorMessage(string msg) => int.TryParse(msg, out var error);

}
