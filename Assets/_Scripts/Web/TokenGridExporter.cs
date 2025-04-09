using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ExportedTokenGrid
{
    public string tokenName;
    public string army;
    public short currentRotation;
    public int x;
    public int y;
}

[System.Serializable]
public struct ExportedAttackLog
{
    public int battlePhase;
    public int attackerX;
    public int attackerY;
    public int targetX;
    public int targetY;

    public ExportedAttackLog(int phase, int ax, int ay, int tx, int ty)
    {
        this.battlePhase = phase;
        this.attackerX = ax;
        this.attackerY = ay;
        this.targetX = tx;
        this.targetY = ty;
    }
}

[System.Serializable]
public class ListWrapper<T>
{
    public List<T> items;
    public ListWrapper(List<T> items)
    {
        this.items = items;
    }
}

public static class TokenGridExporter
{
    private static List<ExportedTokenGrid> ExportTokenGrid(Dictionary<Vector2Int, Token> tokenGrid)
    {
        List<ExportedTokenGrid> exportList = new();

        foreach (var kvp in tokenGrid)
        {
            Vector2Int coords = kvp.Key;
            Token token = kvp.Value;

            ExportedTokenGrid data = new()
            {
                tokenName = token.tokenData.tokenName,
                army = token.tokenData.army,
                currentRotation = (short)token.transform.rotation.eulerAngles.z,
                x = coords.x,
                y = coords.y
            };

            exportList.Add(data);
        }

        return exportList;
    }

    public static string ExportAttackListAsJson(List<(int, int, int, int, int)> attackLog)
    {
        List<ExportedAttackLog> entries = new();

        foreach (var (phase, ax, ay, tx, ty) in attackLog)
        {
            entries.Add(new ExportedAttackLog(phase, ax, ay, tx, ty));
        }

        return JsonUtility.ToJson(new ListWrapper<ExportedAttackLog>(entries));
    }

    public static string ExportTokenGridAsJson(Dictionary<Vector2Int, Token> tokenGrid)
    {
        var exportList = ExportTokenGrid(tokenGrid);
        var wrapper = new ListWrapper<ExportedTokenGrid>(exportList);
        return JsonUtility.ToJson(wrapper);
    }
}
