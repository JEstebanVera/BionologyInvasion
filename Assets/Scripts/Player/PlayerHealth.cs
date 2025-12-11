using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Salud del jugador")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("HUD de vida")]
    public Image[] heartImages;
    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;

    [Header("Sonido de daño")]
    public AudioClip damageSound;      //asignar en el inspector
    private AudioSource audioSource;

    [Header("Efecto de Daño")]
    public GameObject damageEffectPrefab;  // prefab partículas
    private GameObject damageFXInstance;   // instancia runtime



    private CameraShake cameraShake;


    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHearts();

        cameraShake = Camera.main.GetComponent<CameraShake>(); // referencia a la variable de aqui y al script como tal

        audioSource = GetComponent<AudioSource>(); //  obtiene el AudioSource del jugador

        // --- Instancia FX de daño ---
        if (damageEffectPrefab != null)
        {
            damageFXInstance = Instantiate(damageEffectPrefab, transform.position, Quaternion.identity, transform);
            damageFXInstance.SetActive(false);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHearts();

        // reproducir sonido daño
        if (damageSound != null)
            audioSource.PlayOneShot(damageSound);

        // activar shake de cámara
        if (cameraShake != null)
            cameraShake.Shake();

        // --- Activa partículas de daño 1 segundo ---
        if (damageFXInstance != null)
            StartCoroutine(DamageEffectRoutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    private IEnumerator DamageEffectRoutine()
    {
        damageFXInstance.SetActive(true);
        yield return new WaitForSeconds(1f);
        damageFXInstance.SetActive(false);
    }

    private void UpdateHearts()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < currentHealth)
                heartImages[i].sprite = fullHeartSprite;
            else
                heartImages[i].sprite = emptyHeartSprite;
        }
    }

    private void Die()
    {
        GameManager.Instance.PlayerDefeated();
        gameObject.SetActive(false);
    }
}
