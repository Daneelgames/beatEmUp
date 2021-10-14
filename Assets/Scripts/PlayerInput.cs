using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [Header("Movement Stats")]
    [SerializeField] private float movementSpeed = 5;
    [SerializeField] private float runningSpeedBonus = 5;
    [SerializeField] private float turningSpeed = 5;
    [SerializeField] private float gravity = 5;
    [SerializeField] private float jumpPower = 5;
    [SerializeField] private LayerMask groundMask;

    [Header("Links")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator anim;
    [SerializeField] private AttackManager attackManager;

    private Quaternion lookRotation;

    private Vector3 movementVector;
    private Vector3 verticalVector;
    private float horizontalAxis;
    private float verticalAxis;

    private bool moving = false;
    private bool running = false;
    private bool grounded = false;
    
    private static readonly int Moving = Animator.StringToHash("Moving");
    private static readonly int Running = Animator.StringToHash("Running");
    private static readonly int Jumping = Animator.StringToHash("Jumping");
    
    private Collider[] groundColliders = new Collider[1];

    private float characterRadius = 0.1f;

    public static PlayerInput Instance;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        characterRadius = characterController.radius;
        StartCoroutine(ControlMovementAnimations());
    }

    IEnumerator ControlMovementAnimations()
    {
        #region movement Animations
        while (true)
        {
            //yield return new WaitForSeconds(0.1f);
            yield return null;
            if (moving && anim.GetBool(Moving) == false)
            {
                anim.SetBool(Moving, true);
            }
            else if (!moving && anim.GetBool(Moving))
            {
                anim.SetBool(Moving, false);
            }
            
            if (running && anim.GetBool(Running) == false)
            {
                anim.SetBool(Running, true);
            }
            else if (!running && anim.GetBool(Running))
            {
                anim.SetBool(Running, false);
            }
            
            if (!grounded && anim.GetBool(Jumping) == false)
            {
                anim.SetBool(Jumping, true);
            }
            else if (grounded && anim.GetBool(Jumping))
            {
                anim.SetBool(Jumping, false);
            }
        }
        #endregion
    }

    private void FixedUpdate()
    {
        CheckGrounded();
    }
    void CheckGrounded()
    {
        Physics.OverlapSphereNonAlloc(transform.position + Vector3.up * characterRadius/2, characterRadius, groundColliders, groundMask);
        grounded = groundColliders[0];
    }

    void Update()
    {
        GetAttackingInput();
        GetMovementInput();
        AddGravity();
        ApplyMovement();
        RotateToMovementDirection();
    }


    void GetAttackingInput()
    {
        if (Input.GetButtonDown("Attack"))
            attackManager.TryToAttack();
    }
    
    void GetMovementInput()
    {
        //reading the input:
        horizontalAxis = Input.GetAxis("Horizontal");
        verticalAxis = Input.GetAxis("Vertical");
         
        if (Mathf.Approximately(horizontalAxis, 0) && Mathf.Approximately(verticalAxis, 0))
            moving = false;
        else
            moving = true;

        if (moving && Input.GetButton("Run"))
            running = true;
        else
            running = false;

        if (grounded && Input.GetButtonDown("Jump") && attackManager.CanMove)
        {
            verticalVector.y = jumpPower;
            groundColliders = new Collider[1];
        }

        //camera forward and right vectors:
        var forward = Camera.main.transform.forward;
        var right = Camera.main.transform.right;
 
        //project forward and right vectors on the horizontal plane (y = 0)
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();    
        right.Normalize();

        //this is the direction in the world space we want to move:
        movementVector = forward * verticalAxis + right * horizontalAxis;
    }

    void AddGravity()
    {
        if (characterController.isGrounded && verticalVector.y < 0)
        {
            verticalVector.y = 0;
            return;   
        }
        
        verticalVector.y -= gravity * Time.deltaTime;
    }
    
    void ApplyMovement()
    {
        //now we can apply the movement:
        float resultMovementSpeed = movementSpeed;
        if (running)
            resultMovementSpeed += runningSpeedBonus;
        
        if (attackManager.CanMove)
            characterController.Move((movementVector * resultMovementSpeed) * Time.deltaTime);
        
        characterController.Move(verticalVector * Time.deltaTime);
    }

    void RotateToMovementDirection()
    {
        if (!moving || !attackManager.CanRotate)
            return;

        lookRotation.SetLookRotation(movementVector, Vector3.up);
        var targetRotation = Quaternion.Slerp(transform.rotation, lookRotation, turningSpeed * Time.deltaTime);
        transform.eulerAngles = new Vector3(0, targetRotation.eulerAngles.y, 0);
    }

}