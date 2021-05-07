using System;
using System.Collections;
using UnityEngine;
using Config;

public class Player : MonoBehaviour
{
    [SerializeField] private int health = 3;
    [Tooltip("Players movement speed")]
    [SerializeField] private float speed = 3.5f;

    [Header("Laser Settings")]
    [SerializeField] private Transform laserSpawnPosition;
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private GameObject tripleShotPrefab;
    [SerializeField] private float fireRate = 0.15f;
    private float _canFireTimer = 0;

    [Header("TripleShot PowerUp")]
    public bool _isTripleShotEnabled = false; //Todo: Currently testing, eventually make this private

    private SpawnManager _spawnManager;
    public void Start()
    {
        _spawnManager = GameObject.Find("Spawn_Manager").GetComponent<SpawnManager>();
        if(_spawnManager == null){Debug.LogError("Spawn_Manager was null");}
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
        
        //Is Power-up active?
        var prefabToUse = _isTripleShotEnabled ? tripleShotPrefab : laserPrefab;
        Instantiate(prefabToUse, laserSpawnPosition.position, Quaternion.identity);
    }

    public void TakeDamage(int value)
    {
        health -= value;
        Debug.Log($"Player Remaining Health {health}");
        if(health <= 0){
            DestroyUs();
        }
    }

    private void DestroyUs()
    {
        //TODO: Instantiate(_deathPrefab, transform.position.x, Quaternion.identity);
        _spawnManager.OnPlayerDeath();
        Destroy(gameObject);
    }

    private IEnumerator _powerUpTimerRef;
    public void CollectPowerUp(PowerUp powerUp)
    {
        if (_powerUpTimerRef != null)
        {
            StopCoroutine(_powerUpTimerRef);
        }
        _isTripleShotEnabled = true; //TODO: unique PowerUp
        _powerUpTimerRef = PowerUpTimer(powerUp.Duration);
        StartCoroutine(_powerUpTimerRef);
    }

    private IEnumerator PowerUpTimer(float timer)
    {
        Debug.Log("Timer");
        yield return new WaitForSeconds(timer);
        _isTripleShotEnabled = false;
    }
}
