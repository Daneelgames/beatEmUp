using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class NavMeshBaker : MonoBehaviour
{
    public List<NavMeshSurface> navMeshSurfaces = new List<NavMeshSurface>();
    
    [ContextMenu("Bake")]
    public void BakeNavMeshes()
    {
        for (int i = 0; i < navMeshSurfaces.Count; i++)
        {
            navMeshSurfaces[i].BuildNavMesh();
        }
    }
}
