using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [SerializeField] private float rotateSpeed;
    [SerializeField] private GameObject explosionPrefab;
    private SpawnManager _spawnManager;
    private void Start()
    {
        _spawnManager = GameObject.Find("Spawn_Manager").GetComponent<SpawnManager>();
        if(_spawnManager == null){Debug.LogError("SpawnManager was null");}
    }
    private void Update() => transform.Rotate(Vector3.forward * (rotateSpeed * Time.deltaTime));

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player_Attack"))
        {
            if (explosionPrefab)
            {
                Instantiate(explosionPrefab, transform.position, transform.rotation);
            }
            other.gameObject.GetComponent<Laser>().OnDestroyLaser();
            _spawnManager.StartSpawners();
            Destroy(gameObject);
        }
    }
}
