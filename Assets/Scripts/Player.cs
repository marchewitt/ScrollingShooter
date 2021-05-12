using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using Config;
using TMPro.EditorUtilities;
using UI;
using UnityEngineInternal;

public class Player : MonoBehaviour
{
    //Manager Refs
    private SpawnManager _spawnManager;
    private UIManager _uiManager;
    private GameManager _gameManager;
    
    [Header("Player Data")]
    private int _score = 0;
    [Header("Player Config")]
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
    private IEnumerator _speedTimerRef;

    [Header("Speed PowerUp")] 
    private bool _isShieldOn = false;
    [SerializeField] private GameObject shieldsVFXRef;
    

    private bool IsShieldOn
    {
        get => _isShieldOn;
        set
        {
            _isShieldOn = value;
            shieldsVFXRef.SetActive(_isShieldOn);
        }
    }

    private int Score
    {
        get => _score;
        set
        {
            _score = value;
            if (_uiManager)
            {
                _uiManager.UpdateScore(_score);
            }
        }
    }

    private int Health
    {
        get => health;
        set
        {
            health = value;
            if(_uiManager){ _uiManager.UpdateLives(health);}
            if(health <= 0){ OnPlayerDestroy(); }
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
        if(_spawnManager == null){Debug.LogError("SpawnManager was null");}
        _uiManager = GameObject.Find("Master_Canvas").GetComponent<UIManager>();
        if(_uiManager == null){Debug.LogError("UIManager was null on Master_Canvas");}
        _gameManager = GameObject.Find("Game_Manager").GetComponent<GameManager>();
        if(_gameManager == null){Debug.LogError("GameManager was null");}
        
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
        Health -= value;
    }

    private void OnPlayerDestroy()
    {
        //TODO: Instantiate(_deathPrefab, transform.position.x, Quaternion.identity);
        _spawnManager.OnPlayerDeath();
        _uiManager.UpdateGameOver(true);
        _gameManager.GameOver();
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

    public void AddScore(int value)
    {
        if(value > 0){
            Score += value;
        }
    }
}
