using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class BattleController : MonoBehaviour
{
    public Button battleButton;
    public BoardManager boardManager;

    private int battlePhase; // Aktualna faza bitwy

    private void Start()
    {
        battleButton.onClick.AddListener(StartBattle);
    }

    private void StartBattle()
    {
        Debug.Log("Rozpoczynam bitwê!");

        //ApplyModuleEffects();
        battlePhase = boardManager.GetHighestInitiative();
        MiddleBattle();

        boardManager.ChangeCurrentPlayer();
        Debug.Log("Koñczê bitwê!");
    }

    private void MiddleBattle()
    {
        while (battlePhase >= 0)
        {
            Debug.Log($"Faza bitwy: {battlePhase}");

            List<Token> tokensToRemove = new List<Token>();

            foreach (var tokenEntry in boardManager.tokenGrid)
            {
                Token token = tokenEntry.Value;
                if (token == null || !token.currentInitiatives.Contains(battlePhase)) continue;

                foreach (var attackEffect in token.currentAttackEffects)
                {
                    ProcessAttack(token, attackEffect, tokensToRemove);
                }
            }

            RemoveDestroyedTokens(tokensToRemove);
            battlePhase--;
        }
    }

    private void ProcessAttack(Token attacker, DirectionalFeatures attackEffect, List<Token> tokensToRemove)
    {
        AttackDirection rotatedDir = attacker.GetRotatedDirection(attackEffect.direction);
        bool isRangedAttack = attackEffect.attacks.Any(effect => effect.isRanged);

        Token targetToken = isRangedAttack
            ? GetRangedTarget(attacker, rotatedDir)
            : GetMeleeTarget(attacker, rotatedDir);

        if (targetToken != null && CanAttack(attacker, targetToken))
        {
            foreach (var effect in attackEffect.attacks)
            {
                targetToken.TakeDamage(effect.attackPower);
                Debug.Log($"{attacker.tokenData.tokenName} ({attacker.currentAttackEffects.Count} efektów) atakuje {targetToken.tokenData.tokenName} za {effect.attackPower} DMG! (Ranged: {effect.isRanged})");

                if (targetToken.currentHealth <= 0 && !tokensToRemove.Contains(targetToken))
                {
                    tokensToRemove.Add(targetToken);
                }
            }
        }
    }

    private Token GetMeleeTarget(Token attacker, AttackDirection direction)
    {
        Vector2Int attackDir = boardManager.GetHexDirection(attacker.hexCoords, direction);
        Vector2Int targetPos = attacker.hexCoords + attackDir;

        if (boardManager.tokenGrid.TryGetValue(targetPos, out Token targetToken) && CanAttack(attacker, targetToken))
        {
            return targetToken;
        }
        return null;
    }

    private Token GetRangedTarget(Token attacker, AttackDirection direction)
    {
        Vector2Int attackDir = boardManager.GetHexDirection(attacker.hexCoords, direction);
        Vector2Int targetPos = attacker.hexCoords + attackDir;

        while (boardManager.IsValidPosition(targetPos))
        {
            if (boardManager.tokenGrid.TryGetValue(targetPos, out Token targetToken))
            {
                if (CanAttack(attacker, targetToken))
                {
                    return targetToken; // Trafiony wróg
                }
            }
            attackDir = boardManager.GetHexDirection(targetPos, direction);
            targetPos += attackDir;
        }
        return null;
    }

    private bool CanAttack(Token attacker, Token target)
    {
        if (attacker.tokenData.tokenType == TokenType.Headquarter && target.tokenData.tokenType == TokenType.Headquarter)
        {
            return false;
        }
        return attacker.tokenData.army != target.tokenData.army;
    }

    private void RemoveDestroyedTokens(List<Token> tokensToRemove)
    {
        StatsManager statsManager = FindObjectOfType<StatsManager>();

        if (statsManager == null)
        {
            Debug.LogError("StatsManager nie zosta³ znaleziony w scenie!");
            return;
        }

        foreach (var deadToken in tokensToRemove)
        {
            if (deadToken == null || deadToken.tokenData == null)
            {
                Debug.LogWarning("¯eton lub jego dane s¹ null!");
                continue;
            }

            // Pobranie w³aœciciela ¿etonu na podstawie armii
            int ownerPlayer = boardManager.GetTokenOwner(deadToken.tokenData.army);

            if (ownerPlayer == 0)
            {
                Debug.LogError($"Nie mo¿na okreœliæ w³aœciciela dla ¿etonu {deadToken.tokenData.tokenName} ({deadToken.tokenData.army})!");
                continue;
            }

            Debug.Log($"¯eton {deadToken.tokenData.tokenName} ({deadToken.tokenData.army}) zosta³ zniszczony");

            // Dodanie ¿etonu do odpowiedniego cmentarza
            statsManager.AddToGraveyard(deadToken.tokenData, ownerPlayer);

            // Usuniêcie ¿etonu z planszy
            boardManager.RemoveToken(deadToken);
        }
    }


}

