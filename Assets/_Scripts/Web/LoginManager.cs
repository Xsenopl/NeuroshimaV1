using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    [SerializeField] Text userName;

    [Header("Register")]
    public GameObject regPanel;
    [SerializeField] InputField regEmail;
    [SerializeField] InputField regUsername;
    [SerializeField] InputField regPassword;
    [SerializeField] Text regWarningText;

    [Header("Login")]
    public GameObject logPanel;
    [SerializeField] InputField logEmail;
    [SerializeField] InputField logPassword;
    [SerializeField] Text logWarningText;

    public void ShowRegisterPanel()
    {
        logWarningText.text = ""; logEmail.text = ""; logPassword.text = "";
        logPanel.SetActive(false);
        regPanel.SetActive(true);
    }
    public void ShowLoginPanel()
    {
        regWarningText.text = ""; regEmail.text = ""; regUsername.text = ""; regPassword.text = "";
        regPanel.SetActive(false);
        logPanel.SetActive(true);
    }

    public void TryToRegisterUser()
    {
        if(string.IsNullOrWhiteSpace(regEmail.text) || regEmail.text.Length < 5 || !regEmail.text.Contains("@")) { regWarningText.text = "Podaj w³aœciwy email"; return; }
        if(string.IsNullOrWhiteSpace(regUsername.text) || regUsername.text.Length < 5) { regWarningText.text = "Podaj w³aœciw¹ nazwê u¿ytkownika"; return; }
        if(string.IsNullOrWhiteSpace(regPassword.text) || regPassword.text.Length < 5) { regWarningText.text = "Podaj w³aœciwe has³o"; return; }

        WebController.RegisterUser(regEmail.text, regUsername.text, regPassword.text);
        //userName.text = $"Pomyœlnie zarejestrowano u¿ytkownika {regUsername.text}";
    }

    public async void TryToLoginUser()
    {
        if (string.IsNullOrWhiteSpace(logEmail.text) || logEmail.text.Length < 5 || !logEmail.text.Contains("@")) { logWarningText.text = "Podaj w³aœciwy email"; return; }
        if (string.IsNullOrWhiteSpace(logPassword.text) || logPassword.text.Length < 5) { logWarningText.text = "Podaj w³aœciwe has³o"; return; }

        string username = await WebController.LoginUser(logEmail.text, logPassword.text);
        if (username == null) 
            { logWarningText.text = "Nie ma takiego u¿ytkownika"; return; }
        userName.text = $"Witaj {username}";
        if(GameController.instance != null) 
            GameController.instance.SetLogedUser(username);
    }



}
