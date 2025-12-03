using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public int weaponId;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerShooting player = other.GetComponent<PlayerShooting>();
        if (player == null) return;

        player.ActivateSpecialWeapon(weaponId);

        Destroy(gameObject);
    }
}
