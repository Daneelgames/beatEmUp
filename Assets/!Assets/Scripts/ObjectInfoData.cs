using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewObjectInfo", menuName = "ScriptableObjects/ObjectInfo", order = 1)]
public class ObjectInfoData : ScriptableObject
{
    public string objectName = "Joel";
    public string objectNameAttackedBySomeone = "Joel";
    public string objectGetsItem = "Joel gets";
    public string objectUsesItem = "Joel uses";
    
    // attack message: Joel shoot Frank for 25 damage;
    public string objectDamaged = "Joel is damaged";
}