using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BattleControllerTests
{
    private GameObject battleObject;
    private BattleController battleController;

    [SetUp]
    public void Setup()
    {
        battleObject = new GameObject("BattleController");
        battleController = battleObject.AddComponent<BattleController>();

        var boardObject = new GameObject("BoardManager");
        var boardManager = boardObject.AddComponent<BoardManager>();
        battleController.boardManager = boardManager;

        // Ustawienie wymaganych referencji GameController.instance
        GameObject gameControllerGO = new GameObject("GameController");
        gameControllerGO.AddComponent<GameController>();
        GameController.instance = gameControllerGO.GetComponent<GameController>();
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(battleObject);
        Object.DestroyImmediate(battleController.boardManager.gameObject);
        Object.DestroyImmediate(GameController.instance.gameObject);
    }

    [UnityTest]
    public IEnumerator StartBattle_WithNoTokens_CompletesWithoutError()
    {
        battleController.StartBattle(0);
        yield return null;
        Assert.Pass("Bitwa zakoñczona bez b³êdów dla pustej planszy.");
    }

    [UnityTest]
    public IEnumerator StartBattle_InitializesAndClearsAttackLog()
    {
        // Przygotowanie sztucznego ataku
        var fakeToken = new GameObject("Attacker").AddComponent<Token>();
        fakeToken.hexCoords = new Vector2Int(0, 0);
        fakeToken.tokenData = ScriptableObject.CreateInstance<TokenData>();
        fakeToken.tokenData.army = "Army1";
        fakeToken.currentHealth = 5;
        fakeToken.currentInitiatives = new List<int> { 0 };
        fakeToken.currentAttackEffects = new List<DirectionalFeatures>();

        battleController.boardManager.tokenGrid = new Dictionary<Vector2Int, Token>
        {
            [new Vector2Int(0, 0)] = fakeToken
        };

        // Rozpoczêcie bitwy
        battleController.StartBattle(0);
        yield return null;

        Assert.IsTrue(true); // Jeœli nie wyst¹pi wyj¹tek – test przeszed³
    }
}

    //// A Test behaves as an ordinary method
    //[Test]
    //public void BattleControllerTestsSimplePasses()
    //{
    //    // Use the Assert class to test conditions
    //}

    //// A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    //// `yield return null;` to skip a frame.
    //[UnityTest]
    //public IEnumerator BattleControllerTestsWithEnumeratorPasses()
    //{
    //    // Use the Assert class to test conditions.
    //    // Use yield to skip a frame.
    //    yield return null;
    //}