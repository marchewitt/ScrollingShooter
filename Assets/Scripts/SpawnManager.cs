using System.Collections;
using System.Collections.Generic;
using Config;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private GameObject enemeyContainer;
    [SerializeField] private float spawnRate = 3f;

    private bool _spawnEnemies = true;
    
    void Start()
    {
        StartCoroutine(SpawnEnemy());
    }

    private IEnumerator SpawnEnemy()
    {
        while (_spawnEnemies)
        {
            var spawnPos = new Vector3(Random.Range(ScreenBounds.ScreenLeft, ScreenBounds.ScreenRight),
                  ScreenBounds.ScreenTop + 2f, 0);
                
            var newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            newEnemy.transform.parent = enemeyContainer.transform;
            
            yield return new WaitForSeconds(spawnRate);
        }
    }

    public void OnPlayerDeath()
    {
        _spawnEnemies = false;
    }
}
