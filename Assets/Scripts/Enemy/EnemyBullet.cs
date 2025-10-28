using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [Header("Atributos")]
    public int damage = 1;
    public float speed = 10f;
    public float lifeTime = 5f;

    private void Start()
    {
        // Se destruye automáticamente tras X segundos
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            return;

        // Movimiento constante hacia abajo
        transform.Translate(Vector3.down * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignorar enemigos, otras balas enemigas, balas del jugador o columnas
        if (other.CompareTag("Enemy") ||
            other.CompareTag("EnemyBullet") ||
            other.CompareTag("PlayerBullet") ||
            other.CompareTag("Columna"))
            return;

        // Si golpea al jugador, aplicar daño
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            // Por si el collider está en un hijo del prefab del jugador
            if (playerHealth == null)
                playerHealth = other.GetComponentInParent<PlayerHealth>();

            if (playerHealth != null)
                playerHealth.TakeDamage(damage);

            // Se destruye después de aplicar el daño
            Destroy(gameObject);
            return;
        }

        // Si toca cualquier otro objeto (paredes, bordes, etc.), se destruye
        Destroy(gameObject);
    }
}
