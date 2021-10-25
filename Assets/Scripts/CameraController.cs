using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Vector2 xMinMax = new Vector2(15, 40);
    [SerializeField] private float distanceInFrontOfCharacter = 3;
    [SerializeField] private float cameraSmooth = 0.75f;
    [SerializeField] private float cameraMoveSpeed = 100;
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
        yield return new WaitForSeconds(1);
        canFollow = true;
    }


    private float mouseInputY = 0;
    void LateUpdate()
    {
        if (canFollow == false)
            return;
        
        // parent smoothly follows it
        Vector3 targetPosition = parent.gameObject.transform.position;
        if (PlayerInput.Instance)
            targetPosition = PlayerInput.Instance.transform.position + PlayerInput.Instance.transform.forward * distanceInFrontOfCharacter;
        else if (PartyInputManager.Instance)
        {
            float horizontalAxis = Input.GetAxis("Horizontal");
            float verticalAxis = Input.GetAxis("Vertical");
            Vector3 movementVector = parent.gameObject.transform.forward * verticalAxis + parent.gameObject.transform.right * horizontalAxis;
            targetPosition += movementVector.normalized * cameraMoveSpeed;
        }
        
        parent.gameObject.transform.position = Vector3.Lerp(parent.gameObject.transform.position, targetPosition, cameraSmooth * Time.deltaTime);
        parent.transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X"), 0) * (cameraTurnSpeed * Time.deltaTime));

        mouseInputY = -Input.GetAxis("Mouse Y");

        if (transform.localEulerAngles.x > xMinMax.y && mouseInputY > 0)
        {
            mouseInputY = 0;   
        }
        else if (transform.localEulerAngles.x < xMinMax.x && mouseInputY < 0)
            mouseInputY = 0;
        
        transform.Rotate(new Vector3(mouseInputY, 0, 0) * ((cameraTurnSpeed / 5) * Time.deltaTime));

        var resultEulerAngles = transform.localEulerAngles;
        transform.eulerAngles = new Vector3(Mathf.Clamp(resultEulerAngles.x,xMinMax.x,xMinMax.y), transform.eulerAngles.y, transform.eulerAngles.z);
    }
}
