using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [Header("Slots de armas en el HUD")]
    public Image basicWeaponSlot;
    public Image chargedWeaponSlot;
    public Image multiWeaponSlot;

    [Header("Highlights de selección")]
    public GameObject basicHighlight;
    public GameObject chargedHighlight;
    public GameObject multiHighlight;

    [Header("Sprites de las armas")]
    public Sprite basicSprite;
    public Sprite chargedLockedSprite;
    public Sprite chargedActiveSprite;
    public Sprite multiLockedSprite;
    public Sprite multiActiveSprite;

    private void Start()
    {
        // Config inicial
        if (basicWeaponSlot != null)
            basicWeaponSlot.sprite = basicSprite;

        if (chargedWeaponSlot != null)
            chargedWeaponSlot.sprite = chargedLockedSprite;

        if (multiWeaponSlot != null)
            multiWeaponSlot.sprite = multiLockedSprite;

        // Apagamos todos los highlights al inicio
        SetHighlight(1); // por defecto selecciona el básico
    }

    public void UnlockWeaponHUD(int weaponId)
    {
        switch (weaponId)
        {
            case 2:
                if (chargedWeaponSlot != null)
                    chargedWeaponSlot.sprite = chargedActiveSprite;
                break;

            case 3:
                if (multiWeaponSlot != null)
                    multiWeaponSlot.sprite = multiActiveSprite;
                break;
        }
    }

    // Activa el highlight del arma seleccionada
    public void SetHighlight(int weaponId)
    {
        if (basicHighlight != null) basicHighlight.SetActive(false);
        if (chargedHighlight != null) chargedHighlight.SetActive(false);
        if (multiHighlight != null) multiHighlight.SetActive(false);

        switch (weaponId)
        {
            case 1:
                if (basicHighlight != null) basicHighlight.SetActive(true);
                break;
            case 2:
                if (chargedHighlight != null) chargedHighlight.SetActive(true);
                break;
            case 3:
                if (multiHighlight != null) multiHighlight.SetActive(true);
                break;
        }
    }
}
