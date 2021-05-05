using System.Collections;
using System.Collections.Generic;
using Config;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private GameObject enemeyContainer;
    [SerializeField] private float spawnRate = 3f;
    
    
    void Start()
    {
        StartCoroutine(SpawnEnemy());
    }

    private IEnumerator SpawnEnemy()
    {
        while (true)
        {
            var spawnPos = new Vector3(Random.Range(ScreenBounds.ScreenLeft, ScreenBounds.ScreenRight),
                  ScreenBounds.ScreenTop + 2f, 0);
                
            var newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            newEnemy.transform.parent = enemeyContainer.transform;
            
            yield return new WaitForSeconds(spawnRate);
        }
    }
}
