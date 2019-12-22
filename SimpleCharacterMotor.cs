using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCharacterMotor : MonoBehaviour
{
    Animator animator;
    CharacterController controller;

    [Header("Movement")]
    public float crouchSpeed;
    public float walkSpeed;
    public float jogSpeed;
    public float sprintSpeed;
    public float airControlSpeed;
    [Space]
    public float acceleration;
    public float precision;
    float moveSmooth;
    public float airDrag;
    public float rotateSpeed;
    string movementType;
    bool isCrouching;

    public Transform regulatorAngle;
    public Transform IKRotator;
    Quaternion originalIKRotation;
    bool originalCreated;

    Vector3 forward;
    Vector3 right;
    [HideInInspector]
    public Vector3 movementInput;
    Vector3 desiredMoveDirection;
    Vector3 targetRotationDirection;
    float desiredMoveSpeed;
    Vector3 baseMovement;
    Vector3 locomotion;
    Vector3 airControl;
    
    Vector3 animAddSpeedX;
    Vector3 animAddSpeedY;
    Vector3 animAddSpeedZ;

    [Header("Jumping Variables")]
    //jumping
    public float gravity;
    public float jumpSpeed;
    public float jumpDivisor;
    float jumpMultiplier;
    public float verticalVelocity;
    bool mirrorJump;
    bool isAllowedToJump;
    bool jump;
    bool hasJumped;

    [Header("Stamina Variables")]
    //stamina
    public float maxStamina;
    public float staminaDrainRate;
    public float staminaLeft;
    public float lowStaminaMultiplier;
    public bool staminaDepleted;
    float jumpStamina;

    Vector3 finalMovement;
   
    [Space]
    [Header("Ground Check")]
    //ground checking variables
    public RaycastHit groundRayHit;
    Vector3 groundCheckRayOrigin;

    [Space]
    [Header("Wall Check Variables")]
    //wall checking variables
    public Transform forwardRayOrigin;
    public RaycastHit forwardRayHit;
    public float requiredWallRunDistance;
    public float gravityDecrease;
    public RaycastHit rightWallCheck;
    public RaycastHit leftWallCheck;
    public bool isWallRunningOnRight;
    public bool isWallRunningOnLeft;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        staminaLeft = maxStamina;
    }

    void Update()
    {
        Locomotion();
        Raycasts();
        Animation();
    }

    void Locomotion()
    {
        MovementTypes();
        Stamina();

        movementInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized + new Vector3(Input.GetAxis("Left Analog X"), 0, Input.GetAxis("Left Analog Y"));
        movementInput = Vector3.ClampMagnitude(movementInput, 1);

        if (animator.GetFloat("Freeze Input Curve") < 1)
        {
            //movementInput.z = Mathf.Clamp(movementInput.z, -animator.GetFloat("Freeze Input Curve") / 2, animator.GetFloat("Freeze Input Curve") / 2);
            movementInput = Vector3.ClampMagnitude(movementInput, animator.GetFloat("Freeze Input Curve"));
        }

        forward = regulatorAngle.forward;
        right = regulatorAngle.right;
        forward.Normalize();
        right.Normalize();
        desiredMoveDirection = forward * movementInput.z + right * movementInput.x;

        switch (movementType)
        {
            case "Crouching":
                desiredMoveSpeed = crouchSpeed;
                break;
            case "Walking":
                desiredMoveSpeed = walkSpeed;
                break;
            case "Jogging":
                desiredMoveSpeed = jogSpeed;
                break;
            case "Sprinting":
                desiredMoveSpeed = sprintSpeed;
                break;
            case "Tired":
                desiredMoveSpeed = walkSpeed;
                break;
            case "Jumped":
                break;
            case "Falling":
                break;
            case "Idle":
                break;
        }

        //Animation Add Speed
        animAddSpeedX = transform.right * animator.GetFloat("Add Speed X");
        animAddSpeedY = transform.up * animator.GetFloat("Add Speed Y");
        animAddSpeedZ = transform.forward * animator.GetFloat("Add Speed Z");

        //Acceleration
        if (movementInput.magnitude < 1 || desiredMoveSpeed < 1) { moveSmooth = precision; } else { moveSmooth = acceleration; }

        //Movement
        if (controller.isGrounded)
        {
            baseMovement = Vector3.Lerp(baseMovement, desiredMoveDirection * desiredMoveSpeed * movementInput.magnitude, Time.deltaTime * moveSmooth);

            airControl = Vector3.zero;
        }
        else
        {
            airControl = Vector3.Lerp(airControl, desiredMoveDirection * airControlSpeed * movementInput.magnitude, Time.deltaTime * acceleration);
        }

        //Drag
        if (controller.isGrounded == false) { baseMovement = baseMovement - (baseMovement * Time.fixedDeltaTime * (airDrag / 10)); }

        //Final Movement
        finalMovement = baseMovement + airControl + animAddSpeedX + animAddSpeedY + animAddSpeedZ;

        //Locomotion
        locomotion = new Vector3(finalMovement.x, 0, finalMovement.z);
        
        //Jump and Gravity
        JumpAndGravity();

        //Rotation
        if (movementInput.magnitude > 0)
        {
            targetRotationDirection = desiredMoveDirection;
        }

        if (controller.isGrounded == false)
        {
            if (Input.GetButton("Positive Fire") || Input.GetButton("Negative Fire"))
            {
                targetRotationDirection = regulatorAngle.forward;
                if (groundRayHit.distance > 4)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Camera.main.transform.forward), Time.deltaTime * rotateSpeed / 6);
                }
                else
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(regulatorAngle.forward), Time.deltaTime * rotateSpeed / 6);
                    
                }
            }
            else 
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetRotationDirection), Time.deltaTime * (3 / Mathf.Clamp(baseMovement.magnitude, 1, baseMovement.magnitude / 2) ) );
            }
            /* else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Camera.main.transform.forward), Time.deltaTime * rotateSpeed / 2);
            }

            if (controller.isGrounded && originalCreated == false)
            {
                originalIKRotation = IKRotator.rotation;
                originalCreated = true;
            } 

            IKRotator.rotation = originalIKRotation; */
        }
        else
        {
            if (movementInput.magnitude > 0 && baseMovement.magnitude > .5f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(locomotion), Time.deltaTime * rotateSpeed);
            }

            //IKRotator.rotation = Quaternion.Slerp(IKRotator.rotation, transform.rotation, Time.deltaTime * rotateSpeed / 4);
        }

        if (groundRayHit.distance < 4)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, new Quaternion(0, transform.rotation.y, 0, transform.rotation.w), Time.deltaTime * rotateSpeed / 6);
        }

        if (animator.GetFloat("Root Motion Curve") > 0) { animator.applyRootMotion = true; } else { animator.applyRootMotion = false; } 
        
        controller.Move(finalMovement * Time.deltaTime);

        //Grapple();
    }

    void JumpAndGravity()
    {
        AnimationBasedConstraints();

        jumpMultiplier = new Vector2(finalMovement.x, finalMovement.z).magnitude / jumpDivisor;

        jump = false;

        if (isAllowedToJump && Input.GetButtonDown("Jump") && controller.isGrounded)
        { jump = true; }

        if (jump)
        {
            if (animator.GetFloat("Mirror Curve") > 0) { animator.SetBool("Mirror Jump", true); } else { animator.SetBool("Mirror Jump", false); }

            if (staminaDepleted) { jumpStamina = lowStaminaMultiplier; }
            else if (isWallRunningOnLeft || isWallRunningOnRight)
            { jumpStamina = lowStaminaMultiplier * 4; }
            else { jumpStamina = 1; }

            verticalVelocity = jumpSpeed * (jumpMultiplier + 1);

            staminaLeft = staminaLeft - jumpSpeed * (jumpMultiplier + 1) * jumpStamina;

            hasJumped = true;
        }

        //Gravity
        if (controller.isGrounded)
        {
            verticalVelocity = Mathf.Clamp(verticalVelocity, -gravity * Time.deltaTime, Mathf.Infinity);

            if (jump == false)
            {
                hasJumped = false;
            }
        }
        else
        {
            verticalVelocity = verticalVelocity - gravity * Time.deltaTime;
        }

        //Terminal Velocity Clamp
        verticalVelocity = Mathf.Clamp(verticalVelocity, -54, Mathf.Infinity);

        WallRunning();

        //Assignment
        finalMovement = new Vector3(finalMovement.x, verticalVelocity, finalMovement.z);
    }

    void Raycasts()
    {
        #region Raycasts

        groundCheckRayOrigin = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);

        Physics.Raycast(groundCheckRayOrigin, Vector3.down, out groundRayHit);
        Physics.Raycast(forwardRayOrigin.position, transform.forward, out forwardRayHit);

        //Physics.CapsuleCast(groundCheckRayOrigin, groundCheckRayOrigin, .3f, Vector3.down, out groundRayHit);

        Debug.DrawRay(groundCheckRayOrigin, Vector3.down);
        Debug.DrawRay(forwardRayOrigin.position, transform.forward);
        Debug.DrawRay(forwardRayOrigin.position, desiredMoveDirection);

        #endregion
    }

    float animMoveSpeed;
    float animJumpSpeed = .5f;


    void Animation()
    {
        animator.SetBool("Jump", jump);

        animator.SetBool("Has Jumped", hasJumped);

        animator.SetBool("Pressed Jump Button", Input.GetButton("Jump"));

        animator.SetBool("Is Grounded", controller.isGrounded);

        animator.SetFloat("Distance to Ground", groundRayHit.distance);

        animMoveSpeed = Mathf.Lerp(animMoveSpeed, new Vector2(finalMovement.x, finalMovement.z).magnitude, Time.deltaTime * precision);
        animator.SetFloat("Anim Move Speed", animMoveSpeed);

        animJumpSpeed = Mathf.Clamp(animMoveSpeed, 5, 10);
        animator.SetFloat("Anim Jump Speed", (1 / (animJumpSpeed / 2)) * 2);

        animator.SetFloat("Vertical Velocity", Mathf.Abs(controller.velocity.y));

        animator.SetBool("Crouched", isCrouching);
        animator.SetBool("Pressed Crouch Button", Input.GetButton("Crouch"));

        animator.SetBool("Pressed Dodge Button", Input.GetButton("Dodge"));

        if (movementType == "Sprinting") { animator.SetBool("Sprinting", true); } else { animator.SetBool("Sprinting", false); }
        animator.SetBool("Pressed Sprint Button", Input.GetButton("Sprint"));

        animator.SetFloat("Movement Input", movementInput.magnitude);

        animator.SetFloat("Input Z", Mathf.Lerp(animator.GetFloat("Input Z"), movementInput.z, Time.deltaTime * 5));
        animator.SetFloat("Input X", Mathf.Lerp(animator.GetFloat("Input X"), movementInput.x, Time.deltaTime * 5));

        //moveDirectionRotation = Quaternion.LookRotation(desiredMoveDirection).y * 180;

        //if ((Quaternion.LookRotation(desiredMoveDirection).y * 180) - transform.rotation.y >= 135)
        //{ animator.SetBool("Change Direction", true); } else { animator.SetBool("Change Direction", false); }

    }

    void AnimationBasedConstraints()
    {
        //Keeps the variable true for every frame, only is false when the condition below are true
        isAllowedToJump = false;

        if (animator.GetFloat("Jump Curve") > 0)
        {
            isAllowedToJump = true;
        }
        if (animator.GetFloat("Jump Curve") < -.5f)
        {
            hasJumped = false;
        }
    }

    void Stamina()
    {
        if (staminaLeft == maxStamina)
        { staminaDepleted = false; }
        else if (staminaLeft < Time.fixedDeltaTime)
        { staminaDepleted = true; }

        if (controller.isGrounded)
        {
            staminaLeft = staminaLeft + staminaDrainRate * Time.fixedDeltaTime;
        }
        if (staminaDepleted == false && movementType == "Sprinting")
        {
            staminaLeft = staminaLeft - staminaDrainRate * 2 * Time.fixedDeltaTime;
        }
        else if (staminaDepleted == false && movementType == "Wall Running")
        {
            staminaLeft = staminaLeft - staminaDrainRate * Time.fixedDeltaTime;
        }
        else if (staminaDepleted && controller.isGrounded)
        {
            staminaLeft = staminaLeft - staminaDrainRate / 2 * Time.fixedDeltaTime;
        }

        staminaLeft = Mathf.Clamp(staminaLeft, 0, maxStamina);

    }

    void MovementTypes()
    {
        if (Input.GetButtonDown("Crouch") && isCrouching == false && new Vector3(finalMovement.x, 0, finalMovement.z).magnitude < sprintSpeed * .75f && groundRayHit.distance < 1.5f)
        { isCrouching = true; }
        else if (Input.GetButtonDown("Crouch") && isCrouching || jump || Input.GetButton("Sprint") && movementInput.magnitude > .5f || groundRayHit.distance > 1.5f)
        { isCrouching = false; }

        if (isWallRunningOnRight || isWallRunningOnLeft)
        {
            movementType = "Wall Running";
        }
        else if (isCrouching && controller.isGrounded)
        {
            movementType = "Crouching";
        }
        else if (staminaDepleted && controller.isGrounded)
        {
            movementType = "Tired";
        }
        else if (movementInput.magnitude > 0 && movementInput.magnitude < .5f && controller.isGrounded || Input.GetButton("Positive Fire") && controller.isGrounded || Input.GetButton("Negative Fire") && controller.isGrounded)
        {
            movementType = "Walking";
        }
        else if (Input.GetButton("Sprint") && movementInput.magnitude > .5f && controller.isGrounded)
        {
            movementType = "Sprinting";
        }
        else if (movementInput.magnitude > .5f && controller.isGrounded)
        {
            movementType = "Jogging";
        }
        else if (controller.isGrounded == false)
        {
            movementType = "Falling";
        }
        else if (movementInput.magnitude == 0 && controller.isGrounded)
        {
            movementType = "Idle";
        }

    }

    void WallRunning()
    {
        //right wall check
        Physics.Raycast(transform.position, transform.right, out rightWallCheck, requiredWallRunDistance);

        //left wall check
        Physics.Raycast(transform.position, -transform.right, out leftWallCheck, requiredWallRunDistance);

        isWallRunningOnRight = false;
        if (rightWallCheck.collider != null && Input.GetButton("Sprint") && new Vector3(finalMovement.x, 0, finalMovement.z).magnitude > jogSpeed && movementInput.magnitude > .5f && controller.isGrounded == false && staminaDepleted == false)
        {
            isWallRunningOnRight = true;
        }

        isWallRunningOnLeft = false;
        if (leftWallCheck.collider != null && Input.GetButton("Sprint") && new Vector3(finalMovement.x, 0, finalMovement.z).magnitude > jogSpeed && movementInput.magnitude > .5f && controller.isGrounded == false && staminaDepleted == false)
        {
            isWallRunningOnLeft = true;
        }

        if (isWallRunningOnLeft || isWallRunningOnRight)
        {
            verticalVelocity = verticalVelocity + (gravity * Time.deltaTime * gravityDecrease);
        }

        animator.SetBool("Wall Run Right", isWallRunningOnRight);
        animator.SetBool("Wall Run Left", isWallRunningOnLeft);
    }

    bool anchored = false;

    Vector3 grapplingTrajectory;

    void Grapple()
    {
        if (Input.GetButton("Use Gadget") && Camera.main.GetComponent<CameraController>().crosshairHit.collider != null && Camera.main.GetComponent<CameraController>().crosshairHit.distance < 10)
        {
            anchored = true;

            grapplingTrajectory.y = new Vector3(finalMovement.x, finalMovement.z).magnitude * new Vector2(finalMovement.x, finalMovement.z).magnitude;

            transform.position = new Vector3(transform.position.x, grapplingTrajectory.y, transform.position.z);
        }
        else
        {
            anchored = false;
        }
    }
}
