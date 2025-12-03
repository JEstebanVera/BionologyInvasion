using UnityEngine;
using System.Collections;

public class CarLaneController : MonoBehaviour
{
    [Header("Car Settings")]
    public GameObject[] carPrefabs;
    public float speed = 5f;

    [Header("Lane Points")]
    public Transform spawnPoint;
    public Transform endPoint;

    [Header("Respawn")]
    public float respawnDelay = 3f;

    private GameObject currentCar;
    private bool isShuttingDown = false;
    private Coroutine loopCoroutine;

    private void Start()
    {
        loopCoroutine = StartCoroutine(CarLoop());
    }

    private void OnDisable()
    {
        // cuando el objeto se desactiva, pausamos el loop y limpiamos con seguridad
        isShuttingDown = true;

        if (loopCoroutine != null)
            StopCoroutine(loopCoroutine);

        StopAllCoroutines();

        if (currentCar != null)
        {
            Destroy(currentCar);
            currentCar = null;
        }
    }

    private IEnumerator CarLoop()
    {
        while (!isShuttingDown)
        {
            GameObject prefab = GetRandomCar();
            if (prefab == null) yield break;

            // instanciamos con rotación X = -90
            currentCar = Instantiate(prefab, spawnPoint.position, Quaternion.Euler(-90f, 0f, 0f));

            yield return StartCoroutine(MoveCar());

            if (currentCar != null)
                Destroy(currentCar);

            currentCar = null;

            yield return new WaitForSeconds(respawnDelay);
        }
    }

    private IEnumerator MoveCar()
    {
        while (currentCar != null && !isShuttingDown)
        {
            currentCar.transform.position = Vector3.MoveTowards(
                currentCar.transform.position,
                endPoint.position,
                speed * Time.deltaTime
            );

            if (Vector3.Distance(currentCar.transform.position, endPoint.position) < 0.1f)
                break;

            yield return null;
        }
    }

    private GameObject GetRandomCar()
    {
        if (carPrefabs.Length == 0)
        {
            Debug.LogError("No hay prefabs asignados en carPrefabs.");
            return null;
        }

        return carPrefabs[Random.Range(0, carPrefabs.Length)];
    }

    // Fuerza la parada completa y destruye el coche actual (como tenías antes)
    public void ForceStop()
    {
        isShuttingDown = true;
        StopAllCoroutines();

        if (currentCar != null)
        {
            Destroy(currentCar);
            currentCar = null;
        }
    }

    // PAUSA no destructiva: detiene el loop pero NO destruye el coche actual (más segura)
    public void PauseSpawn()
    {
        if (isShuttingDown) return;
        isShuttingDown = true;

        if (loopCoroutine != null)
            StopCoroutine(loopCoroutine);

        loopCoroutine = null;
        // no destruimos currentCar aquí; lo dejamos quieto
    }

    // Reanuda el spawn si fue pausado
    public void ResumeSpawn()
    {
        if (!isShuttingDown) return;
        isShuttingDown = false;

        // Si ya hay una coroutine, no arrancamos otra
        if (loopCoroutine == null)
            loopCoroutine = StartCoroutine(CarLoop());
    }

    public void StopPermanently()
    {
        isShuttingDown = true;

        if (loopCoroutine != null)
            StopCoroutine(loopCoroutine);

        loopCoroutine = null;

        if (currentCar != null)
        {
            Destroy(currentCar);
            currentCar = null;
        }
    }

}
