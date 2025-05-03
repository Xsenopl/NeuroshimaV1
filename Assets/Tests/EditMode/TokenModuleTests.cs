using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class TokenModuleTests
{
    private Token source;
    private Token target;

    [SetUp]
    public void Setup()
    {
        source = CreateToken("Source");
        target = CreateToken("Target");
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(source.gameObject);
        Object.DestroyImmediate(target.gameObject);
    }

    private Token CreateToken(string name, List<int> initiatives = null, int health = 5)
    {
        GameObject go = new GameObject(name);
        Token token = go.AddComponent<Token>();
        TokenData data = ScriptableObject.CreateInstance<TokenData>();
        data.tokenName = name;
        data.health = health;
        data.initiatives = initiatives ?? new List<int> { 2 };
        token.tokenData = data;
        token.currentInitiatives = new List<int>(data.initiatives);
        token.currentHealth = data.health;
        token.currentFeatures = new List<Features>();
        return token;
    }

    [Test]
    public void ApplyEffectToTarget_AddsHealthBoost()
    {
        var effect = new ModuleEffect { effectType = ModuleEffectType.HealthBoost, value = 3 };

        source.ApplyEffectToTarget(target, effect);
        Assert.AreEqual(8, target.currentHealth);
    }

    [Test]
    public void ApplyEffectToTarget_AddsMovementIfMissing()
    {
        var effect = new ModuleEffect { effectType = ModuleEffectType.GiveMovement, value = 2 };

        source.ApplyEffectToTarget(target, effect);
        Assert.IsTrue(target.currentFeatures.Exists(f => f.feature == TokenFeatures.Moving));
    }

    [Test]
    public void RemoveEffectFromTarget_RemovesMovement()
    {
        var effect = new ModuleEffect { effectType = ModuleEffectType.GiveMovement, value = 2 };
        source.ApplyEffectToTarget(target, effect);

        source.RemoveEffectFromTarget(target, effect);
        var movingFeature = target.currentFeatures.Find(f => f.feature == TokenFeatures.Moving);

        if (movingFeature.feature == TokenFeatures.Moving)
            Assert.AreEqual(0, movingFeature.quantity);
        else
            Assert.Pass("Cecha Moving zosta³a ca³kowicie usuniêta");
    }

    [Test]
    public void IncreaseHealth_AddsCorrectAmount()
    {
        target.IncreaseHealth(4);
        Assert.AreEqual(9, target.currentHealth);
    }

    [Test]
    public void IncreaseInitiative_AddsCorrectly()
    {
        target.currentInitiatives = new List<int> { 3, 0 };
        target.IncreaseInitiative(1);
        CollectionAssert.AreEqual(new List<int> { 4, 1 }, target.currentInitiatives);
    }

    [Test]
    public void DecreaseInitiative_NeverBelowZero()
    {
        target.currentInitiatives = new List<int> { 1, 0 };
        target.DecreaseInitiative(2);
        CollectionAssert.AreEqual(new List<int> { 0, 0 }, target.currentInitiatives);
    }

    [Test]
    public void GainExtraInitiative_AddsOneBelowLowest()
    {
        target.currentInitiatives = new List<int> { 4, 2 };
        target.GainExtraInitiative(1);
        CollectionAssert.Contains(target.currentInitiatives, 1);
    }

    [Test]
    public void RemoveLowestInitiative_OnlyRemovesOneIfMultipleZeros()
    {
        target.currentInitiatives = new List<int> { 2, 0, 0 };
        target.RemoveLowestInitiative();
        CollectionAssert.AreEqual(new List<int> { 2, 0 }, target.currentInitiatives);
    }

    [Test]
    public void GainExtraMovement_AddsMovementIfNotPresent()
    {
        target.GainExtraMovement(1);
        Assert.AreEqual(TokenFeatures.Moving, target.currentFeatures[0].feature);
        Assert.AreEqual(1, target.currentFeatures[0].quantity);
    }

    [Test]
    public void RemoveExtraMovement_RemovesFeatureIfNotInTokenData()
    {
        target.GainExtraMovement(2);
        target.RemoveExtraMovement(2);
        Assert.IsEmpty(target.currentFeatures);
    }

    [Test]
    public void RemoveExtraMovement_DecreasesIfInTokenData()
    {
        target.tokenData.tokenFeatures = new List<Features> { new Features { feature = TokenFeatures.Moving, quantity = 1 } };
        target.currentFeatures = new List<Features> { new Features { feature = TokenFeatures.Moving, quantity = 3 } };

        target.RemoveExtraMovement(1);
        Assert.AreEqual(2, target.currentFeatures[0].quantity);
    }
}
