using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewObjectInfo", menuName = "ScriptableObjects/ObjectInfo", order = 1)]
public class ObjectInfoData : ScriptableObject
{
    public enum Sex
    {
        Male, Female, Unknown 
    }

    public Sex sex = Sex.Unknown;
    [Header("Generated Text")]
    public string objectName = "";
    public string objectDies = "";
    public string objectAttacks = "";
    public string objectKills = "";
    public string objectGetsItem = "";
    public string objectIsDamaged = "";
}