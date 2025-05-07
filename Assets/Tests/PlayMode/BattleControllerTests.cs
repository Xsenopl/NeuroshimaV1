using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class BattleControllerTests      // Klasa potrzebuje specjalnie stworzonej sceny pod testy
{
    private BattleController battleController;
    private BoardManager boardManager;

    [UnitySetUp]
    public IEnumerator LoadScene()
    {
        SceneManager.LoadScene("TestBoardScene");
        yield return null;

        yield return new WaitForSeconds(0.1f);

        // Znalezienie obiektów w scenie
        battleController = Object.FindObjectOfType<BattleController>();
        boardManager = Object.FindObjectOfType<BoardManager>();
    }

    [UnityTest]
    public IEnumerator StartBattle_CleanBoardRun()
    {
        Assert.IsNotNull(battleController, "BattleController nie zosta³ znaleziony na scenie.");
        Assert.IsNotNull(boardManager, "BoardManager nie zosta³ znaleziony na scenie.");

        // Symulacja bitwy – np. najpierw highestInitiative = 2
        battleController.StartBattle(2);

        yield return null; // Odczekanie jednej klatki

        Assert.Pass("Bitwa uruchomiona poprawnie na TestBoardScene.");
    }

    [UnityTest]
    public IEnumerator DestroyedTokens_AreRemovedAfterBattle()
    {
        // Pozycje na planszy
        Vector2Int targetPos = new Vector2Int(1, 0);
        Vector2Int attackerPos = new Vector2Int(2, 0);

        // ¯eton atakuj¹cy
        TokenData attackerData = ScriptableObject.CreateInstance<TokenData>();
        attackerData.tokenName = "Attacker";
        attackerData.army = "Army1";
        attackerData.health = 5;
        attackerData.initiatives = new List<int> { 0 };
        attackerData.tokenFeatures = new List<Features>();
        attackerData.directionFeatures = new List<DirectionalFeatures>
        {
            new DirectionalFeatures
            {
                direction = AttackDirection.Down,
                attacks = new List<AttackFeatures>
                {
                    new AttackFeatures { attackPower = 1, isRanged = false }
                }
            }
        };

        // ¯eton atakowany
        TokenData target = ScriptableObject.CreateInstance<TokenData>();
        target.tokenName = "Target";
        target.army = "Army2";
        target.health = 1;
        target.initiatives = new List<int>();
        target.tokenFeatures = new List<Features>();

        // Dodaje ¿etony na planszê
        boardManager.PlaceToken((Vector3Int)targetPos, target);
        boardManager.PlaceToken((Vector3Int)attackerPos, attackerData);

        yield return new WaitForSeconds(0.1f); // poczekaj na inicjalizacjê

        Assert.IsTrue(boardManager.tokenGrid.ContainsKey(targetPos), "Target nie istnieje.");
        Assert.IsTrue(boardManager.tokenGrid.ContainsKey(attackerPos), "Attacker nie istnieje.");

        // Bitwa
        battleController.StartBattle(0);
        yield return null;

        // Sprawdza, czy ¿eton zosta³ usuniêty
        bool existsAfterBattle = boardManager.tokenGrid.ContainsKey(targetPos);
        Assert.IsFalse(existsAfterBattle, "Zabity ¿eton nie zosta³ usuniêty po bitwie.");
    }

}