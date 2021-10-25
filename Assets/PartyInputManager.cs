using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyInputManager : MonoBehaviour
{
    public static PartyInputManager Instance;
    
    public List<HealthController> selectedAllyUnits = new List<HealthController>();

    private void Awake()
    {
        Instance = this;
    }
}