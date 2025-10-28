using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Salud del jugador")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("HUD de vida")]
    public Image[] heartImages;        // Las 5 imágenes asignadas en el inspector
    public Sprite fullHeartSprite;     // Sprite lleno
    public Sprite emptyHeartSprite;    // Sprite vacío

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHearts();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHearts();

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
        // Notificar al GameManager
        GameManager.Instance.PlayerDefeated();
        // Desactivar jugador o destruirlo
        gameObject.SetActive(false);
    }
}
