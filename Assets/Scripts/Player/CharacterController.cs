using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;        // Velocidad normal de movimiento
    public float minX = -7f;            // Límite izquierdo
    public float maxX = 7f;             // Límite derecho

    [Header("Dash")]
    public float dashSpeed = 15f;       // Velocidad durante el dash
    public float dashDuration = 0.6f;   // Duración del dash en segundos
    public float dashCooldown = 1f;     // Tiempo de espera antes de poder volver a hacer dash

    private float dashCooldownTimer = 0f;
    private bool isDashing = false;

    private void Update()
    {
        HandleMovement();
        HandleDash();
    }

    private void HandleMovement()
    {
        if (isDashing) return; // Si estamos en dash, ignoramos el movimiento normal

        float inputX = 0f;

        if (Input.GetKey(KeyCode.A))
            inputX = -1f;
        else if (Input.GetKey(KeyCode.D))
            inputX = 1f;

        Vector3 move = new Vector3(inputX * moveSpeed * Time.deltaTime, 0f, 0f);
        transform.Translate(move);

        // Limitar la posición dentro de minX y maxX
        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
    }

    private void HandleDash()
    {
        dashCooldownTimer -= Time.deltaTime;

        // Si estamos en dash, no volvemos a activar otro
        if (isDashing) return;

        // Revisamos si se presiona ESPACIO + dirección
        if (Input.GetKey(KeyCode.Space))
        {
            if (Input.GetKey(KeyCode.A))
            {
                StartCoroutine(DoDash(-1f));
            }
            else if (Input.GetKey(KeyCode.D))
            {
                StartCoroutine(DoDash(1f));
            }
        }
    }

    private System.Collections.IEnumerator DoDash(float direction)
    {
        if (dashCooldownTimer > 0f) yield break; // Si aún no terminó el cooldown, no hacer dash

        isDashing = true;
        dashCooldownTimer = dashCooldown; // Reiniciamos el cooldown

        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            float moveX = direction * dashSpeed * Time.deltaTime;
            transform.Translate(moveX, 0f, 0f);

            // Limitar posición para no salir de la pantalla
            float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
            transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);

            elapsed += Time.deltaTime;
            yield return null; // Esperar al siguiente frame
        }

        isDashing = false;
    }
}

