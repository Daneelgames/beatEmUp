using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomizeAudio : MonoBehaviour
{
    public AudioSource au;
    public bool playOnStart = true;
    public Vector2 pitchMinMax = new Vector2(0.7f, 1.1f);

    private void Start()
    {
        if (playOnStart)
        {
            au.pitch = Random.Range(pitchMinMax.x, pitchMinMax.y);
            au.Play();
        }
    }
}
