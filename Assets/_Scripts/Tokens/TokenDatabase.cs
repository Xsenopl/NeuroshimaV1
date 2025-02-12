using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TokenDatabase", menuName = "NeuroshimaHex/TokenDatabase")]
public class TokenDatabase : ScriptableObject
{
    public TokenData[] allTokens;
}

