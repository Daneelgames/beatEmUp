using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorEvents : MonoBehaviour
{
    [SerializeField] private AudioManager _audioManager;
    [SerializeField] private NoiseMaker noiseMaker;
    
    public void PlayStep()
    {
        if (_audioManager)
            _audioManager.PlaySteps(true);
    }
    public void PlayRunStep()
    {
        if (_audioManager)
            _audioManager.PlaySteps(false);
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

    public void WalkStepNoise()
    {
        if (noiseMaker)
            noiseMaker.WalkStepNoise();
    }
    public void RunStepNoise()
    {
        if (noiseMaker)
            noiseMaker.RunStepNoise();
    }

    public void AttackNoise()
    {
        if (noiseMaker)
            noiseMaker.AttackNoise();
    }
    public void ShotNoise()
    {
        if (noiseMaker)
            noiseMaker.ShotNoise();
    }
}
