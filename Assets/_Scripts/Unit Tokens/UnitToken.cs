using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

public class UnitToken : Token
{
    private int _hp {  get; set; }
    private Dictionary<bool, string> _skillAvailable { get; set; }
    private Dictionary<int, int> _attackSidesDmgMele { get; set; }
    private Dictionary<int, int> _attackSidesDmgRange { get; set; }
    private Dictionary<int, string> _effectSides { get; set; }


    public override void use()
    {
        throw new System.NotImplementedException();
    }
    public override void changeParameters()
    {
        throw new System.NotImplementedException();
    }
}
