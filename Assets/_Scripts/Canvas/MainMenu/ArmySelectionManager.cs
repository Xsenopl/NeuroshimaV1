using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmySelectionManager : MonoBehaviour
{
    public MainMenuController mainMenuController;

    public Image defaultImage;
    public Image leftPlayerImage;
    public Text leftPlayerText;
    public Image rightPlayerImage;
    public Text rightPlayerText;

    private enum Player { None, Player1, Player2 }
    private Player activePlayer = Player.None;

    public void SelectPlayer1() => activePlayer = Player.Player1;
    public void SelectPlayer2() => activePlayer = Player.Player2;

    public void SelectArmy(Sprite armySprite, string armyName)
    {
        if (activePlayer == Player.None) return;

        bool isEmptyArmyName = string.IsNullOrEmpty(armyName);
        string otherArmy = activePlayer == Player.Player1 ? mainMenuController.player2Army : mainMenuController.player1Army;

        Debug.Log($"Ta armia: {armyName} i inna armia {otherArmy}");

        if (otherArmy == armyName && !isEmptyArmyName)
        {
            // Zamiana armii
            if (activePlayer == Player.Player1)
            {
                leftPlayerImage.sprite = armySprite;
                leftPlayerText.text = armyName;
                mainMenuController.player1Army = isEmptyArmyName ? "" : armyName;

                rightPlayerText.text = "Wybierz armiê";
                mainMenuController.player2Army = "";
                rightPlayerImage.sprite = defaultImage.sprite;
            }
            else
            {
                rightPlayerImage.sprite = armySprite;
                rightPlayerText.text = armyName;
                mainMenuController.player2Army = isEmptyArmyName ? "" : armyName;

                leftPlayerText.text = "Wybierz armiê";
                mainMenuController.player1Army = "";
                leftPlayerImage.sprite = defaultImage.sprite;
            }
            return;
        }

        // Standardowy wybór
        if (activePlayer == Player.Player1)
        {
            leftPlayerImage.sprite = armySprite;
            leftPlayerText.text = isEmptyArmyName ? "Wybierz Armiê" : armyName;
            mainMenuController.player1Army = isEmptyArmyName ? "" : armyName;
        }
        else
        {
            rightPlayerImage.sprite = armySprite;
            rightPlayerText.text = isEmptyArmyName ? "Wybierz Armiê" : armyName;
            mainMenuController.player2Army = isEmptyArmyName ? "" : armyName;
        }
    }
}
