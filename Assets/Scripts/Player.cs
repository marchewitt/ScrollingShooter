using System;
using System.Collections;
using UnityEngine;
using Config;
using UnityEngineInternal;

public class Player : MonoBehaviour
{
    [SerializeField] private int health = 3;
    [Tooltip("Players movement speed")]
    [SerializeField] private float baseSpeed = 3.5f;
    private float _speed = 0;

    [Header("Laser Settings")]
    [SerializeField] private Transform laserSpawnPosition;
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private GameObject tripleShotPrefab;
    [SerializeField] private float fireRate = 0.15f;
    private float _canFireTimer = 0;

    [Header("TripleShot PowerUp")]
    private bool _isTripleShotEnabled = false;
    private IEnumerator _tripleShotTimerRef;

    [Header("Speed PowerUp")]
    [Tooltip("1.3f would be 30% faster")]
    [SerializeField] private float speedPowerUpMultiplier = 1.3f;
    private bool _isSpeedUpEnabled = false;
    private IEnumerator _speedTimerRef;

    [Header("Speed PowerUp")] 
    [SerializeField] private GameObject shieldsVFXRef;

    private SpawnManager _spawnManager;
    private bool _isShieldOn = false;
    private bool IsShieldOn
    {
        get => _isShieldOn;
        set
        {
            _isShieldOn = value;
            shieldsVFXRef.SetActive(_isShieldOn);
        }
    }

    public void Awake()
    {
        shieldsVFXRef.SetActive(false);
        _speed = baseSpeed;
    }

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

        transform.Translate((moveDirection * (_speed * Time.deltaTime)));    
        

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
        if (IsShieldOn)
        {
            IsShieldOn = false;
            return;
        }
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

    // private IEnumerator _powerUpTimerRef;
    // public void CollectPowerUp(PowerUp powerUp)
    // {
    //     if (_powerUpTimerRef != null)
    //     {
    //         StopCoroutine(_powerUpTimerRef);
    //     }
    //     _isTripleShotEnabled = true; //TODO: unique PowerUp
    //     _powerUpTimerRef = PowerUpTimer(powerUp.Duration);
    //     StartCoroutine(_powerUpTimerRef);
    // }
    //
    // private IEnumerator PowerUpTimer(float timer)
    // {
    //     Debug.Log("Timer");
    //     yield return new WaitForSeconds(timer);
    //     _isTripleShotEnabled = false;
    // }
    

    public void CollectPowerUp_TripleShot(PowerUp powerUp)
    {
        if (_tripleShotTimerRef != null)
        {
            StopCoroutine(_tripleShotTimerRef);
        }
        _isTripleShotEnabled = true;
        _tripleShotTimerRef = PowerUpTimer_TripleShot(powerUp.Duration);
        StartCoroutine(_tripleShotTimerRef);
    }

    private IEnumerator PowerUpTimer_TripleShot(float timer)
    {
        Debug.Log("Timer");
        yield return new WaitForSeconds(timer);
        _isTripleShotEnabled = false;
    }


    public void CollectPowerUp_SpeedUp(PowerUp powerUp)
    {
        if (_speedTimerRef != null)
        {
            StopCoroutine(_speedTimerRef);
        }
        _isSpeedUpEnabled = true; 
        _speed = baseSpeed * speedPowerUpMultiplier;
        
        _speedTimerRef = PowerUpTimer_SpeedUp(powerUp.Duration);
        StartCoroutine(_speedTimerRef);
    }

    private IEnumerator PowerUpTimer_SpeedUp(float timer)
    {
        Debug.Log("Timer");
        yield return new WaitForSeconds(timer);
        _speed = baseSpeed;
    }
    
    public void CollectPowerUp_Shield(PowerUp powerUp)
    {
        if (IsShieldOn)
        {
            return;
        }
        IsShieldOn = true;
    }
    
}
