using UnityEngine;
using UnityEngine.UI;

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

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHearts();

        audioSource = GetComponent<AudioSource>(); //  obtiene el AudioSource del jugador
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHearts();

        // reproducir sonido daño
        if (damageSound != null)
            audioSource.PlayOneShot(damageSound);

        if (currentHealth <= 0)
        {
            Die();
        }
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
