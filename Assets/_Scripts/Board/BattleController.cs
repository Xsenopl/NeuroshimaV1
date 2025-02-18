using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class BattleController : MonoBehaviour
{
    public Button battleButton;
    public BoardManager boardManager;

    private void Start()
    {
        battleButton.onClick.AddListener(StartBattle);
    }

    private void StartBattle()
    {
        Debug.Log("Rozpoczynam bitwê!");

        List<Token> tokensToRemove = new List<Token>();

        foreach (var tokenEntry in boardManager.tokenGrid)
        {
            Token token = tokenEntry.Value;
            if (token == null) continue;

            foreach (var attackEffect in token.tokenData.attackEffects)
            {
                ProcessAttack(token, attackEffect, tokensToRemove);
            }
        }

        RemoveDestroyedTokens(tokensToRemove);

        Debug.Log("Koñczê bitwê!");
    }

    private void ProcessAttack0(Token attacker, DirectionalEffects attackEffect, List<Token> tokensToRemove)
    {
        AttackDirection rotatedDir = attacker.GetRotatedDirection(attackEffect.direction);
        Vector2Int attackDir = boardManager.GetHexDirection(attacker.hexCoords, rotatedDir);
        Vector2Int targetPos = attacker.hexCoords + attackDir;

        Token targetToken = attackEffect.effects.Any(effect => effect.isRanged)
            ? GetRangedTarget0(targetPos, rotatedDir)
            : GetMeleeTarget0(targetPos);

        if (targetToken != null)
        {
            foreach (var effect in attackEffect.effects)
            {
                targetToken.TakeDamage(effect.attackPower);
                Debug.Log($"{attacker.tokenData.tokenName} atakuje {targetToken.tokenData.tokenName} za {effect.attackPower} DMG!");

                if (targetToken.currentHealth <= 0 && !tokensToRemove.Contains(targetToken))
                {
                    tokensToRemove.Add(targetToken);
                }
            }
        }
    }

    private Token GetMeleeTarget0(Vector2Int targetPos)
    {
        boardManager.tokenGrid.TryGetValue(targetPos, out Token targetToken);
        return targetToken;
    }

    private Token GetRangedTarget0(Vector2Int targetPos, AttackDirection direction)
    {

        while (boardManager.IsValidPosition(targetPos))
        {
            if (boardManager.tokenGrid.TryGetValue(targetPos, out Token targetToken))
            {
                //Debug.Log($"Znaleziono cel: {targetToken.tokenData.tokenName} na pozycji {targetPos}");
                return targetToken;
            }

            Vector2Int attackDir = boardManager.GetHexDirection(targetPos, direction);
            targetPos += attackDir;
        }

        return null;
    }

    private void ProcessAttack(Token attacker, DirectionalEffects attackEffect, List<Token> tokensToRemove)
    {
        AttackDirection rotatedDir = attacker.GetRotatedDirection(attackEffect.direction);
        bool isRangedAttack = attackEffect.effects.Any(effect => effect.isRanged);

        Token targetToken = isRangedAttack
            ? GetRangedTarget(attacker, rotatedDir)
            : GetMeleeTarget(attacker, rotatedDir);

        if (targetToken != null && CanAttack(attacker, targetToken))
        {
            foreach (var effect in attackEffect.effects)
            {
                targetToken.TakeDamage(effect.attackPower);
                Debug.Log($"{attacker.tokenData.tokenName} atakuje {targetToken.tokenData.tokenName} za {effect.attackPower} DMG!");

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
        foreach (var deadToken in tokensToRemove)
        {
            Debug.Log($"¯eton {deadToken.tokenData.tokenName} zniszczony!");
            boardManager.RemoveToken(deadToken);
        }
    }
}
