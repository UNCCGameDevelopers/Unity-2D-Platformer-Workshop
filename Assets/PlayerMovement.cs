using Microsoft.Cci;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    #region Step 1

    //Step 1 : Create variables for referencing our players components
    private PlayerInput input; //this is private because we assign it in code
    private Rigidbody2D rb; //this is private because we assign it in code
    public GameObject cam; //this is public because we assign it manually

    //Step 1 : Create Input Actions 
    private InputAction jumpAction;
    private InputAction moveAction;

    //Step 1 : create isGrounded variables
    public float groundCheckDist = 0.1f;
    //Step 1

    //Step 1 : Create configurable variables
    [Header("Movement Parameters")] //Explain that this will show a header in the inspector to categorize variables
    [Range(1, 20)] public float moveSpeed = 12f; //Say that this variable is for modifying our movement vector to actually create speed.
    [Range(1, 20)] public float maxSpeed = 14f;
    [Range(1, 10)] public float maxDeccelSpeed = 5f; //When you let go of the controls clamp speed to this value.

    private Vector2 moveDirection; //Step 1 : our move direction vector
    private Vector2 currentInput; //Step 1 : our input vector

    #endregion

    #region Step 2

    public bool isGrounded => Physics2D.BoxCast(transform.position, this.GetComponent<BoxCollider2D>().size, 0f, -transform.up, groundCheckDist, rCasting);
    public bool inAir => !jumping && !isGrounded;
    public bool doJump;

    

    

    private LayerMask rCasting;

    [Header("Jumping Parameters")]
    [SerializeField] private int jumpCount = 1; //What we modify and check when jumping.
    public int jumpTotal = 1; //Total jumps, so for instance if you wanted 3 jumps set this to 3.
    [SerializeField] private bool jumpCanceled;
    [SerializeField] private bool jumping;
    public float jumpHeight = 5f; //Our jump height, set this to a specific value and our player will reach that height with a maximum deviation of 0.1
    [SerializeField] private float buttonTime;
    [SerializeField] private float jumpTime;
    public float jumpDist; //This is not in the actual tutorial because I only used it for testing the distance of the actual jump.
    public Vector2 ogJump; //Not included just like what I said above.
    public float fallMultiplier = 2.5f; //When you reach the peak of the expected arc this is the force applied to make falling more fluid.
    public float lowJumpMultiplier = 2f; //When you stop holding jump to do a low jump this is the force applied to make the jump stop short.
    public float Multiplier = 100f; //This is so we can scale with deltatime properly.

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        #region Step 1
        input = GetComponent<PlayerInput>(); //Assign input component
        rb = GetComponent<Rigidbody2D>(); //Assign rigidbody component
        moveAction = input.actions["Move"]; //Assign moveAction to our Move action in our player input actions.


        #endregion

        #region Step 2
        jumpAction = input.actions["Jump"]; //Assign jump action
        rCasting = LayerMask.GetMask("Player"); //Assign our layer mask to player
        rCasting = ~rCasting; //Invert the layermask value so instead of being just the player it becomes every layer but the mask
        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        #region Step 2
        
        #region bool control
        doJump |= (jumpAction.GetButtonDown() && jumpCount > 0 && !jumping); //We use |= because we want doJump to be set from true to false
        //This ^ Operator is the or equals operator, it's kind of hard to explain so hopefully I explain this correctly,
        //Basically its saying this : doJump = doJump || (jumpAction.GetButtonDown() && jumpCount > 0 && !jumping)
        //Which is too say, if doJump is already true we return true otherwise we check (jumpAction.GetButtonDown() && jumpCount > 0 && !jumping)
        //The reason we do this is because the only time we want doJump = false is when we directly set it later in the code after we
        //call the jump function. So unless we are setting doJump to false it will be able to return true and only check our conditional
        //again once it is set to false.
        //in the event that doJump is already false and our conditional returns true;
        if (isGrounded)
        {
            if (!jumping)
            {
                jumpCount = jumpTotal; //Reset jump count when we land.
                jumpCanceled = false;
            }
        }

        if (jumping) //Track the time in air when jumping
        {
            jumpTime += Time.deltaTime;
        }

        if (jumping && !jumpCanceled)
        {

            if (jumpAction.GetButtonUp()) //If we stop giving input for jump cancel jump so we can have a variable jump.
            {
                jumpCanceled = true;
            }

            if (jumpTime >= buttonTime) //When we reach our projected time stop jumping and begin falling.
            {
                jumpCanceled = true;
                jumpDist = Vector2.Distance(transform.position, ogJump); //Not needed, just calculates distance from where we started jumping to our highest point in the jump.
            }
        }

        if (jumpCanceled) //Cancel the jump.
        {
            jumping = false;
        }

        #endregion

        #endregion

        #region Step 1
        HandleMovementInput();
        #endregion
    }

    private void FixedUpdate()
    {
        #region Step 2
        HandleJump();
        #endregion

        #region Step 1
        ApplyFinalMovements(); //Explain that applying force to a rigidbody can only occur in a fixed update because fixed update is the physics step.
        #endregion

    }

    private void HandleMovementInput() //Step 1
    {
        //Explain that this vector2 is based off of our moveAction, For each key depending on what is pressed we get a value from 0-1 W is positive, S negative, A negative, D positive.
        currentInput = moveAction.ReadValue<Vector2>(); //Step 1:



        currentInput.y = 0; //Explain that we set the y velocity this way because otherwise the player would be able to just fly up or down by pressing up or down.
        moveDirection = ((currentInput.x * cam.transform.right.normalized) + (currentInput.y * cam.transform.up.normalized)) * moveSpeed; //This binds our controls to the characters right and forward directions so when we rotate control remains the same.
        
    }

    private void HandleJump() //Step 2
    {
        

        if (doJump)
        {
            doJump = false;
            jumpCount--;
            ogJump = transform.position;
            float jumpForce;

            jumpForce = Mathf.Sqrt(2f * Physics2D.gravity.magnitude * jumpHeight) * rb.mass; 
            buttonTime = (jumpForce / (rb.mass * Physics2D.gravity.magnitude)); //initial velocity divided by player accel for gravity gives us the amount of time it will take to reach the apex.
            rb.velocity = new Vector2(rb.velocity.x, 0); //Reset y velocity before we jump so it is always reaching desired height.
            rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse); //don't normalize transform.up cus it makes jumping more inconsistent.
            jumpTime = 0;
            jumping = true;
            jumpCanceled = false;
        }

        //Where I learned this https://www.youtube.com/watch?v=7KiK0Aqtmzc
        //This is what gives us consistent fall velocity so that jumping has the correct arc.
        Vector2 localVel = transform.InverseTransformDirection(rb.velocity);

        if (localVel.y < 0 && inAir) //If we are in the air and at the top of the arc then apply our fall speed to make falling more game-like
        {
            Vector2 jumpVec = Multiplier * -transform.up * (fallMultiplier - 1) * Time.deltaTime;
            rb.AddForce(jumpVec, ForceMode2D.Force);
        }
        else if (localVel.y > 0 && !(jumpAction.phase == InputActionPhase.Performed) && inAir) //If we stop before reaching the top of our arc then apply enough downward velocity to stop moving, then proceed falling down to give us a variable jump.
        {
            Vector2 jumpVec = Multiplier * -transform.up * (lowJumpMultiplier - 1) * Time.deltaTime;
            rb.AddForce(jumpVec, ForceMode2D.Force);
        }
    }

    private void ApplyFinalMovements() //Step 1
    {

        rb.AddForce(moveDirection, ForceMode2D.Force);
        if (currentInput.x != 0) //What this does is when we stop giving input (when the player tries to stop moving) we deccel in a specific 
        {                        //range by setting velocity to our max deccel speed so it slows down to 0 from that value.
            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxSpeed, maxSpeed), rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxDeccelSpeed, maxDeccelSpeed), rb.velocity.y);
        }
    }
}
