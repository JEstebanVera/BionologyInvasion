using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 10;
    public int currentHealth;

    [Header("Efectos")]
    public GameObject deathEffectPrefab;
    public GameObject specialPickupEffectPrefab;

    [Header("Sonidos")]
    public AudioClip deathSound;
    public AudioClip shootSound;
    public AudioClip hitSound;     //  Nuevo: sonido al recibir daño
    private AudioSource audioSource;

    [Header("Pickups")]
    public GameObject weaponPickupChargePrefab;
    public GameObject weaponPickupMultiplePrefab;

    [Header("Ataque")]
    public GameObject enemyBulletPrefab;
    public Transform bulletSpawnPoint;

    [Header("Movimiento Horizontal")]
    public float horizontalSpeed = 2f;

    [HideInInspector] public bool containsPickup = false;
    [HideInInspector] public int pickupWeaponId = 2;

    private EnemySpawner spawner;
    private Animator animator;
    private Rigidbody rb;

    private int currentAreaIndex;
    private bool isActive = false;
    private bool movingRight = true;
    private bool isDead = false;

    private Coroutine shootingCoroutine;
    private BoxCollider currentArea;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;

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

        Vector3 pos = transform.position;
        float dir = movingRight ? 1f : -1f;
        pos.x += dir * horizontalSpeed * Time.deltaTime;

        Vector3 min = currentArea.bounds.min;
        Vector3 max = currentArea.bounds.max;

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
            rbBullet.linearVelocity = Vector3.down * 6f;

        SafeDestroy.DestroyAfterSecondsSafe(this, bullet, 3f);

        if (shootSound != null)
            audioSource.PlayOneShot(shootSound);
    }

    public void MoveDown()
    {
        if (isDead) return;

        int nextArea = currentAreaIndex + 1;

        if (spawner == null || nextArea >= spawner.spawnAreas.Length)
        {
            if (spawner != null)
                spawner.OnEnemyReachedBottom(this);
            else
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

        transform.position = end;
        isActive = true;

        if (shootingCoroutine != null) StopCoroutine(shootingCoroutine);
        shootingCoroutine = StartCoroutine(ShootingRoutine());
    }


    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        // sonido de impacto
        if (hitSound != null)
            audioSource.PlayOneShot(hitSound);

        currentHealth -= dmg;
        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        StopAllCoroutines();
        isActive = false;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        if (deathSound != null)
            audioSource.PlayOneShot(deathSound);

        if (deathEffectPrefab != null)
        {
            GameObject fx = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            SafeDestroy.DestroyAfterSecondsSafe(this, fx, 3f);
        }

        if (containsPickup)
        {
            GameObject prefabToSpawn = null;

            if (pickupWeaponId == 2) prefabToSpawn = weaponPickupChargePrefab;
            else if (pickupWeaponId == 3) prefabToSpawn = weaponPickupMultiplePrefab;

            if (prefabToSpawn != null)
            {
                GameObject pickup = Instantiate(prefabToSpawn, transform.position, Quaternion.identity);
                var wp = pickup.GetComponent<WeaponPickup>();
                if (wp != null) wp.weaponId = pickupWeaponId;
                SafeDestroy.DestroyAfterSecondsSafe(this, pickup, 5f);
            }
        }

        GameManager.Instance.AddScore(10);

        if (spawner != null)
            spawner.RemoveEnemy(gameObject);

        float wait = (deathSound != null) ? deathSound.length : 0f;
        SafeDestroy.DestroyAfterSecondsSafe(this, gameObject, wait);
    }
}
