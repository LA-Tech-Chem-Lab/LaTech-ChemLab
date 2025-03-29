using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity.Netcode;

namespace Unity.Multiplayer.Center.NetcodeForGameObjectsExample{
    public class playerMovement : MonoBehaviour
    {
        public Transform cameraTransform;
        public Transform targetCamPosition;

        [Header("Locks")]
        public bool canMove = true;
        public bool canTurn = true;
        public bool isTyping = false;

        [Header("Moving")]
        bool isGrounded;
        public float crouchSpeed = 2f;
        public float walkSpeed = 8f;
        public float runSpeed = 18f;
        float moveSpeed;
        float actualMoveSpeed; public float realWorldMoveSpeed;
        Vector3 movement;
        Vector3 playerVelocity;
        float groundCheckDistance = 1.1f;
        public float jumpHeight = 2.5f;
        bool sprinting; bool crouching;
        
        CharacterController controller;

        [Header("Turning")]
        public float xSens = 150f; public float ySens = 120f;
        Vector2 mouseMovement;
        public float xRotationCam = 0f;
        bool turningEnabled;

        public Animator playerAnimator;
        Vector3 prevPosition;
        interactWithObjects interactScript;


        // Start is called before the first frame update
        void Start()
        {
            controller = GetComponent<CharacterController>();
            interactScript = GetComponent<interactWithObjects>();
            cameraTransform = transform.GetChild(0);


            // if (IsLocalPlayer) // Disable Mesh for current client
            // {
            //     transform.Find("MESH")?.gameObject.SetActive(false);
            // }
        }

        // Update is called once per frame
        void Update()
        {
            handleInput();
            moving();
            if (canTurn) turning();
            handleAnimations();
            handleCamera();
            
            prevPosition = transform.position;

            if (transform.position.y < -80f){ // Teleport to the teacher if you fall off terrain
                transform.position = new Vector3(12f,2f,-0.91f);
                transform.localEulerAngles = new Vector3(0f, 88.4f, 0f);
            }





            
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////





        void handleInput()
        {
            crouching = (Input.GetKey(KeyCode.LeftControl));
            sprinting = (Input.GetKey(KeyCode.LeftShift));
            // controller.height = crouching ? 1.2f : 2.4f;

            if (Input.GetKeyDown(KeyCode.Space) && canMove)
                Jump();

            if (Input.GetKeyUp(KeyCode.Space) && playerVelocity.y > 0f) // Stop the jump if you let go of space
                playerVelocity.y/=2f;

        }


        void moving()
        {
            moveSpeed = sprinting ? runSpeed : walkSpeed;

            moveSpeed = crouching ? crouchSpeed : moveSpeed;

            movement = Vector3.Normalize(transform.TransformDirection(new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"))));
            if (movement.magnitude == 0) moveSpeed = 0f;
            else actualMoveSpeed = Mathf.Lerp(actualMoveSpeed, moveSpeed, 10f * Time.deltaTime);


            if (canMove)
                controller.Move(movement * actualMoveSpeed * Time.deltaTime);
            

            if (!isGrounded) playerVelocity.y += Time.deltaTime * Physics.gravity.y;
            controller.Move(playerVelocity * Time.deltaTime);

            realWorldMoveSpeed = (transform.position - prevPosition).magnitude/Time.deltaTime;

            bool prevInAir = !isGrounded;
            // jumpHeight = -1 / ((actualMoveSpeed / 10f) + 0.3f) + 4.833f;
            isGrounded = (controller.isGrounded || Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, LayerMask.NameToLayer("Terrain")));

            if (isGrounded)
            {
                playerVelocity = Vector3.down * Mathf.Max(3f, -playerVelocity.y);
                
                if (prevInAir && isGrounded){ // We Just Landed
                    playerAnimator.SetTrigger("Landed");
                }
            }

        }

        void turning()
        {
            turningEnabled = !Cursor.visible;
            mouseMovement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            ///////////////////////////////////////////////////////////

            float xTurn = mouseMovement[0] * xSens * Time.deltaTime;
            float yTurn = mouseMovement[1] * ySens * Time.deltaTime;

            if (turningEnabled)
            {
                // Left + Right
                transform.Rotate(Vector3.up, xTurn);

                // Up + Down
                xRotationCam -= yTurn;
                xRotationCam = Mathf.Clamp(xRotationCam, -72.5f, 85f);
            }

            ///////////////////////////////////////////////////////////

        }

        public void updateTyping(){
            canMove = !isTyping;
            canTurn = canMove;
        }

        void handleCamera()
        {
            // POV Camera
            cameraTransform.localEulerAngles = new Vector3(xRotationCam, 0f, 0f);
            
            Vector3 cameraTargetPosWithOffset = targetCamPosition.position + interactScript.eyeOffset;
            
            cameraTransform.position = Vector3.Slerp(cameraTransform.position, cameraTargetPosWithOffset, Time.deltaTime * 10f);


        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 

        void Jump()
        {
            if (isGrounded){
                playerAnimator.SetTrigger("Jump");
                playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            }
            // else if (playerVelocity.y > 0)
            //     playerVelocity = movement * 20f + Vector3.up;
        }

        void handleAnimations()
        {
            float betterSpeed = Mathf.Min(realWorldMoveSpeed, actualMoveSpeed);
            playerAnimator.SetFloat("Value", betterSpeed); // Local update for owner
            
        }


    }
}