using UnityEngine;

public class AutoDestroyParticles : MonoBehaviour
{
    [Header("Tiempo fijo de vida (0 = usar duración real)")]
    public float fixedLifetime = 3f;

    private void OnEnable()
    {
        // Si hay tiempo fijo, usarlo inmediatamente
        if (fixedLifetime > 0f)
        {
            Destroy(gameObject, fixedLifetime);
            return;
        }

        // Si no, intentar obtener duración real del sistema
        float maxDuration = 0f;
        ParticleSystem[] systems = GetComponentsInChildren<ParticleSystem>();

        foreach (var ps in systems)
        {
            // duracion + lifetime máximo
            float dur = ps.main.duration + ps.main.startLifetime.constantMax;
            if (dur > maxDuration) maxDuration = dur;
        }

        // Seguridad: si no encontró nada, destruir después de 2s
        if (maxDuration <= 0f)
            maxDuration = 2f;

        Destroy(gameObject, maxDuration);
    }
}
