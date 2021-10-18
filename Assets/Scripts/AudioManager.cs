using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource stepsAu;
    public AudioSource attackAu;
    public AudioSource damagedAu;
    public List<AudioClip> stepsClips;
    public List<AudioClip> attackClips;
    public List<AudioClip> damagedClips;
    
    public void PlaySteps(bool reduceVolume)
    {
        if (stepsAu == null)
            return;
        
        stepsAu.clip = stepsClips[Random.Range(0, stepsClips.Count)];
        if (reduceVolume)
            stepsAu.volume = 0.3f;
        else
            stepsAu.volume = 1f;
        stepsAu.pitch = Random.Range(0.6f, 1.1f);
        stepsAu.Play();
    }
    public void PlayAttack()
    {
        attackAu.clip = attackClips[Random.Range(0, attackClips.Count)];
        attackAu.pitch = Random.Range(0.6f, 1.1f);
        attackAu.Play();
    }
    public void PlayDamaged()
    {
        damagedAu.clip = damagedClips[Random.Range(0, damagedClips.Count)];
        damagedAu.pitch = Random.Range(0.6f, 1.1f);
        damagedAu.Play();
    }
}
