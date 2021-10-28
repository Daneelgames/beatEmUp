using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldStreamer : MonoBehaviour
{
    [SerializeField] 
    private List<Transform> worldCells = new List<Transform>();

    IEnumerator Start()
    {
        yield break;
        while (true)
        {
            float distance = 1000;
            float newDistance = 0;
            for (int i = 0; i < worldCells.Count; i++)
            {
                newDistance = Vector3.Distance(worldCells[i].transform.position,
                    GameManager.Instance.mainCamera.transform.position);
                yield return null;   
            }
        }
    }
}