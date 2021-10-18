using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    [SerializeField] private float distanceInFrontOfCharacter = 3;
    [SerializeField] private float cameraSmooth = 0.75f;
    [SerializeField] private float cameraTurnSpeed = 500f;
    
    private Transform parent;
    private Quaternion targetRotation;
    private bool canFollow = false;
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
        targetRotation = transform.rotation; 
        canFollow = true;
    }


    private float mouseInputY = 0;
    void LateUpdate()
    {
        if (canFollow == false)
            return;
        
        // parent smoothly follows it
        parent.gameObject.transform.position = Vector3.Lerp(parent.gameObject.transform.position, PlayerInput.Instance.transform.position + PlayerInput.Instance.transform.forward * distanceInFrontOfCharacter, cameraSmooth * Time.smoothDeltaTime);
        parent.transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X"), 0) * (cameraTurnSpeed * Time.deltaTime));

        mouseInputY = -Input.GetAxis("Mouse Y");

        if (transform.localEulerAngles.x > 40 && mouseInputY > 0)
            mouseInputY = 0;
        else if (transform.localEulerAngles.x < 15 && mouseInputY < 0)
            mouseInputY = 0;
        
        transform.localEulerAngles += new Vector3(mouseInputY, 0, 0) * ((cameraTurnSpeed / 5) * Time.deltaTime);
    }
}
