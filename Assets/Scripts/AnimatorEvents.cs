using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorEvents : MonoBehaviour
{
    [SerializeField] private AudioManager _audioManager;
    
    public void PlayStep()
    {
        if (_audioManager)
            _audioManager.PlaySteps();
    }
    public void PlayAttack()
    {
        if (_audioManager)
            _audioManager.PlayAttack();
    }
    public void PlayDamaged()
    {
        if (_audioManager)
            _audioManager.PlayDamaged();
    }
}
