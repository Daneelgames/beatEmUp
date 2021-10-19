using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUi : MonoBehaviour
{
    [SerializeField] private HealthController hc;

    [SerializeField] private Image healthbar;
    private void Update()
    {
        healthbar.fillAmount = (hc.Health * 1f) / (hc.HealthMax * 1f);
    }
}