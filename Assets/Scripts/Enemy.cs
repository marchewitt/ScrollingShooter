using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Config;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    [Tooltip("Enemy's movement speed")]
    [SerializeField] private float speed = 4.0f;
    
    private Vector3 _direction = Vector3.down; 
    

    private void Update()
    {
        transform.Translate(_direction * (speed * Time.deltaTime));

        if (transform.position.y < ScreenBounds.ScreenBottom)
        {
            Respawn(transform.position.x);
        }
        
    }

    private void Respawn(float oldX)
    {
        var randomX = oldX;
        while (oldX - 0.1f <= randomX && randomX <= oldX + 0.1f)
        {
            //Random spawn position until not close to old x position.
            randomX = Random.Range(ScreenBounds.ScreenLeft, ScreenBounds.ScreenRight);
        }

        transform.position = new Vector3(randomX, ScreenBounds.ScreenTop + 2f, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player_Attack"))
        {
            other.GetComponent<Laser>().DestoryUs();
            DestroyUs();
            //TODO: RewardPoints();
        }
        else if (other.CompareTag("Player"))
        {
            Player player = other.gameObject.GetComponent<Player>();
            if (player)
            {
                player.TakeDamage(1);
            }
            else
            {
                Debug.LogError("Player was null!!!");
            }
            DestroyUs();
        }
    }

    private void DestroyUs()
    {
        //TODO: Instantiate(collisionPrefab, transform.position, transform.rotation);
        Respawn(transform.position.x);
    }
}
