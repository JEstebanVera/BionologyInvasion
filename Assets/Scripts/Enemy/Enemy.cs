using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 10;
    public int currentHealth;

    [Header("Efectos")]
    public GameObject deathEffectPrefab;             // prefab de part�culas al morir 
    public GameObject specialPickupEffectPrefab;     // prefab de part�culas verdes si tiene pickup

    [Header("Sonidos")]
    public AudioClip deathSound;        // Sonido de muerte
    public AudioClip shootSound;        // Sonido al disparar
    private AudioSource audioSource;    // Fuente de audio propia

    [Header("Pickups")]
    public GameObject weaponPickupChargePrefab;   // prefab del pickup "Cargado" (weaponId = 2)
    public GameObject weaponPickupMultiplePrefab; // prefab del pickup "M�ltiple" (weaponId = 3)

    [Header("Ataque")]
    public GameObject enemyBulletPrefab;  // prefab de bala enemiga (tag: EnemyBullet)
    public Transform bulletSpawnPoint;

    [Header("Movimiento Horizontal")]
    public float horizontalSpeed = 2f;      // velocidad lateral

    [HideInInspector] public bool containsPickup = false;
    [HideInInspector] public int pickupWeaponId = 2; // 2 = cargado, 3 = m�ltiple

    private EnemySpawner spawner;
    private Animator animator;
    private Rigidbody rb;

    private int currentAreaIndex;
    private bool isActive = false;
    private bool movingRight = true;
    private bool isDead = false;

    private Coroutine shootingCoroutine;
    private BoxCollider currentArea;  // �rea actual

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;

        // Asegurar que el enemigo tenga un AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        if (containsPickup && specialPickupEffectPrefab != null)
        {
            GameObject fx = Instantiate(specialPickupEffectPrefab, transform.position, Quaternion.identity, transform);
            SafeDestroy.DestroyAfterSecondsSafe(this, fx, 10f);
        }
    }

    private void Update()
    {
        if (isDead) return;
        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;
        if (!isActive || currentArea == null) return;

        // Movimiento horizontal dentro del �rea
        Vector3 pos = transform.position;
        float dir = movingRight ? 1f : -1f;
        pos.x += dir * horizontalSpeed * Time.deltaTime;

        Vector3 min = currentArea.bounds.min;
        Vector3 max = currentArea.bounds.max;

        // Invertir direcci�n al llegar a los bordes
        if (pos.x >= max.x)
        {
            pos.x = max.x;
            movingRight = false;
        }
        else if (pos.x <= min.x)
        {
            pos.x = min.x;
            movingRight = true;
        }

        transform.position = pos;
    }

    public void Initialize(EnemySpawner spawnerRef)
    {
        spawner = spawnerRef;
        currentHealth = maxHealth;
    }

    public void AssignCurrentArea(int index)
    {
        currentAreaIndex = index;
        if (spawner != null && currentAreaIndex >= 0 && currentAreaIndex < spawner.spawnAreas.Length)
        {
            currentArea = spawner.spawnAreas[currentAreaIndex];
        }
    }

    public void SetActiveBehavior(bool active)
    {
        if (isDead) return;
        isActive = active;

        if (active)
        {
            StartMovementAndShooting();
        }
        else
        {
            // Detener s�lo las corrutinas internas relacionadas
            if (shootingCoroutine != null)
            {
                StopCoroutine(shootingCoroutine);
                shootingCoroutine = null;
            }
        }
    }

    private void StartMovementAndShooting()
    {
        if (isDead) return;

        if (shootingCoroutine != null)
            StopCoroutine(shootingCoroutine);

        shootingCoroutine = StartCoroutine(ShootingRoutine());
    }

    private IEnumerator ShootingRoutine()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(1f);
            if (!isActive) continue;
            Shoot();
        }
    }

    private void Shoot()
    {
        if (isDead) return;
        if (enemyBulletPrefab == null || bulletSpawnPoint == null) return;

        GameObject bullet = Instantiate(enemyBulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();
        if (rbBullet != null)
        {
            rbBullet.linearVelocity = Vector3.down * 6f;
        }

        SafeDestroy.DestroyAfterSecondsSafe(this, bullet, 3f);

        // Sonidos
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
    }

    public void MoveDown()
    {
        if (isDead) return;

        int nextArea = currentAreaIndex + 1;

        if (spawner == null || nextArea >= spawner.spawnAreas.Length)
        {
            // Notificamos al spawner (usa su m�todo de GameOver)
            if (spawner != null)
                spawner.OnEnemyReachedBottom(this);
            else
                GameManager.Instance.GameOver();

            return;
        }

        currentAreaIndex = nextArea;
        currentArea = spawner.spawnAreas[currentAreaIndex];

        // asegurarnos de cancelar corutinas vulnerables y ejecutar la bajada
        StopAllCoroutines();
        StartCoroutine(MoveDownRoutine());
    }

    private IEnumerator MoveDownRoutine()
    {
        if (isDead) yield break;

        Vector3 start = transform.position;
        Vector3 end = new Vector3(start.x, currentArea.bounds.center.y, start.z);
        float duration = 0.4f;
        float t = 0f;

        while (t < duration)
        {
            if (this == null) yield break;
            transform.position = Vector3.Lerp(start, end, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        if (this == null) yield break;

        transform.position = end;
        isActive = true;

        // Reiniciar disparo seguro
        if (shootingCoroutine != null) StopCoroutine(shootingCoroutine);
        shootingCoroutine = StartCoroutine(ShootingRoutine());
    }

    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        currentHealth -= dmg;
        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Detener todo comportamiento y corutinas
        StopAllCoroutines();
        isActive = false;

        // Deshabilitar collider para evitar nuevas colisiones
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // reproducir sonido de muerte
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // Instanciar efecto de muerte y destruirlo de forma segura
        if (deathEffectPrefab != null)
        {
            GameObject fx = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            SafeDestroy.DestroyAfterSecondsSafe(this, fx, 3f);
        }

        // Soltar pickup correcto si aplica
        if (containsPickup)
        {
            GameObject prefabToSpawn = null;
            if (pickupWeaponId == 2 && weaponPickupChargePrefab != null)
                prefabToSpawn = weaponPickupChargePrefab;
            else if (pickupWeaponId == 3 && weaponPickupMultiplePrefab != null)
                prefabToSpawn = weaponPickupMultiplePrefab;

            if (prefabToSpawn != null)
            {
                GameObject pickup = Instantiate(prefabToSpawn, transform.position, Quaternion.identity);
                var wp = pickup.GetComponent<WeaponPickup>();
                if (wp != null) wp.weaponId = pickupWeaponId;
                SafeDestroy.DestroyAfterSecondsSafe(this, pickup, 5f);
            }
        }

        // Sumar puntos (una sola vez)
        GameManager.Instance.AddScore(10);

        // Notificar spawner para remover de su lista
        if (spawner != null)
            spawner.RemoveEnemy(gameObject);

        // Destruir el enemigo de forma segura tras acabar el sonido (si hay) o inmediatamente
        float wait = (deathSound != null) ? deathSound.length : 0f;
        SafeDestroy.DestroyAfterSecondsSafe(this, gameObject, wait);
    }
}

