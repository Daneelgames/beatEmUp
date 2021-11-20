using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonLook : MonoBehaviour
{
    public static FirstPersonLook Instance;
    
    public float cameraTurnSpeed = 100;
    public Vector2 xMinMax = new Vector2(-45, 45);
    private Transform parentObject;
    private float camParentTargetX;
    private float camParentTargetY;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        parentObject = transform.parent; 
        transform.parent = null;
    }

    void Update()
    {
        if (PartyInputManager.Instance.InventoryMode)
            return;
        
        camParentTargetX -= Input.GetAxis("Mouse Y") * cameraTurnSpeed * Time.smoothDeltaTime;
        camParentTargetX = Mathf.Clamp (camParentTargetX, xMinMax.x, xMinMax.y);
        camParentTargetY += Input.GetAxis("Mouse X") * cameraTurnSpeed * Time.smoothDeltaTime;
    }
    
    private void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, parentObject.position, Time.smoothDeltaTime * 30);
        
        if (PartyInputManager.Instance.InventoryMode)
            return;
        
        transform.eulerAngles = new Vector3 (camParentTargetX, camParentTargetY, 0.0f);
    }
}