using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;
    [SerializeField] private bool canMove = false;
    [SerializeField] private Transform parent;
    [SerializeField] private Vector2 xMinMax = new Vector2(15, 40);
    [SerializeField] private Vector2 zMinMax = new Vector2(-150, -10);
    [SerializeField] private float distanceInFrontOfCharacter = 3;
    [SerializeField] private float cameraParentHeight = 4;
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
        camParentTargetX = Mathf.Clamp (camParentTargetX, xMinMax.x, xMinMax.y);
        while (PlayerInput.Instance == null && PartyInputManager.Instance == null)
        {
            yield return null;
        }

        camParentTargetX = parent.transform.eulerAngles.x;
        camParentTargetY = parent.transform.eulerAngles.y;
        camTargetZ = transform.localPosition.z;
        yield return new WaitForSeconds(1);
        camParentTargetX = Mathf.Clamp (camParentTargetX, xMinMax.x, xMinMax.y);
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
            if (canMove)
            {
                float horizontalAxis = Input.GetAxisRaw("Horizontal");
                float verticalAxis = Input.GetAxisRaw("Vertical");

                if (Input.GetButton("MoveCamera"))
                {
                    horizontalAxis = Input.GetAxis("Mouse X") * 5;
                    verticalAxis = Input.GetAxis("Mouse Y") * 5;
                }
            
                var forward = GameManager.Instance.mainCamera.transform.forward;
                var right = GameManager.Instance.mainCamera.transform.right;
 
                //project forward and right vectors on the horizontal plane (y = 0)
                forward.y = 0f;
                right.y = 0f;
                forward.Normalize();
                right.Normalize();
            
                Vector3 movementVector = forward * verticalAxis + right * horizontalAxis;
                targetPosition += movementVector.normalized * (cameraMoveSpeed * Time.smoothDeltaTime);
            }
            
            camTargetZ += Input.GetAxis("Mouse ScrollWheel") * cameraZoomSpeed * Time.smoothDeltaTime;
            camTargetZ = Mathf.Clamp(camTargetZ, zMinMax.x, zMinMax.y);
            
            if (Input.GetButton("RotateCamera"))
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

    public void MoveCameraToPosition(Vector3 newPos, Transform _parent)
    {
        if(!canMove)
        {
            parent.gameObject.transform.parent = _parent;
            if (_parent != null)
                parent.gameObject.transform.localPosition = Vector3.up * cameraParentHeight;
        }
        else
        {
            if (Vector3.Distance(parent.gameObject.transform.position, newPos) > 50)
                parent.gameObject.transform.position = newPos;
        }
    }
}
