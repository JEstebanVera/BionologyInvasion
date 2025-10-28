using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Puntaje y UI")]
    public int score = 0;
    public TextMeshProUGUI scoreText;
    public GameObject levelMessagePanel;
    public TextMeshProUGUI levelMessageText;

    [Header("UI de Progreso")]
    public TextMeshProUGUI nextLevelText;

    [Header("Fondos por Nivel")]
    public GameObject fondoNivel1;
    public GameObject fondoNivel2;
    public GameObject fondoNivel3;

    [Header("Referencias")]
    public EnemySpawner spawner;

    [Header("Transición de Niveles")]
    public ParticleSystem levelTransitionParticles;
    public float transitionDuration = 5f;

    // --- CONTROL INTERNO ---
    private int currentLevel = 1;
    private int[] levelThresholds = { 100, 500, 1000 };
    public bool IsPaused { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateScoreUI();
        UpdateNextLevelUI();
        SetFondo(1);
        if (levelMessagePanel != null) levelMessagePanel.SetActive(false);
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI();
        UpdateNextLevelUI();

        // Verificar transición de nivel
        if (currentLevel <= levelThresholds.Length && score >= levelThresholds[currentLevel - 1])
        {
            StartCoroutine(LevelTransitionRoutine(currentLevel));
            currentLevel++;
        }

        if (spawner != null)
            spawner.OnScoreChanged(score);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "PUNTOS: " + score.ToString();
        }
    }

    private void UpdateNextLevelUI()
    {
        if (nextLevelText == null) return;

        int nextTarget = 0;
        if (currentLevel - 1 < levelThresholds.Length)
            nextTarget = levelThresholds[currentLevel - 1];

        if (score >= levelThresholds[levelThresholds.Length - 1])
            nextLevelText.text = "¡Máximo nivel alcanzado!";
        else
            nextLevelText.text = $"Puntos para siguiente nivel: {Mathf.Max(0, nextTarget - score)}";
    }

    private IEnumerator LevelTransitionRoutine(int level)
    {
        PauseGame(true);

        // Detener spawner
        if (spawner != null)
            spawner.enabled = false;

        // Mensaje
        string msg = "";
        switch (level)
        {
            case 1: msg = "¡Nivel 2! Más enemigos"; break;
            case 2: msg = "¡Nivel 3! Invasión intensa"; break;
            case 3: msg = "¡Victoria! Final del juego"; break;
        }

        ShowMessage(msg);

        if (levelTransitionParticles != null)
            levelTransitionParticles.Play();

        yield return new WaitForSecondsRealtime(transitionDuration);

        if (levelTransitionParticles != null)
            levelTransitionParticles.Stop();

        HideMessage();

        SetFondo(level + 1);

        UpdateNextLevelUI();

        // Reanudar juego
        PauseGame(false);

        // Reanudar spawn si no es el final
        if (level < levelThresholds.Length && spawner != null)
            spawner.enabled = true;
        else if (level >= levelThresholds.Length)
            StartCoroutine(EndGameRoutine());
    }


    private void PauseGame(bool state)
    {
        IsPaused = state;
        Time.timeScale = state ? 0f : 1f;
    }

    private IEnumerator EndGameRoutine()
    {
        PauseGame(true);
        ShowMessage("¡Victoria! Final del juego");
        yield return new WaitForSecondsRealtime(2.5f);
        PauseGame(false);
        GoToGameOver();
    }

    private void SetFondo(int level)
    {
        if (fondoNivel1 != null) fondoNivel1.SetActive(level == 1);
        if (fondoNivel2 != null) fondoNivel2.SetActive(level == 2);
        if (fondoNivel3 != null) fondoNivel3.SetActive(level == 3);
    }

    private void ShowMessage(string txt)
    {
        if (levelMessagePanel == null || levelMessageText == null) return;
        levelMessageText.text = txt;
        levelMessagePanel.SetActive(true);
    }

    private void HideMessage()
    {
        if (levelMessagePanel == null) return;
        levelMessagePanel.SetActive(false);
    }

    private void GoToGameOver()
    {
        Time.timeScale = 1f; // Asegurar que no se quede pausado
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("GameOver");
    }

    public void PlayerDefeated()
    {
        ShowMessage("¡Has sido derrotado!");
        Invoke(nameof(GameOver), 2.5f);
    }

    public float GetSpawnIntervalByScore()
    {
        if (score >= 500) return 3f;
        if (score >= 100) return 6f;
        return 9f;
    }

    public bool CanSpawnSpecialEnemy()
    {
        if (spawner == null) return true;
        int count = 0;
        foreach (var e in spawner.GetActiveEnemies())
        {
            if (e == null) continue;
            Enemy enemy = e.GetComponent<Enemy>();
            if (enemy != null && enemy.containsPickup)
                count++;
        }
        return count < 3;
    }

    public void GameOver()
    {
        GoToGameOver();
    }
}
