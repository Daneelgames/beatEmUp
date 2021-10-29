using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPerksInfo", menuName = "ScriptableObjects/PerksInfo", order = 1)]
public class PerksInfoData : ScriptableObject
{
    public List<Perk> perks;
}