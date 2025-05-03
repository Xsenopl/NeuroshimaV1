using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class BattleController : MonoBehaviour
{
    public BoardManager boardManager;

    private int battlePhase; // Aktualna faza bitwy
    private List<(int phase, int ax, int ay, int tx, int ty)> attackList = new List<(int, int, int, int, int)>();

    private void Start()
    {    }

    public void StartBattle(int highestInitiative)
    {
        Debug.Log("Rozpoczynam bitwê!");

        battlePhase = highestInitiative;
        MiddleBattle();

        if (GameController.instance != null)
            GameController.instance.EndTurn();

        string json = TokenGridExporter.ExportAttackListAsJson(attackList);
        WebController.RegisterBattle(json);
        attackList.Clear();
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
                attackList.Add((battlePhase, attacker.hexCoords.x, attacker.hexCoords.y, targetToken.hexCoords.x, targetToken.hexCoords.y));

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