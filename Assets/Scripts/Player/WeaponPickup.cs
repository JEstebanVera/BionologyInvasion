using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public int weaponId; // 2 = cargado, 3 = múltiple
    public float fallSpeed = 1f; // velocidad en la que cae

    [Header("Sonido")]
    public AudioClip pickupSound;

    private void Start()
    {
        // Destruir de forma segura después de 5s
        SafeDestroy.DestroyAfterSecondsSafe(this, gameObject, 5f);
    }

    private void Update()
    {
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var ps = other.GetComponent<PlayerShooting>();
            if (ps != null)
                ps.UnlockWeapon(weaponId);

            // Reproducir sonido de pickup (usamos PlayClipAtPoint para no depender del AudioSource local)
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            // destruir de forma segura al final del frame
            SafeDestroy.DestroyEndOfFrame(this, gameObject);
        }
    }
}
