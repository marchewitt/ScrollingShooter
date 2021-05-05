using System.Collections;
using System.Collections.Generic;
using Packages.Rider.Editor.UnitTesting;
using UnityEngine;
using UnityEngine.Serialization;
using Config;

public class Player : MonoBehaviour
{
    [SerializeField] private int health = 3;
    [Tooltip("Players movement speed")]
    [SerializeField] private float speed = 3.5f;

    [Header("Laser Settings")]
    [SerializeField] private Transform laserSpawnPosition;
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private float fireRate = 0.15f;
    private float _canFireTimer = 0;
    
    //Screen Bounds
    // private const float ScreenRight = 8.5f;
    // private const float ScreenLeft = -8.5f;
    // private const float ScreenTop = 5.8f;
    // private const float ScreenBottom = -3.8f;

    public void Start()
    {
        transform.position = Vector3.zero;
    }


    private void Update()
    {
        CalculateMovement();

        if (Input.GetKeyDown(KeyCode.Space) && Time.time > _canFireTimer)
        {
            FireLaser();
        }
    }

    private void CalculateMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        var moveDirection = new Vector3(horizontalInput, verticalInput, 0).normalized;

        transform.Translate((moveDirection * (speed * Time.deltaTime)));

        #region Wrap And Clamp Screen Bounds

        var position = transform.position;
        if (position.x > ScreenBounds.ScreenRight)
        {
            position = new Vector3(ScreenBounds.ScreenLeft, position.y, 0);
        }
        else if (transform.position.x < ScreenBounds.ScreenLeft)
        {
            position = new Vector3(ScreenBounds.ScreenRight, position.y, 0);
        }

        transform.position = new Vector3(position.x, Mathf.Clamp(position.y, ScreenBounds.ScreenBottom, ScreenBounds.ScreenTop), 0);

        #endregion
    }

    private void FireLaser()
    {
        _canFireTimer = Time.time + fireRate;
        Instantiate(laserPrefab, laserSpawnPosition.position, Quaternion.identity);
    }

    public void TakeDamage(int value)
    {
        health -= value;
        Debug.Log($"Player Remaining Health {health}");
        DestroyUs();
    }

    private void DestroyUs()
    {
        //TODO: Instantiate(_deathPrefab, transform.position.x, Quaternion.identity);
        Destroy(gameObject);
    }
}
