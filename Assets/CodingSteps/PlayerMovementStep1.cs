using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementStep1 : MonoBehaviour
{
    #region step 1
    [Header("Assigned Parameters")]
    public GameObject cam; //this is public because we assign it manually

    [Header("Movement Parameters")] //Explain that this will show a header in the inspector to categorize variables
    [Range(1, 20)] public float moveSpeed = 12f; //Say that this variable is for modifying our movement vector to actually create speed.
    [Range(1, 20)] public float maxSpeed = 14f; //Say this variable is what we use to "cap" our speed so once we reach this we no longer increase in speed.
    //Explain that range creates a slider in the inspector for us to set the value so we don't set it too high.

    private PlayerInput input; //this is private because we assign it in code and nothing should be modifying it.
    private Rigidbody2D rb; //this is private because we assign it in code and nothing should be modifying it. 
    

    private InputAction moveAction;

    
    private Vector2 moveDirection; //our move direction vector
    private Vector2 currentInput; //our input vector

    #endregion

    #region step 2
    // Start is called before the first frame update
    void Start()
    {
        input = GetComponent<PlayerInput>(); //Assign input component attatched to the gameObject this script is attatched to
        rb = GetComponent<Rigidbody2D>(); //Assign rigidbody component like above
        moveAction = input.actions["Move"]; //Assign moveAction to our Move action in our player input actions, name is case sensitive.
        //EXPLAIN WHY WE USE THIS INPUT SYSTEM AND WHY IT IS AWESOME, Y'KNOW CUS OF THE ABILITY FOR IT TO AUTOMATICALLY SWITCH FROM
        //KEYBOARD TO CONTROLLER INPUT WITHOUT CHANGING THE CODE (for the most part).
    }
    #endregion

    #region step 4
    // Update is called once per frame
    void Update()
    {
        HandleMovementInput();
    }
    #endregion

    #region step 6
    private void FixedUpdate()
    {
        ApplyFinalMovements(); //Explain that applying force to a rigidbody can only occur in a fixed update because fixed update is the physics step.
    }
    #endregion

    #region step 3
    private void HandleMovementInput() //Step 1
    {
        //Explain that this vector2 is based off of our moveAction, For each key depending on what is pressed we get a value from 0-1 W is positive, S negative, A negative, D positive.
        currentInput = moveAction.ReadValue<Vector2>(); //Step 1:



        currentInput.y = 0; //Explain that we set the y velocity this way because otherwise the player would be able to just fly up or down by pressing up or down.
        moveDirection = ((currentInput.x * cam.transform.right.normalized) + (currentInput.y * cam.transform.up.normalized)) * moveSpeed; //This binds our controls to the characters right and forward directions so when we rotate control remains the same.

    }
    #endregion

    #region step 5
    private void ApplyFinalMovements() //Step 1
    {
        //Add our input based move direction to our players rigidbody as a force
        rb.AddForce(moveDirection, ForceMode2D.Force);
        //Clamp player velocity to our max speed.
        rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxSpeed, maxSpeed), rb.velocity.y);
    }
    #endregion
}
