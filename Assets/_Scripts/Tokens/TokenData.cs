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
    public List<DirectionalFeatures> directionFeatures; // Lista efektów ataku i umiejêtnoœci wed³ug kierunku
    public List<TokenFeatures> tokenFeatures;
    public List<ModuleEffect> moduleEffects;
}

public enum TokenType { Unit, Module, Headquarter, Action }
public enum AttackDirection { Up, UpRight, DownRight, Down, DownLeft, UpLeft }
public enum DirectionalAbility { Armor, Net, Sniper }
public enum TokenFeatures { Moving, Push }

// Struktura przechowuj¹ca informacje o efektach w danym kierunku
[System.Serializable]
public struct AttackFeatures
{ 
    public int attackPower; // Si³a ataku (0 to brak ataku)
    public bool isRanged; // Czy jest to atak dystansowy?
}

// Struktura przechowuj¹ca efekty dla danego kierunku
[System.Serializable]
public struct DirectionalFeatures
{
    public AttackDirection direction; // Kierunek ataku
    public List<AttackFeatures> attacks; // Lista efektów w danym kierunku
    public DirectionalAbility[] abilities; // Lista zdolnoœci kierunkowych
}

[System.Serializable]
public struct ModuleEffect
{
    public ModuleEffectType effectType; // Typ efektu modu³u
    public int value;                   // Wartoœæ efektu (np. +1 do obra¿eñ, +1 inicjatywa)
    public AttackDirection direction;   // Kierunek
}

public enum ModuleEffectType
{
    MeleeDamageBoost, RangedDamageBoost, HealthBoost, InitiativeBoost, InitiativeReduction, ExtraInitiative, GiveMovement, Medic, ChangeAttackToRange, CaptureTheModule
}
