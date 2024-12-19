using UnityEngine;

public class JumpScript : MonoBehaviour
{
    public float jumpForce = 5f; // Adjust the jump force as needed
    private bool isGrounded = false;
    private Rigidbody rb;
    
    private AudioManager audioManager;

    private void Awake()
    {
        // Find the AudioManager by its tag
        //audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();

        if (audioManager == null)
        {
            Debug.LogError("AudioManager not found! Make sure it has the 'Audio' tag.");
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Check if the spacebar is pressed and if the capsule is grounded
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
    }

    void Jump()
    {
        // Add a vertical force to make the capsule jump
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false; // The capsule is now in the air

        // Play the jump sound effect through AudioManager
        if (audioManager != null)
        {
            //audioManager.PlaySFX(audioManager.Jump); // Play the jump sound
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the capsule has landed on a plane or other ground surface
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}
