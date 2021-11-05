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
        Unit, Item
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
    
    public void Spawn(AssetReference assetReference, Vector3 newPos, Quaternion newRot, ObjectType objType, HealthController interactorHc, HealthController throwerHc, Vector3 throwTargetPos)
    {
        if (assetReference.RuntimeKeyIsValid() == false)
        {
            Debug.Log("Invalid Key " + assetReference.RuntimeKey);
        }
        if (asyncOperationHandles.ContainsKey(assetReference)) // if exists
        {
            if (asyncOperationHandles[assetReference].IsDone) // if exists and loaded
            {
                SpawnFromLoadedReference(assetReference, newPos, newRot, objType, interactorHc, throwerHc, throwTargetPos);
            }
            else // if exists and not loaded
                EnqueueSpawnForAfterInitialization(assetReference, newPos);
                
            return;
        }
        
        // if not exists
        LoadAndSpawn(assetReference, newPos, newRot, objType, interactorHc, throwerHc, throwTargetPos);
    }

    
    void EnqueueSpawnForAfterInitialization(AssetReference assetReference, Vector3 newPos)
    {
        if (queuedSpawnRequests.ContainsKey(assetReference) == false)
            queuedSpawnRequests[assetReference] = new Queue<Vector3>();
        queuedSpawnRequests[assetReference].Enqueue(newPos);
    }
    
    void LoadAndSpawn(AssetReference assetReference, Vector3 newPos, Quaternion newRot, ObjectType objectType, HealthController interactor, HealthController throwerHc, Vector3 throwTargetPos)
    {
        var op = Addressables.LoadAssetAsync<GameObject>(assetReference);
        asyncOperationHandles[assetReference] = op;
        op.Completed += (operation) =>
        {
            SpawnFromLoadedReference(assetReference, newPos, newRot, objectType, interactor, throwerHc, throwTargetPos);
            if (queuedSpawnRequests.ContainsKey(assetReference))
            {
                while (queuedSpawnRequests[assetReference]?.Any() == true)
                {
                    var position = queuedSpawnRequests[assetReference].Dequeue();
                    SpawnFromLoadedReference(assetReference, position, newRot, objectType, interactor, throwerHc, throwTargetPos);
                }
            }
        };
    }
    
    void SpawnFromLoadedReference(AssetReference assetReference, Vector3 newPos, Quaternion newRot,  ObjectType objectType, HealthController interactor, HealthController throwerHc, Vector3 throwTargetPos)
    {
        assetReference.InstantiateAsync(newPos, Quaternion.identity).Completed 
            += (asyncOperationHandle) =>
            {
                if (spawnedAssets.ContainsKey(assetReference) == false)
                {
                    spawnedAssets[assetReference] = new List<GameObject>();
                }
            
                spawnedAssets[assetReference].Add(asyncOperationHandle.Result);

                var spawnedGO = asyncOperationHandle.Result;
                if (objectType == ObjectType.Unit)
                {
                }
                else if (objectType == ObjectType.Item)
                {
                    ItemsDatabaseManager.Instance.ProceedNewItem(spawnedGO, interactor, throwerHc, throwTargetPos);
                }
            
                var notify = asyncOperationHandle.Result.AddComponent<NotifyOnDestroy>();
                notify.Destroyed += Remove;
                notify.AssetReference = assetReference;
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
