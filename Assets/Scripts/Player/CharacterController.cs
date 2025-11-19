using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;
    public float minX = -7f;
    public float maxX = 7f;

    [Header("Dash")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.6f;
    public float dashCooldown = 1f;

    private float dashCooldownTimer = 0f;
    private bool isDashing = false;

    [Header("Sonido de Dash")]
    public AudioClip dashSound;     // asignar en el inspector
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>(); //obtiene el AudioSource del jugador
    }

    private void Update()
    {
        HandleMovement();
        HandleDash();
    }

    private void HandleMovement()
    {
        if (isDashing) return;

        float inputX = 0f;

        if (Input.GetKey(KeyCode.A))
            inputX = -1f;
        else if (Input.GetKey(KeyCode.D))
            inputX = 1f;

        Vector3 move = new Vector3(inputX * moveSpeed * Time.deltaTime, 0f, 0f);
        transform.Translate(move);

        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
    }

    private void HandleDash()
    {
        dashCooldownTimer -= Time.deltaTime;

        if (isDashing) return;

        if (Input.GetKey(KeyCode.Space))
        {
            if (Input.GetKey(KeyCode.A))
                StartCoroutine(DoDash(-1f));

            else if (Input.GetKey(KeyCode.D))
                StartCoroutine(DoDash(1f));
        }
    }

    private System.Collections.IEnumerator DoDash(float direction)
    {
        if (dashCooldownTimer > 0f) yield break;

        // reproducir sonido del dash
        if (dashSound != null)
            audioSource.PlayOneShot(dashSound);

        isDashing = true;
        dashCooldownTimer = dashCooldown;

        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            float moveX = direction * dashSpeed * Time.deltaTime;
            transform.Translate(moveX, 0f, 0f);

            float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
            transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }
}

