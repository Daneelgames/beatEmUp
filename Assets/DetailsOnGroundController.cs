using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class DetailsOnGroundController : MonoBehaviour
{
    [SerializeField] private List<AssetReference> detailsReferences = new List<AssetReference>();

    [SerializeField] private List<GameObject> spawnedDetails = new List<GameObject>();
    [SerializeField] private int maxDetails = 100;
}