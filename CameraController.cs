using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    Animator animator;
    Vector3 target;
    public Transform skeleton;
    public GameObject player;
    public GameObject gun;
    CharacterController controller;

    [Header ("Camera Settings")]
    public float aimFOV;
    public float dollyShiftDistance;
    public float followSpeed;
    public float aimFollowSpeed;
    public float sensitivity;
    public float ADSMultiplier;
    public float FOVZoomSpeed;
    public float dollyZoomSpeed;

    float adjustedOffsetZ;
    float regularFOV;
    float adjustedFOV;
    float regularSensitivity;
    float programSense = 30;
    float finalFollowSpeed;

    [Header ("Time Shift")]
    public float bulletTime;
    public float bulletTimeSmooth;
    bool timeShiftAllowed;

    [Header("Ranged Ability")]
    //Ranged Ability
    public GameObject crosshairImage;
    public Transform focusPoint;
    private Vector3 rayOrigin;

    //variables from other scripts
    float currentVelocity;
    bool usingSensory;

    public Vector3 publicCrossHair;
    public Vector3 cameraOffset;
    private Vector3 originalCameraOffset;
    public Vector2 rotationLimitsY;

    Vector2 mouseInput;
    Vector2 analogInput;
    Vector2 finalRotation;

    //animation variables
    public GameObject hexagons;
    Animator hexagonsAnimator;
    public float aimingHexagonSpeed;

    float originalRotation;
    public float divisor;

    void Start()
    {
        animator = player.GetComponent<Animator>();
        hexagonsAnimator = hexagons.GetComponent<Animator>();
        controller = player.GetComponent<CharacterController>();

        regularFOV = GetComponent<Camera>().fieldOfView;
        regularSensitivity = sensitivity;
    }

    public RaycastHit crosshairHit;

    void Update()
    {

        //keeps you from seeing the cursor during gameplay and locks it in position
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //crosshair ray
        
        var aimRay = Camera.main.ScreenPointToRay(crosshairImage.transform.position);

        //cursor ray
        Physics.Raycast(aimRay, out crosshairHit);
        if (crosshairHit.collider != null)
        {
            publicCrossHair = crosshairHit.point;
        }
        else
        {
            publicCrossHair = focusPoint.position;
        }

        #region powers

        /* if (crosshairHit.collider != null)
        {
            cursorOnObject = true;
        }
        else
        { cursorOnObject = false; }

        if (crosshairHit.rigidbody != null && crosshairHit.distance < distance && Input.GetButton("Ranged Ability"))
        {
            canLift = true;
        }
        else
        {
            canLift = false;
        }

        liftedObject = crosshairHit.rigidbody;

        if (canLift)
        {
            followVector = crosshairHit.transform.position;

            followVector = Vector3.Lerp(followVector, focusPoint.position, .5f);

            liftedObject.MovePosition(followVector);

            liftedObject.interpolation = RigidbodyInterpolation.Interpolate;
            liftedObject.useGravity = false;
            liftedObject.drag = 2;
            liftedObject.angularDrag = 2;
        }
        else
        {
            liftedObject.interpolation = RigidbodyInterpolation.None;
            liftedObject.useGravity = true;
            liftedObject.drag = 0;
            liftedObject.angularDrag = 0;
            liftedObject = null;
        } */

        #endregion

        hexagonsAnimator.SetFloat("Aim Multiplier", 1);

        TimeShiftConstraints();

        if (Input.GetButton("Positive Fire") || Input.GetButton("Negative Fire"))
        {
            //Aim FOV and Dolly Distance
            gameObject.GetComponent<Camera>().fieldOfView = Mathf.Lerp(gameObject.GetComponent<Camera>().fieldOfView, aimFOV, Time.deltaTime * FOVZoomSpeed);
            adjustedOffsetZ = Mathf.Lerp(adjustedOffsetZ, cameraOffset.z + dollyShiftDistance, Time.deltaTime * dollyZoomSpeed);

            hexagonsAnimator.SetFloat("Aim Multiplier", aimingHexagonSpeed);

            if (timeShiftAllowed)
            {
                Time.timeScale = Mathf.Lerp(Time.timeScale, bulletTime, Time.fixedDeltaTime * bulletTimeSmooth);
                Time.fixedDeltaTime = .02f * Time.timeScale;
                finalFollowSpeed = aimFollowSpeed;
            }
            else
            {
                Time.timeScale = Mathf.Lerp(Time.timeScale, 1, Time.fixedDeltaTime * bulletTimeSmooth);
                Time.fixedDeltaTime = .02f;
                finalFollowSpeed = followSpeed;
            }
        }
        else
        {
            //Original FOV and Dolly Distance
            gameObject.GetComponent<Camera>().fieldOfView = Mathf.Lerp(gameObject.GetComponent<Camera>().fieldOfView, regularFOV, Time.deltaTime * FOVZoomSpeed * .7f);
            adjustedOffsetZ = Mathf.Lerp(adjustedOffsetZ, cameraOffset.z, Time.deltaTime * dollyZoomSpeed / .7f);

            hexagonsAnimator.SetFloat("Aim Multiplier", aimingHexagonSpeed);

            Time.timeScale = Mathf.Lerp(Time.timeScale, 1, Time.fixedDeltaTime * bulletTimeSmooth);
            Time.fixedDeltaTime = .02f;
            finalFollowSpeed = followSpeed;
        } 

        /* if (Input.GetButtonDown("Sensory Ability") && !usingSensory)
        {
            Time.timeScale = bulletTime;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            usingSensory = true;
        }
        else if (Input.GetButtonDown("Sensory Ability") && usingSensory)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
            usingSensory = false;
        } */

        
    }

    float adjustedOffsetX = .5f;

    void TimeShiftConstraints()
    {
        if (controller.isGrounded == false || animator.GetFloat("Slide Curve") > 0)
        {
            timeShiftAllowed = true;
        }
        else
        {
            timeShiftAllowed = false;
        }
    }

    void LateUpdate()
    {
        
        //controller input
        analogInput.x += Input.GetAxis("Right Analog X") * sensitivity * programSense * Time.deltaTime;
        analogInput.y -= Input.GetAxis("Right Analog Y") * sensitivity * programSense * Time.deltaTime;

        //mouse input
        mouseInput.x += Input.GetAxis("Mouse X") * sensitivity * programSense * Time.deltaTime;
        mouseInput.y -= Input.GetAxis("Mouse Y") * sensitivity * programSense * Time.deltaTime;

        finalRotation = mouseInput + analogInput;
        originalRotation = finalRotation.y;

        /* if (finalRotation.y >= 0)
        {
            finalRotation.y = originalRotation;
            adjustedOffsetZ = cameraOffset.z;

        }
        else if (finalRotation.y < 0 && finalRotation.y > -50)
        {
            adjustedOffsetZ = cameraOffset.z - (finalRotation.y * (-cameraOffset.z / divisor));
        } */

        //X is minimum, Y is maximum
        finalRotation.y = Mathf.Clamp(finalRotation.y, rotationLimitsY.x, rotationLimitsY.y);

        Quaternion rotation = Quaternion.Euler(finalRotation.y, finalRotation.x, 0);

        if (player.GetComponent<SimpleCharacterMotor>().isWallRunningOnRight)
        {
            adjustedOffsetX = Mathf.Lerp(adjustedOffsetX, -cameraOffset.x, Time.fixedDeltaTime * 2);
        }
        else
        {
            adjustedOffsetX = Mathf.Lerp(adjustedOffsetX, cameraOffset.x, Time.fixedDeltaTime * 2);
        }

        target = Vector3.Lerp(target, skeleton.position, finalFollowSpeed * Time.fixedDeltaTime);

        Vector3 position = rotation * new Vector3(adjustedOffsetX, cameraOffset.y, adjustedOffsetZ) + target;

        transform.rotation = rotation;
        transform.position = position;
    }
}