using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationPhysics : MonoBehaviour
{
    Rigidbody rb;
    Animator animator;
    AnimatorClipInfo[] currentClipInfo;
    string clipName;

    [Header("Ragdoll Physics")]
    public bool ragdollEnabled;

    [Header("IK Physics")]
    public bool enableIK;
    public bool gunEnabled;

    //IK
    public float fallingIKMultiplier;
    public float fallingIKSmoother;
    float rotateUpDown = 65;
    float rotateSide = -65;
    float rightFootPositionWeight;
    float leftFootPositionWeight;
    float rightFootRotationWeight;
    float leftFootRotationWeight;
    float lookWeight;

    //Origin Ray Hits
    RaycastHit rightFootIKRayHit;
    RaycastHit leftFootIKRayHit;
    RaycastHit frontRayHit;
    RaycastHit backRayHit;

    //Current Animation Position Ray Hits
    RaycastHit animRightFootRayHit;
    RaycastHit animLeftFootRayHit;

    //Goal Postions
    Vector3 rightIKFootGoal;
    Vector3 leftIKFootGoal;    

    //Current Positions
    Vector3 animRightFootPosition;
    Vector3 animLeftFootPosition;

    //Current Rotations
    Quaternion animRightFootRotation;
    Quaternion animLeftFootRotation;

    //Origin Transforms
    public Transform rightFootIKRayOrigin;
    public Transform leftFootIKRayOrigin;
    public Transform frontRayOrigin;
    public Transform backRayOrigin;
    public Transform rightHandIK;
    public Transform leftHandIK;

    Quaternion rightY;
    Quaternion leftY;

    CharacterController controller;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }

    float fallingIK;

    // Update is called once per frame
    void Update()
    {
        //current avatar positions
        animRightFootPosition = animator.GetBoneTransform(HumanBodyBones.RightFoot).position;
        animLeftFootPosition = animator.GetBoneTransform(HumanBodyBones.LeftFoot).position;
        //current avatar rotations
        animRightFootRotation = animator.GetBoneTransform(HumanBodyBones.RightFoot).rotation;
        animLeftFootRotation = animator.GetBoneTransform(HumanBodyBones.LeftFoot).rotation;

        //raycasts for current current avatar positions and rotations
        Physics.Raycast(animRightFootPosition, Vector3.down, out animRightFootRayHit);
        Physics.Raycast(animLeftFootPosition, Vector3.down, out animLeftFootRayHit);

        //raycasts for idle positions and rotations
        Physics.Raycast(rightFootIKRayOrigin.position, Vector3.down, out rightFootIKRayHit);
        Physics.Raycast(leftFootIKRayOrigin.position, Vector3.down, out leftFootIKRayHit);
        //Physics.Raycast(leftHandIK.position, -transform.right, out leftHandRayHit);
        
        fallingIK = 1 / ((GetComponent<SimpleCharacterMotor>().groundRayHit.distance * GetComponent<SimpleCharacterMotor>().groundRayHit.distance) / fallingIKMultiplier);
        
        if (controller.isGrounded)
        {
            rightFootPositionWeight = Mathf.Lerp(rightFootPositionWeight, animator.GetFloat("Right Foot Curve") - GetComponent<SimpleCharacterMotor>().movementInput.magnitude, Time.fixedDeltaTime * 10 / (Mathf.Max(frontRayHit.distance, backRayHit.distance) * Mathf.Max(frontRayHit.distance, backRayHit.distance)));
            leftFootPositionWeight = Mathf.Lerp(leftFootPositionWeight, animator.GetFloat("Right Foot Curve") - GetComponent<SimpleCharacterMotor>().movementInput.magnitude, Time.fixedDeltaTime * 10 / (Mathf.Max(frontRayHit.distance, backRayHit.distance) * Mathf.Max(frontRayHit.distance, backRayHit.distance)));
        }
        else if (animator.GetFloat("Falling Curve") > 0)
        {
            rightFootPositionWeight = Mathf.Lerp(rightFootPositionWeight, fallingIK, Time.fixedDeltaTime * fallingIKSmoother);
            leftFootPositionWeight = Mathf.Lerp(rightFootPositionWeight, fallingIK, Time.fixedDeltaTime * fallingIKSmoother);
        }
        else if (GetComponent<CharacterController>().isGrounded == false)
        {
            rightFootPositionWeight = 0;
            leftFootPositionWeight = 0;
        }
        
    }

    public float slopeSmooth;

    void OnAnimatorIK(int layerIndex)
    {
        if (enableIK)
        {
            SlopeCorrection();

            //position weight
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightFootPositionWeight);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, leftFootPositionWeight);

            //rotation weight
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, rightFootRotationWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, leftFootRotationWeight);

            //IK for idle animations
            animator.SetIKPosition(AvatarIKGoal.RightFoot, new Vector3(rightFootIKRayOrigin.position.x, rightFootIKRayHit.point.y + .11f, rightFootIKRayOrigin.position.z));
            animator.SetIKPosition(AvatarIKGoal.LeftFoot, new Vector3(leftFootIKRayOrigin.position.x, leftFootIKRayHit.point.y + .11f, leftFootIKRayOrigin.position.z));

            //rotation
            animator.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.Euler(animRightFootRotation.x + rightFootIKRayHit.normal.z * 60, transform.rotation.y + rightFootIKRayOrigin.localRotation.y, animRightFootRotation.z + rightFootIKRayHit.normal.x * 60));
            animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.Euler(animLeftFootRotation.x + leftFootIKRayHit.normal.z * 60, animLeftFootRotation.y + transform.rotation.y, animLeftFootRotation.z + leftFootIKRayHit.normal.x * 60));

            rightY = Quaternion.FromToRotation(animator.GetBoneTransform(HumanBodyBones.RightFoot).forward, rightFootIKRayOrigin.forward);
            leftY = Quaternion.FromToRotation(animator.GetBoneTransform(HumanBodyBones.LeftFoot).forward, leftFootIKRayOrigin.forward);

            animator.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.Euler(animRightFootRotation.x + rightFootIKRayHit.normal.z * rotateUpDown, rightY.y, animRightFootRotation.z + rightFootIKRayHit.normal.x * rotateSide));
            animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.Euler(animLeftFootRotation.x + leftFootIKRayHit.normal.z * rotateUpDown, leftY.y, animLeftFootRotation.z + leftFootIKRayHit.normal.x * rotateSide));

            animator.SetFloat("Front Ray Hit Distance", frontRayHit.distance);
            animator.SetFloat("Slope Angle", Mathf.Lerp(animator.GetFloat("Slope Angle"), GetComponent<SimpleCharacterMotor>().groundRayHit.normal.z * rotateUpDown, Time.deltaTime * slopeSmooth));

            /*
            smooth = Mathf.Lerp(smooth, 1 - leftHandRayHit.distance, Time.deltaTime * handSmooth);
            if (leftHandRayHit.distance > 1)
            {
                smooth = Mathf.Lerp(smooth, 0, Time.deltaTime * handSmooth);
            }
            smooth = Mathf.Clamp01(smooth);

            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, smooth);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandRayHit.point + handOffset);

            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, smooth);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandIK.rotation);
            */
        }

        animator.SetLookAtWeight(1);
        animator.SetLookAtPosition(Camera.main.GetComponent<CameraController>().publicCrossHair);

        if (gunEnabled)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandIK.position);

            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandIK.rotation);
        }

        

        //animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 1);
        //animator.SetIKHintPosition(AvatarIKHint.RightElbow, rightElbowIK.position);

        //animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
        //animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandIK.position);

        //animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
        //animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandIK.rotation);

    }

    public float colliderSmooth;
    public float colliderOffset;
    public float minColliderHeight;

    float greaterIK;
    float smallerIK;
    float greaterSlope;
    float smallerSlope;

    float finalHeight;

    void SlopeCorrection()
    {
        //front and back rays
        Physics.Raycast(frontRayOrigin.position, Vector3.down, out frontRayHit);
        Physics.Raycast(backRayOrigin.position, Vector3.down, out backRayHit);

        greaterIK = Mathf.Max(rightFootIKRayHit.distance, leftFootIKRayHit.distance);
        smallerIK = Mathf.Min(rightFootIKRayHit.distance, leftFootIKRayHit.distance);

        greaterSlope = Mathf.Max(frontRayHit.distance, backRayHit.distance);
        smallerSlope = Mathf.Min(frontRayHit.distance, backRayHit.distance);

        finalHeight = ((smallerIK * colliderOffset) - (greaterIK * colliderOffset)) - (1 - Mathf.Clamp01(GetComponent<SimpleCharacterMotor>().groundRayHit.distance)) + 1.8f;
        //finalHeight = ((smallerSlope * colliderOffset) - (greaterSlope * colliderOffset)) + 1.8f;

        controller.height = Mathf.Lerp(controller.height, finalHeight, Time.deltaTime * colliderSmooth);
        controller.height = Mathf.Clamp(controller.height, minColliderHeight, 1.8f);
        
    }

}
