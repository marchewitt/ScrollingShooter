using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [SerializeField] private float rotateSpeed;
    [SerializeField] private GameObject explosionPrefab;
    private void Update()
    {
        transform.Rotate(Vector3.forward * (rotateSpeed * Time.deltaTime));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player_Attack"))
        {
            if (explosionPrefab)
            {
                Instantiate(explosionPrefab, transform.position, transform.rotation);
            }
            other.gameObject.GetComponent<Laser>().OnDestroyLaser();
            Destroy(gameObject);
        }
    }
}
