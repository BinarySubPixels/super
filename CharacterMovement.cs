using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    #region variables

    Animator animator;
    AnimatorClipInfo[] currentClipInfo;
    string clipName;

    //game objects
    Rigidbody rb;
    public Transform skeletonRig;
    public Transform regulatorAngle;
    public Transform realCameraAngle;

    Vector3 movementInput;
    Vector3 moveDir;
    float moveForce;
    float maxMoveVelocity;

    [Header ("Maximum Velocity") ]
    //maximum velocity variables
    public float maxCrouchVelocity;
    public float maxWalkVelocity;
    public float maxJogVelocity;
    public float maxSprintVelocity;

    [Header ("Force Variables") ]
    //force variables
    public float regularForce;
    public float airControlForce;

    [Header ("Jumping Variables") ]
    //jumping
    public float jumpForce;
    public float jumpDivisor;
    float jumpMultiplier;
    
    public bool wantsToJump;
    public bool mirrorJump;
    public bool isAllowedToJump;
    public bool jumped;

    [Header ("Stamina Variables") ]
    //stamina
    public float maxStamina;
    public float staminaDrainRate;
    public float staminaLeft;
    public float lowStaminaMultiplier;
    public bool staminaDepleted;
    float originalDrain;

    [Header ("Ground Check") ]
    //ground checking variables
    RaycastHit groundRayHit;
    Vector3 groundCheckRayOrigin;
    public bool isGrounded;
    public bool isColliding;
    public float IKDistanceTotal;

    [Header ("Parkour Variables") ]
    //parkour
    public Collider mainCollider;
    Vector3 forwardRayOrigin;
    bool isCrouching = false;

    [Header ("Wall Check Variables") ]
    //wall checking variables
    RaycastHit forwardRayHit;
    public Transform wallCheckRayOrigin;
    public float playerDistanceToWall = .75f;
    public float requiredWallRunDistance = .4f;
    public bool wallOnRight;
    public bool wallOnLeft;

    public Vector3 XZvelocity;
    Vector3 slowYvelocity;

    //left and right
    public bool isWallRunningOnRight;
    public bool isWallRunningOnLeft;

    #endregion variables 

    void Start()
    {
        animator = GetComponent<Animator>();

        rb = GetComponent<Rigidbody>();

        moveForce = regularForce;
        maxMoveVelocity = maxJogVelocity;

        originalDrain = staminaDrainRate;

        staminaLeft = maxStamina;
    }

    void FixedUpdate()
    {
        //represents the x and z vectors of the rigidbody velocity
        XZvelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        #region Wall Running

        //wall running for right side
        /* 

        //right wall check
        RaycastHit rightWallCheck;
        Physics.Raycast(transform.position, Vector3.right, out rightWallCheck, playerDistanceToWall);
        if (rightWallCheck.collider != null && rightWallCheck.distance < requiredWallRunDistance)
        {
            wallOnRight = true;
        }
        else
        {
            wallOnRight = false;
        }

        //left wall check
        RaycastHit leftWallCheck;
        Physics.Raycast(transform.position, Vector3.left, out leftWallCheck, playerDistanceToWall);
        if (leftWallCheck.collider != null && leftWallCheck.distance < requiredWallRunDistance)
        {
            wallOnLeft = true;
        }
        else
        {
            wallOnLeft = false;
        } 

        if (wallOnRight && Input.GetButton("Sprint") && XZvelocity.magnitude > maxJogVelocity + 1 && isGrounded == false)
        {
            XZvelocityClamped = new Vector3(rb.velocity.x, 0, rb.velocity.z);

            moveForce = regularForce / 2;

            isWallRunningOnRight = true;
            isWallRunningOnLeft = false;

            animator.SetBool("Wall Run Right", true);
        }

        //wall running for left side
        else if (wallOnLeft && Input.GetButton("Sprint") && XZvelocity.magnitude > maxJogVelocity + 1 && isGrounded == false)
        {
            XZvelocityClamped = new Vector3(rb.velocity.x, 0, rb.velocity.z);

            moveForce = regularForce / 2;

            isWallRunningOnLeft = true;
            isWallRunningOnRight = false;

            animator.SetBool("Wall Run Left", true);

        }
        else if (isGrounded == false && wallOnLeft == false || isGrounded == false && wallOnRight == false)
        {
            moveForce = airControlForce;
            animator.SetBool("Wall Run Right", false);
            animator.SetBool("Wall Run Left", false);
        }

        //XZ velocity clamped is what keeps the player from going faster than the maximum set velocity
        else if (isGrounded)
        {
            moveForce = regularForce;

            isWallRunningOnLeft = false;
            isWallRunningOnRight = false;
        }

        if (wallOnLeft == false)
        {
            isWallRunningOnLeft = false;
        }
        if (wallOnRight == false)
        {
            isWallRunningOnRight = false;
        } */

        #endregion //currently deactivated

        #region Regular Movement
        moveForce = regularForce;

        if (staminaLeft == maxStamina)
        {
            staminaDepleted = false;
        }

       

        staminaLeft = staminaLeft + staminaDrainRate * Time.fixedDeltaTime;

        staminaDrainRate = originalDrain;

        if (isCrouching)
        {
            maxMoveVelocity = Mathf.Lerp(maxMoveVelocity, maxCrouchVelocity, .1f);
        }
        else if (Input.GetButton("Aim") && isGrounded)
        {
            maxMoveVelocity = Mathf.Lerp(maxMoveVelocity, maxWalkVelocity, .1f);
        }
        else if (Input.GetButton("Sprint") && isGrounded && XZvelocity.magnitude > maxWalkVelocity && movementInput.magnitude == 1 && staminaLeft > 0 && staminaDepleted == false)
        {
            maxMoveVelocity = Mathf.Lerp(maxMoveVelocity, maxSprintVelocity, .1f);

            staminaLeft = staminaLeft - staminaDrainRate * Time.fixedDeltaTime * 2;

            if (staminaLeft < 0 + Time.fixedDeltaTime)
            {
                staminaDepleted = true;
            }
        }
        else if (isGrounded && staminaDepleted)
        {
            maxMoveVelocity = Mathf.Lerp(maxMoveVelocity, maxWalkVelocity, .05f);

            staminaLeft = staminaLeft + staminaDrainRate * lowStaminaMultiplier * Time.fixedDeltaTime;
        }
        else if (isGrounded && staminaDepleted == false)
        {
            maxMoveVelocity = Mathf.Lerp(maxMoveVelocity, maxJogVelocity, .05f);
        }

        staminaLeft = Mathf.Clamp(staminaLeft, 0, maxStamina);

        if (isGrounded == false)
        {
            moveForce = airControlForce;

            staminaDrainRate = 0;
        }

        XZvelocity = Vector3.ClampMagnitude(XZvelocity, maxMoveVelocity);

        //movement; use normalized so that you don't go faster when you run diagonally
        movementInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;

        rb.AddForce(regulatorAngle.forward * moveForce * movementInput.z);
        rb.AddForce(regulatorAngle.right * moveForce * movementInput.x);

        rb.velocity = new Vector3(XZvelocity.x, rb.velocity.y, XZvelocity.z);

        #endregion

    }

    void Update()
    {
        #region Raycasts

        groundCheckRayOrigin = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
        forwardRayOrigin = new Vector3(transform.position.x, transform.position.y + .4f, transform.position.z + .5f);

        Physics.Raycast(groundCheckRayOrigin, Vector3.down, out groundRayHit);
        //Physics.CapsuleCast(groundCheckRayOrigin, groundCheckRayOrigin, .3f, Vector3.down, out groundRayHit);

        Physics.Raycast(forwardRayOrigin, transform.forward, out forwardRayHit);

        Debug.DrawRay(groundCheckRayOrigin, Vector3.down);
        Debug.DrawRay(forwardRayOrigin, transform.forward);
        Debug.DrawRay(forwardRayOrigin, (regulatorAngle.forward + new Vector3(0, Mathf.Abs(groundRayHit.normal.z), 0)).normalized);

        #endregion

        //Ground Check
        if (groundRayHit.distance < IKDistanceTotal)
        { isGrounded = true; }
        else { isGrounded = false; }

        //Animation Clip Info
        currentClipInfo = animator.GetCurrentAnimatorClipInfo(0);
        clipName = currentClipInfo[0].clip.name;

        //crouching
        if (Input.GetButtonDown("Crouch") && isCrouching == false)
        { isCrouching = true; }
        else if (Input.GetButtonDown("Crouch") && isCrouching || Input.GetButtonDown("Sprint") && movementInput.magnitude == 1 || Input.GetButton("Jump"))
        { isCrouching = false; }

        #region Jump Constraints

        //Keeps the variable true for every frame, only is false when the condition below are true
        isAllowedToJump = true;

        if (clipName == "Vault Two Handed" || clipName == "Stand To Roll" || clipName == "Step Up Jump" || clipName == "Jump Over" || clipName == "Running Roll")
        {
            isAllowedToJump = false;
            staminaDrainRate = 0;
            movementInput = Vector3.zero;
            maxMoveVelocity = maxSprintVelocity * 1.5f;
        }

        //keeps player from moving or jumping during these animations
        if (clipName == "One Handed Landing" || clipName == "Standing To Crouched")
        {
            isAllowedToJump = false;
            movementInput = Vector3.zero;
        }

        if (clipName == "Stumble Adjust" || clipName == "Falling Idle" || clipName == "Standing Idle To Action Idle") { isAllowedToJump = false;}

        if (Mathf.Max(animator.GetFloat("Left Foot Curve"), animator.GetFloat("Right Foot Curve")) < .5f)
        { isAllowedToJump = false; }

        if (animator.GetFloat("Left Foot Curve") > 0)
        { mirrorJump = true; } else { mirrorJump = false; }
        
        #endregion

        #region Jumping
        //Jumping
        jumped = false;

        jumpMultiplier = (XZvelocity.magnitude / jumpDivisor) + 1;

        if (Input.GetButtonDown("Jump") && isGrounded)
        { wantsToJump = true; }

        if (wantsToJump && isAllowedToJump && jumped == false)
        { jumped = true; }

        if (jumped)
        {
            rb.AddForce(transform.up * jumpForce * jumpMultiplier, ForceMode.Impulse);
            staminaLeft = staminaLeft - jumpForce / 20 * jumpMultiplier / 2;
            animator.SetBool("Mirror Jump", mirrorJump);
            wantsToJump = false;
        }

        #endregion

        #region Animation Variables

        animator.SetFloat("Distance to Ground", groundRayHit.distance);
        
        animator.SetFloat("Running Velocity", Mathf.Clamp(XZvelocity.magnitude, Mathf.Lerp(movementInput.magnitude, 0, Time.fixedDeltaTime * 25), maxMoveVelocity));

        animator.SetFloat("Downward Velocity", rb.velocity.y);

        animator.SetFloat("Forward Proximity", forwardRayHit.distance);

        animator.SetBool("Is Grounded", isGrounded);

        if (movementInput.magnitude == 1) { animator.SetBool("Is Holding Movement Keys", true); } else { animator.SetBool("Is Holding Movement Keys", false); }

        animator.SetBool("Crouched", isCrouching);
        animator.SetBool("Pressed Crouch Button", Input.GetButton("Crouch"));

        animator.SetBool("Jumped", jumped);
        animator.SetBool("Pressed Jump Button", Input.GetButton("Jump"));

        animator.SetBool("Pressed Dodge", Input.GetButton("Dodge"));

        animator.SetBool("Pressed Sprint", Input.GetButton("Sprint"));

        #endregion

        /* if (movementInput.z > 0)
        {
            moveDir = (movementInput + regulatorAngle.forward).normalized;
        }
        if (movementInput.z < 0)
        {
            moveDir = (movementInput - regulatorAngle.forward).normalized;
        } */
        if (movementInput.magnitude > 0)
        { transform.rotation = Quaternion.Slerp(transform.rotation, new Quaternion(0, regulatorAngle.rotation.y, 0, regulatorAngle.rotation.w), Time.fixedDeltaTime * 5); }
        //transform.forward = Vector3.Lerp(transform.forward, moveDir, Time.fixedDeltaTime * 15);
    }

    void LateUpdate()
    {
        
    }
}
