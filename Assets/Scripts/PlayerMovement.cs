using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Animator animator;    // Asignar en el inspector (Footballer)

    private float speed;
    public float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;
    public Transform cam;

    void Start()
    {
        ActualizarStats();
        // Si hay Rigidbody y no lo usas, elimínalo en el inspector o ponlo kinematic
    }

    void Update()
    {
        ActualizarStats();

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }
        else
        {
            // si no hay input no movemos
            controller.Move(Vector3.zero);
        }

        // Calcular velocidad real sobre el plano XZ
        Vector3 horizontalVelocity = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
        float currentSpeed = horizontalVelocity.magnitude;

        // Enviar con damp para evitar parpadeos (0.1f damping)
        animator.SetFloat("speed", currentSpeed, 0.1f, Time.deltaTime);
    }

    void ActualizarStats()
    {
        PlayerStats stats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
        if (stats != null)
        {
            speed = stats.speed;
        }
    }
}
