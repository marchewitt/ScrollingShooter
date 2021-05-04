using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


public class Laser : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    
    [Header("Prefabs Needed")]
    [SerializeField] private GameObject collisionPrefab;

    // Update is called once per frame
    private void Update()
    {
        transform.Translate(Vector3.up * (speed * Time.deltaTime));

        if (transform.position.y >= 8f)
        {
            Destroy(gameObject);
        }
    }
    
    // private void OnCollisionEnter()
    // {
    //     Instantiate(collisionPrefab, transform.position, transform.rotation);
    //     Destroy(gameObject);
    // }
}
