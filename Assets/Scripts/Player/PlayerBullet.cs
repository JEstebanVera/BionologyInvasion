using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [Header("Stats")]
    public int damage = 1;
    public float lifeTime = 3f;

    private void Start()
    {
        // Destruir la bala automáticamente después de cierto tiempo
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignorar colisiones con el jugador, otras balas del jugador o columnas
        if (other.CompareTag("Player") || other.CompareTag("PlayerBullet") || other.CompareTag("Columna") || other.CompareTag("EnemyBullet") || other.CompareTag("PickUpWeapon"))
            return;

        // Si impacta un enemigo, aplicar daño
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            Destroy(gameObject);
            return;
        }

        // Si toca cualquier otra cosa (pared, límite, etc.)
        Destroy(gameObject);
    }
}
