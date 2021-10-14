using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    [SerializeField] private float cameraSmooth = 0.75f;
    
    private Transform parent;
    private IEnumerator Start()
    {
        while (PlayerInput.Instance == null)
        {
            yield return null;
        }

        // create parent object 
        parent = new GameObject().transform;
        parent.gameObject.name = "CameraParent";
        parent.transform.rotation = Quaternion.identity;
        parent.transform.localScale = Vector3.one;
        parent.transform.position = PlayerInput.Instance.transform.position;

        // set camera as child
        transform.parent = parent;
        
        while (true)
        {
            // parent smoothly follows it
            parent.gameObject.transform.position = Vector3.Lerp(parent.gameObject.transform.position, PlayerInput.Instance.transform.position, cameraSmooth * Time.smoothDeltaTime);
            yield return null;
        }
    }

}
