using System.Collections;
using Config;
using UnityEngine;


public class SpawnManager : MonoBehaviour
{
    [Header("Enemy Spawner")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject enemyContainer;
    [SerializeField] private float enemySpawnRate = 3f;
    
    [Header("PowerUp Spawner")]
    [Tooltip("PowerUp SpawnRate off min - max time range in seconds")]
    [SerializeField] private GameObject[] powerUpPrefabs;
    [SerializeField] private Vector2 powerUpSpawnRateRange = new Vector2(3f, 7f);
    [SerializeField] private GameObject powerUpContainer;

    private bool _spawnEnemies = false;
    private bool _spawnPowerUp = false;


    public void StartSpawners()
    {
        if (_spawnEnemies == false)
        {
            _spawnEnemies = true;
            StartCoroutine(SpawnEnemy(2.5f));    
        }

        if (_spawnPowerUp == false)
        {
            _spawnPowerUp = true;
            StartCoroutine(SpawnPowerUp(5f));    
        }
    }

    #region Spawner Logic
    private IEnumerator SpawnEnemy(float initialSpawnDelay)
    {
        yield return new WaitForSeconds(initialSpawnDelay);
        while (_spawnEnemies)
        {
            var spawnPos = new Vector3(Random.Range(ScreenBounds.ScreenLeft, ScreenBounds.ScreenRight),
                ScreenBounds.ScreenTop + 2f, 0);
                
            var newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            newEnemy.transform.parent = enemyContainer.transform;
            
            yield return new WaitForSeconds(enemySpawnRate);
        }
    }
    private IEnumerator SpawnPowerUp(float initialSpawnDelay)
    {
        yield return new WaitForSeconds(initialSpawnDelay);
        
        while (_spawnPowerUp)
        {
            var spawnPos = new Vector3(Random.Range(ScreenBounds.ScreenLeft, ScreenBounds.ScreenRight),
                ScreenBounds.ScreenTop + 2f, 0);

            var randomPowerUp = Random.Range(0, powerUpPrefabs.Length);
            var newPowerUp = Instantiate(powerUpPrefabs[randomPowerUp], spawnPos, Quaternion.identity);
            newPowerUp.transform.parent = powerUpContainer.transform;
            
            var nextSpawnTime = Random.Range(powerUpSpawnRateRange.x, powerUpSpawnRateRange.y);
            yield return new WaitForSeconds(nextSpawnTime);
        }
    }
    #endregion

    public void OnPlayerDeath()
    {
        _spawnEnemies = false;
        _spawnPowerUp = false;
    }
}
