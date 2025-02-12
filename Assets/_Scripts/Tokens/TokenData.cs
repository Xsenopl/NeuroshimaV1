using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewToken", menuName = "NeuroshimaHex/Token")]
public class TokenData : ScriptableObject
{
    public Sprite sprite;
    public string tokenName;
    public TokenType tokenType;
    public int health;
    public int initiative;
    public AttackDirection[] attackDirections;
    public SpecialAbility[] abilities;
}

public enum TokenType { Unit, Module, Headquarter, Action }
public enum AttackDirection { Up, Down, Left, Right, UpLeft, UpRight, DownLeft, DownRight }
public enum SpecialAbility { Armor, Net, Sniper, Push, Moving }

