using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShufflePositionForObjects : MonoBehaviour
{
    public List<GameObject> objectsToShuffle = new List<GameObject>();
    public List<Transform> positionsList = new List<Transform>();
    void Awake()
    {
        Vector3 targetPosition = positionsList[Random.Range(0, positionsList.Count)].position;
        
        for (int i = 0; i < objectsToShuffle.Count; i++)
        {
            objectsToShuffle[i].transform.position = targetPosition;
            objectsToShuffle[i].SetActive(true);
        }
    }
}
