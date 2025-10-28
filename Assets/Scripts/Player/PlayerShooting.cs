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
    public Transform chargeOrb;         // El orb que crece en el cañón
    public Vector3 maxOrbScale = new Vector3(1f, 1f, 1f); // Tamaño maximo del orb
    public Color chargedColor = Color.red; // Color cuando se completa la carga
    private Vector3 initialOrbScale;
    private Renderer orbRenderer;
    private Color baseOrbColor;

    public HUDManager hudManager;
    private enum WeaponType { Basic, Charged, Multi }
    private WeaponType currentWeapon = WeaponType.Basic;

    private HashSet<WeaponType> unlockedWeapons = new HashSet<WeaponType>() { WeaponType.Basic };

    private float chargeTimer = 0f;
    private bool isCharging = false;

    private void Start()
    {
        if (chargeOrb != null)
        {
            initialOrbScale = Vector3.zero;
            orbRenderer = chargeOrb.GetComponent<Renderer>();
            baseOrbColor = orbRenderer.material.color;
            chargeOrb.localScale = initialOrbScale; // oculto al inicio
        }
    }

    private void Update()
    {
        HandleWeaponSwitch();
        HandleShooting();
        UpdateChargeOrb();
    }

    private void HandleWeaponSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && unlockedWeapons.Contains(WeaponType.Basic))
        {
            currentWeapon = WeaponType.Basic;
            hudManager.SetHighlight(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && unlockedWeapons.Contains(WeaponType.Charged))
        {
            currentWeapon = WeaponType.Charged;
            hudManager.SetHighlight(2);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) && unlockedWeapons.Contains(WeaponType.Multi))
        {
            currentWeapon = WeaponType.Multi;
            hudManager.SetHighlight(3);
        }
    }


    private void HandleShooting()
    {
        switch (currentWeapon)
        {
            case WeaponType.Basic:
                if (Input.GetMouseButtonDown(0))
                    ShootBullet(basicBulletPrefab, firePoint, Vector3.up);
                break;

            case WeaponType.Charged:
                if (Input.GetMouseButton(0)) // mantener click izquierdo
                {
                    chargeTimer += Time.deltaTime;

                    if (chargeTimer >= chargeTime)
                    {
                        // Disparo cargado
                        ShootBullet(chargedBulletPrefab, firePoint, Vector3.up);

                        // Reset para permitir un nuevo ciclo sin soltar el click
                        chargeTimer = 0f;
                        isCharging = false;
                        chargeOrb.localScale = Vector3.zero;
                        orbRenderer.material.color = baseOrbColor;
                    }
                }

                if (Input.GetMouseButtonUp(0)) // reset al soltar antes de cargar
                {
                    chargeTimer = 0f;
                    isCharging = false;

                    // Reset visual
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
                }
                break;
        }
    }

    private void UpdateChargeOrb()
    {
        if (currentWeapon == WeaponType.Charged && Input.GetMouseButton(0))
        {
            float t = Mathf.Clamp01(chargeTimer / chargeTime);
            chargeOrb.localScale = Vector3.Lerp(Vector3.zero, maxOrbScale, t);

            if (t >= 1f)
            {
                orbRenderer.material.color = chargedColor; // cambia de color al cargarse
            }
        }
        else
        {
            // Siempre ocultar el orb cuando no estamos cargando
            chargeOrb.localScale = Vector3.zero;
            orbRenderer.material.color = baseOrbColor;
        }
    }

    private void ShootBullet(GameObject bulletPrefab, Transform spawnPoint, Vector3 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, spawnPoint.position, Quaternion.identity);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = direction * bulletSpeed;
        Destroy(bullet, 2f);
    }

    public void UnlockWeapon(int weaponId)
    {
        if (weaponId == 2)
        {
            unlockedWeapons.Add(WeaponType.Charged);
            Debug.Log("Arma Cargada desbloqueada!");
            hudManager.UnlockWeaponHUD(2);
        }
        if (weaponId == 3)
        {
            unlockedWeapons.Add(WeaponType.Multi);
            Debug.Log("Arma Múltiple desbloqueada!");
            hudManager.UnlockWeaponHUD(3);
        }
    }

    // Devuelve si el jugador ya desbloqueó la arma con id (2 o 3)
    public bool HasUnlockedWeapon(int weaponId)
    {
        if (weaponId == 2) return unlockedWeapons.Contains(WeaponType.Charged);
        if (weaponId == 3) return unlockedWeapons.Contains(WeaponType.Multi);
        return false;
    }

}

