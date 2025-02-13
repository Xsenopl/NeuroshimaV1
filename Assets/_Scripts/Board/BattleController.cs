using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
                //Vector2Int attackDir = boardManager.GetHexDirection(token.hexCoords, attackEffect.direction);
                AttackDirection rotatedDir = token.GetRotatedDirection(attackEffect.direction);
                Vector2Int attackDir = boardManager.GetHexDirection(token.hexCoords, rotatedDir);
                Vector2Int targetPos = token.hexCoords + attackDir;

                if (boardManager.tokenGrid.TryGetValue(targetPos, out Token targetToken))
                {
                    foreach (var effect in attackEffect.effects)
                    {
                        targetToken.TakeDamage(effect.attackPower);
                        Debug.Log($"{token.tokenData.tokenName} atakuje {targetToken.tokenData.tokenName} za {effect.attackPower} DMG!");

                        if (targetToken.currentHealth <= 0)
                        {
                            tokensToRemove.Add(targetToken);
                        }
                    }
                }
            }
        }

        // Usuniêcie ¿etonów o zerowym zdrowiu
        foreach (var deadToken in tokensToRemove)
        {
            Debug.Log($"¯eton {deadToken.tokenData.tokenName} zniszczony!");
            boardManager.RemoveToken(deadToken);
        }
        Debug.Log("Koñczê bitwê!");
    }
}
