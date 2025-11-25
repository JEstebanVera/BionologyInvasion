using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DashUI : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite canDashSprite;   // Sprite cuando puede hacer dash
    public Sprite noDashSprite;    // Sprite cuando NO puede hacer dash

    [Header("UI")]
    public Image targetImage;      // La imagen UI a modificar

    private bool isDashingCooldown = false;

    private void Update()
    {
        // Detectar barra espaciadora
        if (Input.GetKeyDown(KeyCode.Space) && !isDashingCooldown)
        {
            StartCoroutine(DashCooldownRoutine());
        }
    }

    private IEnumerator DashCooldownRoutine()
    {
        isDashingCooldown = true;

        // Cambiar a sprite "NoDash"
        if (targetImage != null && noDashSprite != null)
            targetImage.sprite = noDashSprite;

        // Esperar 1 segundo
        yield return new WaitForSeconds(1f);

        // Volver a "CanDash"
        if (targetImage != null && canDashSprite != null)
            targetImage.sprite = canDashSprite;

        isDashingCooldown = false;
    }
}
