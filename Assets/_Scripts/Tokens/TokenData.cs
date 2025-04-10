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
    public List<Features> tokenFeatures;
    public List<ModuleEffect> moduleEffects;
}

public enum TokenType { Unit, Module, Headquarter, Action }
public enum AttackDirection { Up, UpRight, DownRight, Down, DownLeft, UpLeft }
public enum DirectionalAbility { Armor, Net, Sniper }
public enum TokenFeatures { Moving, Push, Battle, Sniper, Granade, Bomb}

// Struktura przechowuj�ca umiej�tno�ci dla danego kierunku
[System.Serializable]
public struct DirectionalFeatures
{
    public AttackDirection direction; // Kierunek ataku
    public List<AttackFeatures> attacks; // Lista efekt�w w danym kierunku
    public DirectionalAbility[] abilities; // Lista zdolno�ci kierunkowych
}

// Struktura przechowuj�ca informacje o ataku
[System.Serializable]
public struct AttackFeatures
{ 
    public int attackPower; // Si�a ataku (0 to brak ataku)
    public bool isRanged; // Czy jest to atak dystansowy?
}

// Struktura przechowuj�ca cechy jednostek
[System.Serializable]
public struct Features
{
    public TokenFeatures feature;
    public int quantity;
}

// Struktura przechowuj�ca efekty modu��w dla danego kierunku
[System.Serializable]
public struct ModuleEffect
{
    public ModuleEffectType effectType; // Typ efektu modu�u
    public int value;                   // Warto�� efektu (np. +1 do obra�e�, +1 inicjatywa)
    public List<AttackDirection> directions;   // Kierunki
    public bool enemyTarger;
}

public enum ModuleEffectType
{
    MeleeDamageBoost, RangedDamageBoost, HealthBoost, InitiativeBoost, InitiativeReduction, ExtraInitiative, GiveMovement, Medic, ChangeAttackToRange, CaptureTheModule
}
