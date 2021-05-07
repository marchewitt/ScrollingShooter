using System.Collections;
using System.Collections.Generic;
using Config;
using UnityEngine;
using UnityEngine.Serialization;

public class SpawnManager : MonoBehaviour
{
    [Header("Enemy Spawner")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject enemeyContainer;
    [SerializeField] private float enemySpawnRate = 3f;
    
    [Header("PowerUp Spawner")]
    [Tooltip("PowerUp SpawnRate off min - max time range in seconds")]
    [SerializeField] private GameObject powerUpPrefab;
    [SerializeField] private Vector2 powerUpSpawnRateRange = new Vector2(3f, 7f);
    [SerializeField] private GameObject powerupContainer;

    private bool _spawnEnemies = true;
    private bool _spawnPowerUp = true;

    void Start()
    {
        StartCoroutine(SpawnEnemy());
        StartCoroutine(SpawnPowerUp());
    }

    private IEnumerator SpawnEnemy()
    {
        while (_spawnEnemies)
        {
            var spawnPos = new Vector3(Random.Range(ScreenBounds.ScreenLeft, ScreenBounds.ScreenRight),
                ScreenBounds.ScreenTop + 2f, 0);
                
            var newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            newEnemy.transform.parent = enemeyContainer.transform;
            
            yield return new WaitForSeconds(enemySpawnRate);
        }
    }
    private IEnumerator SpawnPowerUp()
    {
        //First Wait
        yield return new WaitForSeconds(Random.Range(powerUpSpawnRateRange.x, powerUpSpawnRateRange.y)); 
        
        while (_spawnPowerUp)
        {
            var spawnPos = new Vector3(Random.Range(ScreenBounds.ScreenLeft, ScreenBounds.ScreenRight),
                ScreenBounds.ScreenTop + 2f, 0);
                
            var newPowerUp = Instantiate(powerUpPrefab, spawnPos, Quaternion.identity);
            newPowerUp.transform.parent = powerupContainer.transform;
            
            var nextSpawnTime = Random.Range(powerUpSpawnRateRange.x, powerUpSpawnRateRange.y);
            yield return new WaitForSeconds(nextSpawnTime);
        }
    }

    public void OnPlayerDeath()
    {
        _spawnEnemies = false;
        _spawnPowerUp = false;
    }
}
