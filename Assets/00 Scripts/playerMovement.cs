using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class playerMovement : MonoBehaviour
{
    public Transform cameraTransform;

    [Header("Moving")]
    bool isGrounded;
    public float walkSpeed = 8f;
    public float runSpeed = 18f;
    float moveSpeed;
    float actualMoveSpeed;
    Vector3 movement;
    Vector3 playerVelocity;
    float groundCheckDistance = 1.1f;
    public float jumpHeight = 2.5f;
    bool sprinting;
    
    bool crouching;
    CharacterController controller;

    [Header("Turning")]
    public float xSens = 150f;
    public float ySens = 120f;
    private Vector2 mouseMovement;
    float xRotationCam;
    bool turningEnabled;




    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = transform.GetChild(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeSinceLevelLoad > 0.1f)
        {
            handleInput();
            moving();
            turning();
        }

        handleCamera();
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////





    void handleInput()
    {
        crouching = (Input.GetKey(KeyCode.LeftControl));
        // controller.height = crouching ? 1.2f : 3.3f;
        sprinting = (Input.GetKey(KeyCode.LeftShift));

        if (Input.GetKeyDown(KeyCode.Space))
            Jump();

        if (Input.GetKeyUp(KeyCode.Space) && playerVelocity.y > 0f) // Stop the jump if you let go of space
            playerVelocity.y/=2f;
    }


    void moving()
    {
        moveSpeed = sprinting ? runSpeed : walkSpeed;
        movement = transform.TransformDirection(new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")));
        if (movement.magnitude == 0) moveSpeed = 0f;


        actualMoveSpeed += (moveSpeed - actualMoveSpeed) / 50f;

        controller.Move(movement * actualMoveSpeed * Time.deltaTime);


        if (!isGrounded) playerVelocity.y += Time.deltaTime * Physics.gravity.y;
        controller.Move(playerVelocity * Time.deltaTime);


        // jumpHeight = -1 / ((actualMoveSpeed / 10f) + 0.3f) + 4.833f;
        isGrounded = (controller.isGrounded || Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, LayerMask.NameToLayer("Terrain")));

        if (isGrounded)
        {
            playerVelocity = Vector3.down * Mathf.Max(3f, -playerVelocity.y);
            //print("GROUNDED");
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
            xRotationCam = Mathf.Clamp(xRotationCam, -90f, 75f);
        }

        ///////////////////////////////////////////////////////////

    }

    void handleCamera()
    {
        // POV Camera
        cameraTransform.localEulerAngles = new Vector3(xRotationCam, 0f, 0f);
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 

    void Jump()
    {
        if (isGrounded)
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);

        // else if (playerVelocity.y > 0)
        //     playerVelocity = movement * 20f + Vector3.up;
    }
}
