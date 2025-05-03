using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TokenTests
{
    private GameObject tokenObj;
    private Token token;

    [SetUp] //Przed ka¿dym
    public void Setup()
    {
        tokenObj = new GameObject("Token");
        token = tokenObj.AddComponent<Token>();
        token.tokenData = ScriptableObject.CreateInstance<TokenData>();

        //Dane z tokenData
        token.tokenData.tokenFeatures = new List<Features>
        {
            new Features { feature = TokenFeatures.Moving, quantity = 3 },
            new Features { feature = TokenFeatures.Sniper, quantity = 2 }
        };
    }

    [TearDown]  //Po ka¿dym
    public void Teardown()
    {
        Object.DestroyImmediate(tokenObj);
        Object.DestroyImmediate(token.tokenData);
    }

    [Test]
    public void InitializeCurrentFeatures_CopiesDataCorrectly()
    {
        token.InitializeCurrentFeatures();

        Assert.AreEqual(2, token.currentFeatures.Count);
        Assert.AreEqual(TokenFeatures.Moving, token.currentFeatures[0].feature);
        Assert.AreEqual(3, token.currentFeatures[0].quantity);
        Assert.AreEqual(TokenFeatures.Sniper, token.currentFeatures[1].feature);
        Assert.AreEqual(2, token.currentFeatures[1].quantity);
    }

    [Test]
    public void CanMove_ReturnsTrue_WhenMovingFeaturePresent()
    {
        token.InitializeCurrentFeatures();
        Assert.IsTrue(token.CanMove());
    }

    [Test]
    public void CanMove_ReturnsFalse_WhenNoMovingFeature()
    {
        //Brak Moving
        token.tokenData.tokenFeatures = new List<Features>
        {
            new Features { feature = TokenFeatures.Bomb, quantity = 1 }
        };

        token.InitializeCurrentFeatures();
        Assert.IsFalse(token.CanMove());
    }

    [Test]
    public void ResetMoves_RestoresOriginalQuantityFromTokenData()
    {
        token.InitializeCurrentFeatures();

        token.currentFeatures[0] = new Features
        {
            feature = TokenFeatures.Moving,
            quantity = 0
        };

        token.ResetMoves();

        Assert.AreEqual(3, token.currentFeatures[0].quantity);
    }

    [Test]
    [TestCase(AttackDirection.Up, 0f, AttackDirection.Up)]
    [TestCase(AttackDirection.Up, 60f, AttackDirection.UpLeft)]
    [TestCase(AttackDirection.Up, 120f, AttackDirection.DownLeft)]
    [TestCase(AttackDirection.Up, 180f, AttackDirection.Down)]
    [TestCase(AttackDirection.Up, 240f, AttackDirection.DownRight)]
    [TestCase(AttackDirection.Up, 300f, AttackDirection.UpRight)]
    [TestCase(AttackDirection.Up, 360f, AttackDirection.Up)]
    [TestCase(AttackDirection.Up, -60f, AttackDirection.UpRight)]
    public void GetRotatedDirection_ReturnsExpectedDirection(AttackDirection baseDir, float rotation, AttackDirection expected)
    {
        token.currentRotation = rotation;
        var result = token.GetRotatedDirection(baseDir);
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void InitializeNeighbors_CorrectlyDetectsOccupiedAndEmptyTiles()
    {
        token.hexCoords = new Vector2Int(2, 2); // even row

        var grid = new Dictionary<Vector2Int, Token>
        {
            { new Vector2Int(3, 2), null }, // occupied
            { new Vector2Int(1, 3), null }  // occupied
        };

        token.InitializeNeighbors(grid);

        Assert.AreEqual(6, token.neighborStatus.Count);
        Assert.IsTrue(token.neighborStatus[new Vector2Int(3, 2)]);
        Assert.IsTrue(token.neighborStatus[new Vector2Int(1, 3)]);
        Assert.IsFalse(token.neighborStatus[new Vector2Int(2, 1)]); // empty
    }

    [Test]
    public void CanMoveTo_ReturnsTrue_ForEmptyNeighbor_IfIsPlaced()
    {
        token.hexCoords = new Vector2Int(2, 2); // even row
        token.isPlaced = true;

        var grid = new Dictionary<Vector2Int, Token>
        {
            { new Vector2Int(1, 3), null } // only one occupied
        };

        token.InitializeNeighbors(grid);

        var target = new Vector2Int(3, 2); // empty neighbor
        Assert.IsTrue(token.CanMoveTo(target));
    }

    [Test]
    public void CanMoveTo_ReturnsFalse_ForOccupiedNeighbor()
    {
        token.hexCoords = new Vector2Int(2, 2);
        token.isPlaced = true;

        var grid = new Dictionary<Vector2Int, Token>
        {
            { new Vector2Int(3, 2), null }
        };

        token.InitializeNeighbors(grid);
        Assert.IsFalse(token.CanMoveTo(new Vector2Int(3, 2))); // occupied
    }

    [Test]
    public void CanMoveTo_ReturnsFalse_WhenNotPlaced()
    {
        token.hexCoords = new Vector2Int(2, 2);
        token.isPlaced = false;

        var grid = new Dictionary<Vector2Int, Token>();
        token.InitializeNeighbors(grid);

        Assert.IsFalse(token.CanMoveTo(new Vector2Int(3, 2)));
    }

}



    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    //[UnityTest]
    //public IEnumerator DirectionTestsWithEnumeratorPasses()
    //{
    //    // Use the Assert class to test conditions.
    //    // Use yield to skip a frame.
    //    yield return null;
    //}