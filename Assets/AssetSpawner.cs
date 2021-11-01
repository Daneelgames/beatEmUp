using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AssetSpawner : MonoBehaviour
{
    public enum ObjectType
    {
        Unit, Consumable, Weapon
    }
    
    public static AssetSpawner Instance;
    
    private readonly Dictionary<AssetReference, List<GameObject>> spawnedAssets = 
        new Dictionary<AssetReference, List<GameObject>>();

    private readonly Dictionary<AssetReference, Queue<Vector3>> queuedSpawnRequests = 
        new Dictionary<AssetReference, Queue<Vector3>>(); 
    private readonly Dictionary<AssetReference, AsyncOperationHandle<GameObject>> asyncOperationHandles = 
        new Dictionary<AssetReference, AsyncOperationHandle<GameObject>>();

    public void Awake()
    {
        Instance = this;
    }
    
    public void Spawn(AssetReference assetReference, Vector3 newPos, Quaternion newRot, ObjectType objType)
    {
        if (assetReference.RuntimeKeyIsValid() == false)
        {
            Debug.Log("Invalid Key " + assetReference.RuntimeKey);
        }
        if (asyncOperationHandles.ContainsKey(assetReference)) // if exists
        {
            if (asyncOperationHandles[assetReference].IsDone) // if exists and loaded
            {
                SpawnFromLoadedReference(assetReference, newPos, newRot, objType);
            }
            else // if exists and not loaded
                EnqueueSpawnForAfterInitialization(assetReference, newPos, newRot);
                
            return;
        }
        
        // if not exists
        LoadAndSpawn(assetReference, newPos, newRot, objType);
    }

    void SpawnFromLoadedReference(AssetReference assetReference, Vector3 newPos, Quaternion newRot,  ObjectType objectType)
    {
        assetReference.InstantiateAsync(newPos, Quaternion.identity).Completed 
            += (asyncOperationHandle) =>
        {
            if (spawnedAssets.ContainsKey(assetReference) == false)
            {
                spawnedAssets[assetReference] = new List<GameObject>();
            }
            
            spawnedAssets[assetReference].Add(asyncOperationHandle.Result);

            if (objectType == ObjectType.Unit)
            {
            }
            else if (objectType == ObjectType.Consumable)
            {
            }
            else if (objectType == ObjectType.Weapon)
            {
            }
            
            var notify = asyncOperationHandle.Result.AddComponent<NotifyOnDestroy>();
            notify.Destroyed += Remove;
            notify.AssetReference = assetReference;
        };
    }
    
    void EnqueueSpawnForAfterInitialization(AssetReference assetReference, Vector3 newPos, Quaternion newRot )
    {
        if (queuedSpawnRequests.ContainsKey(assetReference) == false)
            queuedSpawnRequests[assetReference] = new Queue<Vector3>();
        queuedSpawnRequests[assetReference].Enqueue(newPos);
    }
    
    void LoadAndSpawn(AssetReference assetReference, Vector3 newPos, Quaternion newRot, ObjectType objectType)
    {
        var op = Addressables.LoadAssetAsync<GameObject>(assetReference);
        asyncOperationHandles[assetReference] = op;
        op.Completed += (operation) =>
        {
            SpawnFromLoadedReference(assetReference, newPos, newRot, objectType);
            if (queuedSpawnRequests.ContainsKey(assetReference))
            {
                while (queuedSpawnRequests[assetReference]?.Any() == true)
                {
                    var position = queuedSpawnRequests[assetReference].Dequeue();
                    SpawnFromLoadedReference(assetReference, position, newRot, objectType);
                }
            }
        };
    }
    
    void Remove(AssetReference assetReference, NotifyOnDestroy obj)
    {
        Addressables.ReleaseInstance(obj.gameObject);

        spawnedAssets[assetReference].Remove(obj.gameObject);
        if (spawnedAssets[assetReference].Count == 0)
        {
            // Debug.Log($"Removed all{assetReference.RuntimeKey.ToString()}");
            
            if (asyncOperationHandles.Count > 0 && asyncOperationHandles.ContainsKey(assetReference) && asyncOperationHandles[assetReference].IsValid())
                Addressables.Release(asyncOperationHandles[assetReference]);

            asyncOperationHandles.Remove(assetReference);
        }
    }
}
