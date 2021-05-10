using System;
using Config;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    private Player _playerRef;
    [SerializeField] private int pointsWorth = 10;
    [Tooltip("Enemy's movement speed")]
    [SerializeField] private float speed = 4.0f;

    private void Start()
    {
        _playerRef = GameObject.FindWithTag("Player").GetComponent<Player>();
        if(_playerRef == null){Debug.LogError("_playerRef was null in " + gameObject);}
    }

    private void Update()
    {
        transform.Translate(Vector3.down * (speed * Time.deltaTime));

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



    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("hit");
        if (other.CompareTag("Player_Attack"))
        {
            other.GetComponent<Laser>().DestroyUs();
            if( _playerRef ) { _playerRef.AddScore(pointsWorth); }
            DestroyUs();
        }
        else if (other.CompareTag("Player"))
        {
            var player = other.gameObject.GetComponent<Player>();
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
