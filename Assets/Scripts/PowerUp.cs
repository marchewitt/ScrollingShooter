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
    
    [Tooltip("0 is tripleshot, 1 is speed, 2 is shield")]
    [SerializeField] private int powerUpID;
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
            switch (powerUpID)
            {
                case 0:
                    Debug.Log("Triple-shot Picked up");
                    player.CollectPowerUp_TripleShot(this);
                    break;
                case 1:
                    Debug.Log("Speed Picked up");
                    player.CollectPowerUp_SpeedUp(this);
                    break;
                case 2:
                    Debug.Log("Shield Picked up");
                    break;
            }
        }
        Destroy(gameObject);
    }

}
