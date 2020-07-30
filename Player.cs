using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour
{
    public static Player instance;

    public float moveSpeed = 6f;
    public float accelerationTimeGrounded;
    public float maxJumpHeight = 2f;
    public float minJumpHeight = 0.7f;
    public float timeToJumpApex;
    
    public float accelerationTimeAirborne;

    public float coyoteTime = .2f;
    private float coyoteCounter;

    public float jumpBufferLength = .1f;
    private float jumpBufferCount;

    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;
    public float wallSlideSpeedMax = 3;

    public float gravity;
    float maxJumpVelocity;
    float minJumpVelocity;
    float velocityXSmoothing;

    public float knockbackLength;
    public Vector3 knockbackForce;
    private float knockbackCounter;

    private bool isTouchingGround;
    public bool isFacingRight;

    private Animator playerAnimator;

    Vector3 velocity;

    Controller2D controller;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<Controller2D>();
        playerAnimator = GetComponent<Animator>();

        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
        print("Gravity: " + gravity + " Jump Velocity: " + maxJumpVelocity);
    }

    // Update is called once per frame
    void Update()
    {

        if (knockbackCounter <= 0)
        {

            
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            int wallDirx = (controller.collisions.left) ? -1 : 1;


            bool wallSliding = false;
            if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below & velocity.y < 0)
            {
                wallSliding = true;

                if (velocity.y < -wallSlideSpeedMax)
                {
                    velocity.y = -wallSlideSpeedMax;
                }
            }
            
            if (controller.collisions.above || controller.collisions.below)
            {
                velocity.y = 0;
            }


            if (input.x < 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
                isFacingRight = false;
            }
            else if (input.x > 0)
            {
                transform.localScale = new Vector3(1, 1, 1);
                isFacingRight = true;
            }

            
            if (controller.collisions.below)
            {
                isTouchingGround = true;
            }
            else
            {
                isTouchingGround = false;
            }

            if (isTouchingGround)
            {
                coyoteCounter = coyoteTime;
            }
            else
            {
                coyoteCounter -= Time.deltaTime;
            }
            if (Input.GetButtonDown("Jump") && coyoteCounter > 0f)
            {
                velocity.y = maxJumpVelocity;
            }

            if (Input.GetButtonUp("Jump"))
            {
                if (velocity.y > minJumpVelocity)
                {
                    velocity.y = minJumpVelocity;
                }
            }

            if (Input.GetButtonDown("Jump") && wallSliding)
            {
                if (wallDirx == input.x)
                {
                    velocity.x = -wallDirx * wallJumpClimb.x;
                    velocity.y = wallJumpClimb.y;
                }
                else if (input.x == 0)
                {
                    velocity.x = -wallDirx * wallJumpOff.x;
                    velocity.y = wallJumpOff.y;
                }
                else
                {
                    velocity.x = -wallDirx * wallLeap.x;
                    velocity.y = wallLeap.y;
                }
            }

            
            float targetVelocityX = input.x * moveSpeed;
            velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime, input);
        } else
        {
            knockbackCounter -= Time.deltaTime;
            velocity.y = knockbackForce.y;
            if (isFacingRight)
            {
                velocity.x = -knockbackForce.x;
                

            }
            else
            {
                velocity.x = knockbackForce.x;
               
            }
        }

        playerAnimator.SetBool("isGrounded", isTouchingGround);
        playerAnimator.SetFloat("moveSpeed", Mathf.Abs(velocity.x));

    }

    public void KnockBack()
    {
        knockbackCounter = knockbackLength;
        velocity.y = knockbackForce.y;  
    }

}
