using System;
using System.Collections;
using System.Collections.Generic;
using Config;
using UnityEngine;
using UnityEngine.Serialization;

public class PowerUp : MonoBehaviour
{
    [SerializeField] private float speed = 3.0f;
    [SerializeField] private float duration = 0f;
    
    [Tooltip("0 is TripleShot, 1 is SpeedUp, 2 is Shield, 3 is ExtraLife")]
    [SerializeField] private int powerUpID;
    [SerializeField] private AudioClip powerUpAudio;
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
            AudioSource.PlayClipAtPoint(powerUpAudio, transform.position);
            switch (powerUpID)
            {
                case 0:
                    Debug.Log("TripleShot Picked up");
                    player.CollectPowerUp_TripleShot(this);
                    break;
                case 1:
                    Debug.Log("SpeedUp Picked up");
                    player.CollectPowerUp_SpeedUp(this);
                    break;
                case 2:
                    Debug.Log("Shield Picked up");
                    player.CollectPowerUp_Shield(this);
                    break;
                case 3:
                    Debug.Log("ExtraLife Picked Up");
                    player.CollectPowerUp_ExtraLife(this);
                    break;
                case 4:
                    Debug.Log("Ammo Picked Up");
                    player.CollectPowerUp_Ammo(this);
                    break;
                default:
                    Debug.Log($"Missing PowerUp ID! Was given {powerUpID}");
                    break;
            }
        }
        Destroy(gameObject);
    }

}
