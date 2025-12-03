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
        isShuttingDown = true;

        // Detener coroutines inmediatamente
        if (loopCoroutine != null)
            StopCoroutine(loopCoroutine);

        StopAllCoroutines();

        // Destruir coche solo si aún existe
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
}
