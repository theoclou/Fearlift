using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    public GameObject carPrefab;       // Le prefab de voiture
    public Transform spawnPoint;       // Position de spawn
    public float spawnInterval = 2f;   // Temps entre chaque spawn

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnCar();
            timer = 0f;
        }
    }

    void SpawnCar()
    {
        Instantiate(carPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}
