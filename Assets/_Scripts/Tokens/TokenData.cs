using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewToken", menuName = "NeuroshimaHex/Token")]
public class TokenData : ScriptableObject
{
    public string army;
    public Sprite sprite;
    public string tokenName;
    public TokenType tokenType;
    public int health;
    public List<int> initiatives;
    public List<DirectionalEffects> attackEffects; // Lista efekt�w ataku i umiej�tno�ci wed�ug kierunku
    public List<ModuleEffect> moduleEffects;
}

public enum TokenType { Unit, Module, Headquarter, Action }
public enum AttackDirection { Up, UpRight, DownRight, Down, DownLeft, UpLeft }
public enum SpecialAbility { Armor, Net, Sniper, Push, Moving }

// Struktura przechowuj�ca informacje o efektach w danym kierunku
[System.Serializable]
public struct TokenEffect
{
    public SpecialAbility[] abilities; // Lista zdolno�ci specjalnych
    public int attackPower; // Si�a ataku (0 to brak ataku)
    public bool isRanged; // Czy jest to atak dystansowy?
    //public int range; // Zasi�g ataku (1 dal wr�cz, X dla dystansowego)
}

// Struktura przechowuj�ca efekty dla danego kierunku
[System.Serializable]
public struct DirectionalEffects
{
    public AttackDirection direction; // Kierunek ataku
    public List<TokenEffect> effects; // Lista efekt�w w danym kierunku
}

[System.Serializable]
public struct ModuleEffect
{
    public ModuleEffectType effectType; // Typ efektu modu�u
    public int value;                   // Warto�� efektu (np. +1 do obra�e�, +1 inicjatywa)
    public AttackDirection direction;   // Kierunek
}

public enum ModuleEffectType
{
    MeleeDamageBoost, RangedDamageBoost, HealthBoost, InitiativeBoost, InitiativeReduction, ExtraInitiative
}
