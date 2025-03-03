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
    public List<DirectionalFeatures> directionFeatures; // Lista efekt�w ataku i umiej�tno�ci wed�ug kierunku
    public List<TokenFeatures> tokenFeatures;
    public List<ModuleEffect> moduleEffects;
}

public enum TokenType { Unit, Module, Headquarter, Action }
public enum AttackDirection { Up, UpRight, DownRight, Down, DownLeft, UpLeft }
public enum DirectionalAbility { Armor, Net, Sniper }
public enum TokenFeatures { Moving, Push }

// Struktura przechowuj�ca informacje o efektach w danym kierunku
[System.Serializable]
public struct AttackFeatures
{ 
    public int attackPower; // Si�a ataku (0 to brak ataku)
    public bool isRanged; // Czy jest to atak dystansowy?
}

// Struktura przechowuj�ca efekty dla danego kierunku
[System.Serializable]
public struct DirectionalFeatures
{
    public AttackDirection direction; // Kierunek ataku
    public List<AttackFeatures> attacks; // Lista efekt�w w danym kierunku
    public DirectionalAbility[] abilities; // Lista zdolno�ci kierunkowych
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
    MeleeDamageBoost, RangedDamageBoost, HealthBoost, InitiativeBoost, InitiativeReduction, ExtraInitiative, GiveMovement, Medic, ChangeAttackToRange, CaptureTheModule
}
