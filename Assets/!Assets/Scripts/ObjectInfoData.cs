using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewObjectInfo", menuName = "ScriptableObjects/ObjectInfo", order = 1)]
public class ObjectInfoData : ScriptableObject
{
    [Header("ПРОБЕЛЫ И ТОЧКИ НА КОНЦЕ ФРАЗЫ В КОДЕ")]
    [Header("Generated Text")]
    public string objectName = "";
    public string objectSpecialDescription = "";

    public string sexMale = "Male";
    public string sexFemale = "Female";
    public string sexUnknown = "Unknown";
    
    [Header("Variations")]
    public string objectDies = "";
    public string objectAttacks = "";
    public string objectKills = "";
    public string objectGetsItem = "";
    public string objectIsDamaged = "";
    public string weaponLaysInHisHand = "lays in his hand";
    public string healthHigh = "looks alright";
    public string healthMid = "looks weakened";
    public string healthLow = "near death";
}