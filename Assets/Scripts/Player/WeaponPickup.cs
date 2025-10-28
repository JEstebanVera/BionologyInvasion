using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public int weaponId; // 2 = cargado, 3 = múltiple
    public float fallSpeed = 1f; // velocidad en la que cae

    private void Start()
    {
        Destroy(gameObject, 5f); // destruir si no se recoge en 5 segundos
    }

    private void Update()
    {
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerShooting>().UnlockWeapon(weaponId);
            Destroy(gameObject);
        }
    }
}
