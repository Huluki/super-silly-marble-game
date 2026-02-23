using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System;
using System.Collections;


[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float speed = 10;
    
    
    public static Rigidbody rb;
    private InputManager input;
    
    [SerializeField] private Transform camParent;
    [SerializeField] private Transform cam;
    
    [SerializeField] private float rotSpeed = 10f;
    [SerializeField] private float jumpPower = 10f;
    
    [SerializeField] private float moveForce = 30f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float airControlMultiplier = 0.2f;
    [SerializeField] private float groundDrag = 5f;
    [SerializeField] private float extraGravity = 20f;
    [SerializeField] float maxSlopeAngle = 45f;
    
    [SerializeField]  private TextMeshProUGUI powerUpText;
    [SerializeField] private TextMeshProUGUI goalText;
    [SerializeField] private float level = 0;
    [SerializeField] private Vector3 respawnPoint = new Vector3(0, 1, 0);
    
    [SerializeField] Transform marbleVisual;
    [SerializeField] float radius = 0.75f;
    private string lookType = "d";
    
    private Vector3 groundNormal;
    private float slopeAngle;
    private bool speedChanger =  false;
    private bool goalChanger = false;
    private bool shouldReset = false;

    private float currentJump;
    private float currentSpeed;
    
    private float currentGravity;

    private float powerUpNum = 0;

    private float groundCheckRadius = .75f * .3f;
    private float groundCheckDistance = 1f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        input = InputManager.instance;
        powerUpNum = 0;
        goalText.enabled = false;
    }
    
    private void FixedUpdate()
    {
        HandleMovement(Time.fixedDeltaTime);
        SpeedChange();
        Reset();
        Debug.Log(powerUpNum);
    }
    
    private bool IsGrounded()
    {
        // Cast a sphere downwards from the center of the marble
         // Adjust based on your marble size
        float groundDistance = 0.3f; // Margin for error
        
        RaycastHit groundHit;
        bool trueGrounded = Physics.SphereCast(transform.position, radius * 0.4f, Vector3.down, out groundHit, groundCheckDistance);
        
        if(trueGrounded){
         groundNormal = groundHit.normal;
         slopeAngle = Vector3.Angle(groundNormal, Vector3.up);
        }
        
        return trueGrounded;
    }

    
    
  

    private void HandleMovement(float deltaTime)
    {
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camForward * input.Move.y + camRight * input.Move.x;
        
        Vector3 horizontalVel = rb.linearVelocity;
        horizontalVel.y = 0;

        if (horizontalVel.magnitude > maxSpeed)
        {
            rb.linearVelocity = horizontalVel.normalized * maxSpeed + Vector3.up * rb.linearVelocity.y;
        }

        if (IsGrounded() && input.Move.magnitude < 0.1f)
        {
            rb.linearVelocity = Vector3.Lerp(
                rb.linearVelocity,
                new Vector3(0, rb.linearVelocity.y, 0),
                groundDrag * Time.fixedDeltaTime
            );
        }

        if (!IsGrounded())
        {
            rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
        }
        
        float controlMultiplier = IsGrounded() ? 1f : airControlMultiplier;
        
        Vector3 slopeMoveDir = Vector3.ProjectOnPlane(moveDir, groundNormal).normalized;
        
        IsGrounded();
        
        bool tooSteep = slopeAngle > maxSlopeAngle;

        
        if (IsGrounded() && !tooSteep)
        {
            rb.AddForce(slopeMoveDir * moveForce, ForceMode.Acceleration);
        }
        else if (!IsGrounded())
        {
            rb.AddForce(moveDir * (moveForce * controlMultiplier), ForceMode.Acceleration);
        }
    }
    
    

    private void SpeedChange()
    {
        if (speedChanger)
        {
            float duration = 3f;
            
            maxSpeed = Mathf.MoveTowards(maxSpeed, currentSpeed, duration * Time.deltaTime);
            Wait(duration, 1);
        }

        if (goalChanger)
        {
            
            maxSpeed = Mathf.MoveTowards(maxSpeed, 0, 20 * Time.deltaTime);
            
        }
    }
    
    

    public void OnJump()
    {
        // Only trigger on the initial press (performed) and if grounded
        if (IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        Vector3 origin = transform.position + Vector3.down * 0.05f;

        // Top sphere (start)
        Gizmos.DrawWireSphere(origin, groundCheckRadius);

        // Bottom sphere (end)
        Vector3 end = origin + Vector3.down * groundCheckDistance;
        Gizmos.DrawWireSphere(end, groundCheckRadius);

        // Connect them
        Gizmos.DrawLine(origin + Vector3.forward * groundCheckRadius, end + Vector3.forward * groundCheckRadius);
        Gizmos.DrawLine(origin - Vector3.forward * groundCheckRadius, end - Vector3.forward * groundCheckRadius);
        Gizmos.DrawLine(origin + Vector3.right * groundCheckRadius, end + Vector3.right * groundCheckRadius);
        Gizmos.DrawLine(origin - Vector3.right * groundCheckRadius, end - Vector3.right * groundCheckRadius);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("powerUpSpeed") && powerUpNum ==0)
        {
            powerUpNum = 1;
            powerUpText.text = "powerup: \n" +
                               "  speed";
            Destroy(other.gameObject);
        } else if (other.CompareTag("powerUpJump")&& powerUpNum ==0)
        {
            powerUpNum = 2;
            powerUpText.text = "powerup: \n" +
                               "  jump";
            Destroy(other.gameObject);
        }else if (other.CompareTag("powerUpBounce")&& powerUpNum ==0)
        {
            powerUpNum = 4;
            
            powerUpText.text = "powerup: \n" +
                               "  bounce";
            Destroy(other.gameObject);
        }else if (other.CompareTag("powerUpLowGrav")&& powerUpNum ==0)
        {
            powerUpNum = 5;
            
            powerUpText.text = "powerup: \n" +
                               "  low grav";
            Destroy(other.gameObject);
        }else if (other.CompareTag("reset"))
        {
            shouldReset =  true;
        }else if (other.CompareTag("goal"))
        {
            goalText.text = "YOU WON YAY HAPPY CELEBRATE!!";
            goalText.enabled = true;
            goalChanger = true;
            StartCoroutine(Wait(2,3));
        }
            
    }

    public void PowerUpCheck()
    {
        if (powerUpNum > 0)
        {
            if (powerUpNum == 1)
            {
                //1 speed
                
                Vector3 boostDir = cam.forward;
                boostDir.y = 0.0f;                 // keep boost horizontal
                boostDir.Normalize();
                
                
                currentSpeed = maxSpeed;
                maxSpeed = 80;
                rb.AddForce(boostDir * 80, ForceMode.VelocityChange);
                
                speedChanger = true;
                powerUpText.text = "powerup: " +
                                   "  ";
                powerUpNum = 0;
            } else if (powerUpNum == 2)
            {
                //2 jump
                
                currentJump = jumpPower;
                jumpPower = currentJump * 1.5f;
            
                StartCoroutine(Wait(5,2));
            }else if (powerUpNum == 4)
            {
                //4 bounce
                
                rb.AddForce(Vector3.up * (jumpPower*4), ForceMode.Impulse);
                
                //jumpPower =  currentJump;
                powerUpText.text = "powerup: " +
                                   "  ";
                powerUpNum = 0;
            }else if (powerUpNum == 5)
            {
                //5 low grav
                
               rb.useGravity = false;
               currentJump = jumpPower;
               jumpPower = currentJump * 1.3f;
                
               currentGravity = extraGravity;
               extraGravity = 15;
               StartCoroutine(Wait(10,5));
                
                
            }
        }
    }

    IEnumerator Wait(float duration, float instance)
    {
        powerUpNum = 0;
        yield return new WaitForSecondsRealtime(duration);
        if (instance == 1)
        {
            // speed
            speedChanger = false;
        } else if (instance == 2)
        {
            // jump
            
            jumpPower = currentJump;

            powerUpNum = 0;
            powerUpText.text = "powerup: " +
                               "  ";
        } else if (instance == 3)
        {
            // goal
            shouldReset = true;
            
            goalText.enabled = false;
            goalChanger = false;
            maxSpeed = 60;
        }else if (instance == 5)
        {
            extraGravity = currentGravity;
            jumpPower = currentJump;
            rb.useGravity = true;
            
            powerUpText.text = "powerup: " +
                               "  ";
            powerUpNum = 0;
        }
    }
    public void Reset()
    {
        Debug.Log("reset");
        if (shouldReset)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            powerUpText.text = "powerup: " +
                               "  ";
            transform.position = respawnPoint;
            shouldReset = false;
        }
    }
    
    public void OnLook(InputAction.CallbackContext context)
    {
        var device = context.control.device;

        if (device is Mouse)
        {
            lookType = "Mouse";
            CameraController.orbitSpeed = .006f;
            CameraController.followSpeed = 5f;
            CameraController.tiltSpeed = .003f;
        }
        else if (device is Gamepad)
        {
            CameraController.orbitSpeed = .008f;
            CameraController.followSpeed = 5f;
            CameraController.tiltSpeed = .0065f;
            lookType = "Gamepad";
            
        }
        
    }

    public void changeReset()
    {
        shouldReset = true;
    }
    
}