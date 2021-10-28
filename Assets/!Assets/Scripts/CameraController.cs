using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;
    [SerializeField] private Transform parent;
    [SerializeField] private Vector2 xMinMax = new Vector2(15, 40);
    [SerializeField] private Vector2 zMinMax = new Vector2(-150, -10);
    [SerializeField] private float distanceInFrontOfCharacter = 3;
    [SerializeField] private float cameraSmooth = 0.75f;
    [SerializeField] private float cameraMoveSpeed = 100;
    [SerializeField] private float cameraTurnSpeed = 500f;
    [SerializeField] private float cameraZoomSpeed = 500f;
    
    private Quaternion targetRotation;
    private bool canFollow = false;
    
    private float camParentTargetX = 0;
    private float camParentTargetY = 0;
    private float camTargetZ = 0;
    private Vector3 targetPosition;

    void Awake()
    {
        Instance = this;
    }
    
    private IEnumerator Start()
    {
        while (PlayerInput.Instance == null && PartyInputManager.Instance == null)
        {
            yield return null;
        }

        camParentTargetX = parent.transform.localPosition.x;
        camParentTargetY = parent.transform.localPosition.y;
        camTargetZ = transform.localPosition.z;
        yield return new WaitForSeconds(1);
        canFollow = true;
    }

    void Update()
    {
        // input & calculations
        
        targetPosition = parent.gameObject.transform.position;
        
        if (PlayerInput.Instance)
            targetPosition = PlayerInput.Instance.transform.position + PlayerInput.Instance.transform.forward * distanceInFrontOfCharacter;
        else if (PartyInputManager.Instance)
        {
            float horizontalAxis = Input.GetAxisRaw("Horizontal");
            float verticalAxis = Input.GetAxisRaw("Vertical");
            
            var forward = GameManager.Instance.mainCamera.transform.forward;
            var right = GameManager.Instance.mainCamera.transform.right;
 
            //project forward and right vectors on the horizontal plane (y = 0)
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();    
            right.Normalize();
            
            Vector3 movementVector = forward * verticalAxis + right * horizontalAxis;
            targetPosition += movementVector.normalized * (cameraMoveSpeed * Time.smoothDeltaTime);
                
            camTargetZ += Input.GetAxis("Mouse ScrollWheel") * cameraZoomSpeed * Time.smoothDeltaTime;
            camTargetZ = Mathf.Clamp(camTargetZ, zMinMax.x, zMinMax.y);
            
            if (Input.GetButton("Aim"))
            {
                camParentTargetX -= Input.GetAxis("Mouse Y") * cameraTurnSpeed * Time.smoothDeltaTime;
                camParentTargetX = Mathf.Clamp (camParentTargetX, xMinMax.x, xMinMax.y);
                camParentTargetY += Input.GetAxis("Mouse X") * cameraTurnSpeed * Time.smoothDeltaTime;
            }
        }
    }

    void LateUpdate()
    {
        if (canFollow == false)
            return;
        
        parent.gameObject.transform.position = Vector3.Lerp(parent.gameObject.transform.position, targetPosition, cameraSmooth * Time.smoothDeltaTime);
        parent.transform.eulerAngles = new Vector3 (camParentTargetX, camParentTargetY, 0.0f);
        // zoom
        transform.localPosition = new Vector3(0, 0, camTargetZ);
    }

    public void MoveCameraToPosition(Vector3 newPos)
    {
        if (Vector3.Distance(parent.transform.position, newPos) < 20)
            return;
        
        parent.gameObject.transform.position = newPos;
    }
}
