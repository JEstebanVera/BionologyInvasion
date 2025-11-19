using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private float moveDownInterval = 10f; // cada cuánto bajan
    private float moveDownTimer = 0f;

    [Header("Referencias")]
    public GameObject enemyPrefab;
    public BoxCollider[] spawnAreas;   // Cada área representa una "fila"
    public float spawnHeightOffset = 8f;
    public float animationDuration = 1f; // duración del vuelo de entrada
    public GameManager gameManager;

    [Header("Control de Spawn")]
    public int maxEnemiesPerWave = 5;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool spawning = true;

    [Header("Probabilidad de Enemigos Especiales")]
    [Range(0f, 1f)]
    public float specialEnemyChance = 0.15f;

    private PlayerShooting player;

    private void Start()
    {
        StartCoroutine(SpawnRoutine());

        player = FindObjectOfType<PlayerShooting>();

    }

    private void Update()
    {
        // Limpia enemigos destruidos (remueve referencias null)
        activeEnemies.RemoveAll(e => e == null);

        moveDownTimer += Time.deltaTime;

        if (moveDownTimer >= moveDownInterval)
        {
            MoveAllEnemiesDown();
            moveDownTimer = 0f;
        }
    }

    private void MoveAllEnemiesDown()
    {
        if (activeEnemies.Count == 0) return;

        // Hacemos una copia de la lista para evitar problemas si se modifica durante iteración
        var copy = new List<GameObject>(activeEnemies);
        foreach (GameObject enemyObj in copy)
        {
            if (enemyObj == null) continue;

            Enemy enemy = enemyObj.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.MoveDown();
            }
        }
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(1f); // pequeña pausa inicial

        while (spawning)
        {
            // Limpieza preventiva
            activeEnemies.RemoveAll(e => e == null);

            if (activeEnemies.Count < maxEnemiesPerWave)
            {
                SpawnEnemy();
            }

            float interval = gameManager != null
                ? gameManager.GetSpawnIntervalByScore()
                : 8f;

            yield return new WaitForSeconds(interval);
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null || spawnAreas.Length == 0) return;

        // Actual: spawnear siempre desde el área superior (index 0)
        BoxCollider area = spawnAreas[0];

        // Posición final dentro del área
        Vector3 finalPosition = GetRandomPointInside(area);

        // Posición inicial (fuera de pantalla)
        Vector3 spawnPos = finalPosition + Vector3.up * spawnHeightOffset;

        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        activeEnemies.Add(newEnemy);

        // Configurar enemigo
        Enemy enemyScript = newEnemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.Initialize(this);
            enemyScript.AssignCurrentArea(FindAreaIndex(area));
            enemyScript.SetActiveBehavior(false);
        }

        // Determinar si este enemigo tendrá pickup especial
        if (ShouldSpawnSpecialEnemy())
        {
            if (enemyScript != null)
            {
                enemyScript.containsPickup = true;
                enemyScript.pickupWeaponId = Random.value < 0.5f ? 2 : 3; // elegir 2 o 3
            }
        }

        // Animación de vuelo o trigger de animador
        Animator anim = newEnemy.GetComponent<Animator>();
        if (anim != null && anim.HasState(0, Animator.StringToHash("SpawnIn")))
        {
            anim.SetTrigger("SpawnIn");
        }
        else
        {
            StartCoroutine(FlyInRoutine(newEnemy, finalPosition));
        }

        // Activar comportamiento tras animación
        if (enemyScript != null)
            StartCoroutine(ActivateAfterDelay(enemyScript, finalPosition));
    }

    private IEnumerator FlyInRoutine(GameObject enemy, Vector3 targetPos)
    {
        if (enemy == null) yield break;

        Vector3 start = enemy.transform.position;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            if (enemy == null) yield break; // Si fue destruido, salir seguro
            enemy.transform.position = Vector3.Lerp(start, targetPos, elapsed / animationDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (enemy != null)
            enemy.transform.position = targetPos;
    }

    private IEnumerator ActivateAfterDelay(Enemy enemyScript, Vector3 finalPosition)
    {
        yield return new WaitForSeconds(animationDuration);

        if (enemyScript == null || enemyScript.gameObject == null) yield break;

        // seguridad extra
        if (enemyScript != null && enemyScript.gameObject != null)
        {
            enemyScript.transform.position = finalPosition;
            enemyScript.SetActiveBehavior(true);
        }
    }

    private Vector3 GetRandomPointInside(BoxCollider col)
    {
        Vector3 min = col.bounds.min;
        Vector3 max = col.bounds.max;

        float x = Random.Range(min.x, max.x);
        float y = Random.Range(min.y, max.y);
        float z = Random.Range(min.z, max.z);

        return new Vector3(x, y, z);
    }

    private bool ShouldSpawnSpecialEnemy()
    {
        if (player == null) return false;

        // Si el jugador ya tiene las dos armas, no puede aparecer ningún pickup
        if (player.HasAllWeapons())
            return false;

        // Probabilidad base
        if (Random.value > specialEnemyChance)
            return false;

        // Hay probabilidad de enemigo especial, pero elegir arma válida:
        return true;
    }


    private int FindAreaIndex(BoxCollider area)
    {
        for (int i = 0; i < spawnAreas.Length; i++)
        {
            if (spawnAreas[i] == area)
                return i;
        }
        return 0;
    }

    public void RemoveEnemy(GameObject enemy)
    {
        if (enemy == null) return;
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }
    }

    public void OnScoreChanged(int score)
    {

        if (score >= 2000) maxEnemiesPerWave = 10;
        else if (score >= 500) maxEnemiesPerWave = 7;
        else maxEnemiesPerWave = 5;
    }

    public List<GameObject> GetActiveEnemies()
    {

        return new List<GameObject>(activeEnemies);
    }

    public void OnEnemyReachedBottom(Enemy enemy)
    {
        Debug.Log("Un enemigo llegó al área final. GAME OVER.");

        spawning = false; // detiene la rutina de spawn

        // Limpia el enemigo que causó el Game Over
        if (enemy != null)
        {
            RemoveEnemy(enemy.gameObject);
            SafeDestroy.DestroyEndOfFrame(this, enemy.gameObject);
        }

        // Detén todos los enemigos activos actuales (y marca inactivos)
        foreach (var e in new List<GameObject>(activeEnemies))
        {
            if (e == null) continue;
            Enemy en = e.GetComponent<Enemy>();
            if (en != null) en.SetActiveBehavior(false);
        }

        // Llama al GameManager
        if (gameManager != null)
        {
            gameManager.PlayerDefeated();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameOver");
        }
    }
}
