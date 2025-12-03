using UnityEngine;
using System.Collections.Generic;

public class PlayerShooting : MonoBehaviour
{
    [Header("Prefabs de Balas")]
    public GameObject basicBulletPrefab;
    public GameObject chargedBulletPrefab;
    public GameObject multiBulletPrefab;

    [Header("Puntos de Disparo")]
    public Transform firePoint;
    public Transform leftPoint;
    public Transform rightPoint;

    [Header("Stats Bala")]
    public float bulletSpeed = 15f;
    public float chargeTime = 2f;

    [Header("Visual de Carga")]
    public Transform chargeOrb;
    public Vector3 maxOrbScale = new Vector3(1f, 1f, 1f);
    public Color chargedColor = Color.red;
    private Vector3 initialOrbScale;
    private Renderer orbRenderer;
    private Color baseOrbColor;

    [Header("Sonidos de Disparo")]
    public AudioClip basicShootSound;
    public AudioClip chargedShootSound;
    public AudioClip multiShootSound;
    private AudioSource audioSource;

    [Header("Armas Temporales")]
    public bool specialWeaponActive = false;
    private float specialWeaponTimer = 0f;
    public float specialWeaponDuration = 30f;

    public Sprite defaultWeaponSprite;
    public HUDManager hudManager;

    private enum WeaponType { Basic, Charged, Multi }
    private WeaponType currentWeapon = WeaponType.Basic;

    private float chargeTimer = 0f;

    private void Start()
    {
        initialOrbScale = Vector3.zero;

        if (chargeOrb != null)
        {
            orbRenderer = chargeOrb.GetComponent<Renderer>();
            baseOrbColor = orbRenderer.material.color;
            chargeOrb.localScale = initialOrbScale;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        hudManager.SetHighlight(1); // arma básica al iniciar
    }

    private void Update()
    {
        HandleWeaponTimer();
        HandleShooting();
        UpdateChargeOrb();
    }

    // --------------------------
    //     SISTEMA TEMPORAL
    // --------------------------
    public void ActivateSpecialWeapon(int weaponId)
    {
        specialWeaponActive = true;
        specialWeaponTimer = specialWeaponDuration;

        if (weaponId == 2)
        {
            currentWeapon = WeaponType.Charged;
            hudManager.SetHighlight(2);
        }
        if (weaponId == 3)
        {
            currentWeapon = WeaponType.Multi;
            hudManager.SetHighlight(3);
        }

        hudManager.UnlockWeaponHUD(weaponId);
        hudManager.SetHighlight(weaponId);

    }

    private void HandleWeaponTimer()
    {
        if (!specialWeaponActive) return;

        specialWeaponTimer -= Time.deltaTime;

        if (specialWeaponTimer <= 0f)
        {
            specialWeaponActive = false;
            currentWeapon = WeaponType.Basic;
            hudManager.LockAllWeaponsHUD();
            hudManager.SetHighlight(1);
        }
    }

    // --------------------------
    //       DISPARO NORMAL
    // --------------------------
    private void HandleShooting()
    {
        switch (currentWeapon)
        {
            case WeaponType.Basic:
                if (Input.GetMouseButtonDown(0))
                {
                    ShootBullet(basicBulletPrefab, firePoint, Vector3.up);
                    PlaySound(basicShootSound);
                }
                break;

            case WeaponType.Charged:
                if (Input.GetMouseButton(0))
                {
                    chargeTimer += Time.deltaTime;

                    if (chargeTimer >= chargeTime)
                    {
                        ShootBullet(chargedBulletPrefab, firePoint, Vector3.up);
                        PlaySound(chargedShootSound);

                        chargeTimer = 0f;
                        chargeOrb.localScale = Vector3.zero;
                        orbRenderer.material.color = baseOrbColor;
                    }
                }

                if (Input.GetMouseButtonUp(0))
                {
                    chargeTimer = 0f;
                    chargeOrb.localScale = Vector3.zero;
                    orbRenderer.material.color = baseOrbColor;
                }
                break;

            case WeaponType.Multi:
                if (Input.GetMouseButtonDown(0))
                {
                    ShootBullet(basicBulletPrefab, firePoint, Vector3.up);
                    ShootBullet(basicBulletPrefab, leftPoint, new Vector3(-0.5f, 1f, 0f).normalized);
                    ShootBullet(basicBulletPrefab, rightPoint, new Vector3(0.5f, 1f, 0f).normalized);
                    PlaySound(multiShootSound);
                }
                break;
        }
    }

    private void UpdateChargeOrb()
    {
        if (currentWeapon != WeaponType.Charged || !Input.GetMouseButton(0))
        {
            chargeOrb.localScale = Vector3.zero;
            return;
        }

        float t = Mathf.Clamp01(chargeTimer / chargeTime);
        chargeOrb.localScale = Vector3.Lerp(Vector3.zero, maxOrbScale, t);

        if (t >= 1f)
            orbRenderer.material.color = chargedColor;
    }

    private void ShootBullet(GameObject bulletPrefab, Transform spawnPoint, Vector3 direction)
    {
        if (bulletPrefab == null || spawnPoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, spawnPoint.position, Quaternion.identity);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = direction * bulletSpeed;

        SafeDestroy.DestroyAfterSecondsSafe(this, bullet, 2f);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }

    public bool HasAllWeapons()
    {
        return false;
    }
}
