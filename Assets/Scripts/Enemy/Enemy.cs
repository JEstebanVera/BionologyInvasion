using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 10;
    public int currentHealth;

    
    [Header("Efectos")]
    public GameObject deathEffectPrefab;             // prefab de partículas al morir (puede contener varios sistemas)
    public GameObject specialPickupEffectPrefab;     // prefab de partículas verdes si tiene pickup

    [Header("Pickups")]
    public GameObject weaponPickupChargePrefab;   // prefab del pickup "Cargado" (weaponId = 2)
    public GameObject weaponPickupMultiplePrefab; // prefab del pickup "Múltiple" (weaponId = 3)

    [Header("Ataque")]
    public GameObject enemyBulletPrefab;  // prefab de bala enemiga (tag: EnemyBullet)
    public Transform bulletSpawnPoint;

    [Header("Movimiento Horizontal")]
    public float horizontalSpeed = 2f;      // velocidad lateral
    public float moveCheckInterval = 0.1f;  // control de borde

    [HideInInspector] public bool containsPickup = false;
    [HideInInspector] public int pickupWeaponId = 2; // 2 = cargado, 3 = múltiple

    private EnemySpawner spawner;
    private Animator animator;
    private Rigidbody rb;

    private int currentAreaIndex;
    private bool isActive = false;
    private bool movingRight = true;

    private Coroutine shootingCoroutine;
    private BoxCollider currentArea;  // área actual


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
    }

    private void Start()
    {
       
        if (containsPickup && specialPickupEffectPrefab != null)
        {
            GameObject fx = Instantiate(specialPickupEffectPrefab, transform.position, Quaternion.identity, transform);
            // Destruir el efecto tras unos segundos (opcional)
            Destroy(fx, 10f);
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            return;

        if (!isActive || currentArea == null) return;

        // Movimiento horizontal dentro del área
        Vector3 pos = transform.position;
        float dir = movingRight ? 1f : -1f;
        pos.x += dir * horizontalSpeed * Time.deltaTime;

        Vector3 min = currentArea.bounds.min;
        Vector3 max = currentArea.bounds.max;

        // Invertir dirección al llegar a los bordes
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
        isActive = active;

        if (active)
        {
            StartMovementAndShooting();
        }
        else
        {
            StopAllCoroutines();
        }
    }

    private void StartMovementAndShooting()
    {
        if (shootingCoroutine != null)
            StopCoroutine(shootingCoroutine);

        shootingCoroutine = StartCoroutine(ShootingRoutine());
    }

    private IEnumerator ShootingRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (!isActive) continue;
            Shoot();
        }
    }

    private void Shoot()
    {
        if (enemyBulletPrefab == null || bulletSpawnPoint == null) return;

        GameObject bullet = Instantiate(enemyBulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();
        if (rbBullet != null)
        {
            rbBullet.linearVelocity = Vector3.down * 6f;
        }

        Destroy(bullet, 3f);
    }

    public void MoveDown()
    {
        int nextArea = currentAreaIndex + 1;

        if (spawner == null || nextArea >= spawner.spawnAreas.Length)
        {
            GameManager.Instance.GameOver();
            return;
        }

        currentAreaIndex = nextArea;
        currentArea = spawner.spawnAreas[currentAreaIndex];

        StopAllCoroutines();
        StartCoroutine(MoveDownRoutine());
    }

    private IEnumerator MoveDownRoutine()
    {
        Vector3 start = transform.position;
        Vector3 end = new Vector3(start.x, currentArea.bounds.center.y, start.z);
        float duration = 0.4f;
        float t = 0f;

        while (t < duration)
        {
            transform.position = Vector3.Lerp(start, end, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        transform.position = end;
        isActive = true;
        shootingCoroutine = StartCoroutine(ShootingRoutine());
    }

    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        
        if (deathEffectPrefab != null)
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);

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
                Destroy(pickup, 5f); // desaparece si no se recoge
            }
        }

        GameManager.Instance.AddScore(10);

        if (spawner != null)
            spawner.RemoveEnemy(gameObject);

        Destroy(gameObject);
    }
}

