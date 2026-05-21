using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Animator animator;

    [Header("Movimiento Base")]
    private float speed;
    public float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;
    public Transform cam;

    [Header("Sistema de Sprint (Estamina)")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float sprintMultiplier = 1.8f;
    public float staminaDrainRate = 25f;
    public float staminaRegenRate = 15f;
    private bool isExhausted = false;

    // Y inicial: el juego es top-down con el jugador siempre a la misma altura.
    // Cualquier desviación (empujes verticales, escalones, colliders) se corrige
    // al final de cada frame para evitar tanto el vuelo como la caída del mapa.
    private float yInicial;
    private bool  yInicialCapturada = false;

    [Header("Interfaz (UI)")]
    public Slider sliderEstamina;

    private PlayerStats statsCache;

    void Start()
    {
        statsCache = GetComponent<PlayerStats>();
        if (statsCache == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) statsCache = player.GetComponent<PlayerStats>();
        }
        ActualizarStats();
        currentStamina = maxStamina;

        yInicial = transform.position.y;
        yInicialCapturada = true;

        if (sliderEstamina != null)
        {
            sliderEstamina.maxValue = maxStamina;
            sliderEstamina.value = currentStamina;
        }
    }

    void Update()
    {
        ActualizarStats();

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        bool isMoving = direction.magnitude >= 0.1f;
        bool isTryingToSprint = Input.GetKey(KeyCode.Space);

        bool isSprinting = isTryingToSprint && isMoving && !isExhausted;

        if (isSprinting)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;

            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                isExhausted = true;
            }
        }
        else
        {
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;

                if (currentStamina >= maxStamina)
                {
                    currentStamina = maxStamina;
                    isExhausted = false;
                }
            }
        }

        if (sliderEstamina != null)
            sliderEstamina.value = currentStamina;

        float currentSpeedLimit = isSprinting ? (speed * sprintMultiplier) : speed;

        if (isMoving)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            controller.Move(moveDir.normalized * currentSpeedLimit * Time.deltaTime);
        }
        else
        {
            controller.Move(Vector3.zero);
        }

        Vector3 horizontalVelocity = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
        float currentSpeed = horizontalVelocity.magnitude;

        animator.SetFloat("speed", currentSpeed, 0.1f, Time.deltaTime);
    }

    void ActualizarStats()
    {
        if (statsCache != null)
            speed = statsCache.speed;
    }

    void LateUpdate()
    {
        // Bloquea la altura del jugador a la inicial. Evita que enemigos lo
        // empujen hacia arriba (vuelo) y que se caiga del mapa si por algún
        // motivo el CharacterController no detecta suelo.
        if (!yInicialCapturada) return;
        if (Mathf.Approximately(transform.position.y, yInicial)) return;

        controller.enabled = false;
        Vector3 p = transform.position;
        p.y = yInicial;
        transform.position = p;
        controller.enabled = true;
    }
}