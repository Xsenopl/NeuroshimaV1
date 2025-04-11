using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TokenPoprzedni : MonoBehaviour
{
    private int _id { get; set; }
    private string _name { get; set; }
    private string _description { get; set; }

    public abstract void use();
    public abstract void changeParameters();
}
