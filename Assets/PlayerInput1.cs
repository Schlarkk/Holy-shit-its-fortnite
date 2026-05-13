using System.Data;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput1 : MonoBehaviour
{
    [SerializeField]private InputActionAsset input;
    [SerializeField]private float walkSpeed = 5f;
    [SerializeField]private float turnSpeed = 150f;
    [SerializeField]private float jumpForce = 5f;
    [SerializeField]private string mapName = "player";

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;

    private Rigidbody rb;
    private bool isGrounded = true;

    void Start()
    {
        InputActionMap map = input.FindActionMap(mapName);
        moveAction = map.FindAction("Move");
        jumpAction = map.FindAction("Jump");
        sprintAction = map.FindAction("Sprint");

        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()  { input.FindActionMap(mapName).Enable(); }
    void OnDisable() { input.FindActionMap(mapName).Disable(); }

    void Update()
    {
         // Opvragen van de input
        Vector2 moveInput = moveAction.ReadValue<Vector2>();

        //bepalen wat de snelheid is
        float speed = walkSpeed * moveInput.y;

        //sprinten
        if (sprintAction.IsPressed())
            speed *= 2f;

        //bewegen van de speler
        Vector3 movement = transform.forward * speed * Time.deltaTime;
        transform.Translate(movement, Space.World);

        //draaien van de speler
        float angle = moveInput.x * turnSpeed * Time.deltaTime;
        transform.Rotate(0f, angle, 0f, Space.World);


        // Springen
        if (jumpAction.WasPressedThisFrame() && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }


}