/*
    // ________________Prace nad modu³ami________________
    private void ApplyModuleEffects()
    {
        foreach (var tokenEntry in boardManager.tokenGrid)
        {
            Token unit = tokenEntry.Value;
            if (unit == null || unit.tokenData.tokenType == TokenType.Module) continue;

            List<ModuleEffect> attacks = boardManager.GetModuleEffectsForUnit(unit);
            Debug.Log($"Jednostka {unit.tokenData.tokenName} otrzyma³a {attacks.Count} efektów modu³ów.");

            foreach (var effect in attacks)
            {
                switch (effect.effectType)
                {
                    case ModuleEffectType.MeleeDamageBoost:
                        foreach (var attackEffect in unit.currentAttackEffects)
                        {
                            for (int i = 0; i < attackEffect.attacks.Count; i++)
                            {
                                if (!attackEffect.attacks[i].isRanged) // Tylko ataki wrêcz
                                {
                                    attackEffect.attacks[i] = new AttackFeatures
                                    {
                                        attackPower = attackEffect.attacks[i].attackPower + effect.value,
                                        isRanged = attackEffect.attacks[i].isRanged,
                                        abilities = (DirectionalAbility[])attackEffect.attacks[i].abilities.Clone()
                                    };
                                }
                            }
                        }
                        Debug.Log($"{unit.tokenData.tokenName} zyska³ {effect.value} do ataku wrêcz! Nowe wartoœci: " +
                            $"{string.Join(", ", unit.currentAttackEffects.SelectMany(e => e.attacks.Select(a => a.attackPower)))}");
                        break;


                    case ModuleEffectType.RangedDamageBoost:
                        foreach (var attackEffect in unit.currentAttackEffects)
                        {
                            for (int i = 0; i < attackEffect.attacks.Count; i++)
                            {
                                if (attackEffect.attacks[i].isRanged) // Tylko ataki dystansowe
                                {
                                    attackEffect.attacks[i] = new AttackFeatures
                                    {
                                        attackPower = attackEffect.attacks[i].attackPower + effect.value,
                                        isRanged = attackEffect.attacks[i].isRanged,
                                        abilities = (DirectionalAbility[])attackEffect.attacks[i].abilities.Clone()
                                    };
                                }
                            }
                        }
                        Debug.Log($"{unit.tokenData.tokenName} zyska³ {effect.value} do ataku dystansowego! Nowe wartoœci: " +
                            $"{string.Join(", ", unit.currentAttackEffects.SelectMany(e => e.attacks.Select(a => a.attackPower)))}");
                        break;

                    case ModuleEffectType.HealthBoost:
                        unit.currentHealth += effect.value;
                        break;

                    case ModuleEffectType.InitiativeBoost:
                        unit.currentInitiatives = unit.currentInitiatives.Select(i => i + effect.value).ToList();
                        break;

                    case ModuleEffectType.InitiativeReduction:
                        unit.currentInitiatives = unit.currentInitiatives.Select(i => i - effect.value).ToList();
                        break;

                    case ModuleEffectType.ExtraInitiative:
                        int minInitiative = unit.currentInitiatives.Min();
                        unit.currentInitiatives.Add(minInitiative - 1);
                        break;
                }
            }
        }
    }
*/