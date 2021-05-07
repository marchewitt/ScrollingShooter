using System;
using System.Collections;
using System.Collections.Generic;
using Config;
using UnityEngine;
using UnityEngine.Serialization;

public class PowerUp : MonoBehaviour
{
    [SerializeField] private float speed = 3.0f;
    [SerializeField] private float duration = 3.0f;

    public float Duration
    {
        get => duration;
        private set => duration = value;
    }

    private void Update()
    {
        transform.Translate(Vector3.down * (speed * Time.deltaTime));

        if (transform.position.y < ScreenBounds.ScreenBottom)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        //Play Collection SFX
        var player = other.GetComponent<Player>();
        if (player)
        {
            player.CollectPowerUp(this);
        }
        Destroy(gameObject);
    }

}
