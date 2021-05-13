using System.Collections;
using UnityEngine;
using Config;
using Managers;
using UI;

[RequireComponent(typeof(AudioSource))]
public class Player : MonoBehaviour
{
    //Manager Refs
    private SpawnManager _spawnManager;
    private UIManager _uiManager;
    private GameManager _gameManager;
    
    //Internal Refs
    private AudioSource _audioSource;
    
    [Header("Player Data")]
    private float _speed;
    private int _score;

    [Header("Player Config")]
    [SerializeField] private int health = 3;
    [SerializeField] private float baseSpeed = 3.5f;
    [SerializeField] private GameObject rightEngineRef;
    [SerializeField] private GameObject leftEngineRef;
    
    [Header("Thruster Config")]
    [Tooltip("1f would be no change, 1.3f is 30% faster")]
    [SerializeField] private float thrusterMultiplier = 1.4f;
    [SerializeField] private GameObject thursterVFX;
    private bool _isThrusterActive = false;

    [Header("Laser Settings")] 
    
    [SerializeField] private AudioClip laserFireAudio;
    
    [SerializeField] private Transform laserSpawnPosition;
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private GameObject tripleShotPrefab;
    [SerializeField] private float fireRate = 0.15f;
    private float _canFireLaserTimer;

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

            if (health <= 0)
            {
                OnPlayerDestroy();
            }
            else
            {
                UpdateShipVisuals(health);
            }
        }
    }

    private bool IsThrusterActive
    {
        get => _isThrusterActive;
        set
        {
            _isThrusterActive = value;
            if (thursterVFX)
            {
                thursterVFX.SetActive(_isThrusterActive);
            }
        }
    }


    public void Awake()
    {
        shieldsVFXRef.SetActive(false);
        _speed = baseSpeed;
        
        if(rightEngineRef == null){Debug.LogError("rightEngineRef was null");}
        rightEngineRef.SetActive(false);
        if(leftEngineRef == null){Debug.LogError("leftEngineRef was null");}
        leftEngineRef.SetActive(false);
        if(thursterVFX == null){Debug.LogError("leftEngineRef was null");}
        thursterVFX.SetActive(false);
        
        
        _audioSource = gameObject.GetComponent<AudioSource>();
        if(_audioSource == null){Debug.LogError("audioSource was null");}
        
        if(laserFireAudio == null){
            Debug.LogError("laserFireAudio was null");
        }
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
        IsThrusterActive = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

        if (Input.GetKeyDown(KeyCode.Space) && Time.time > _canFireLaserTimer)
        {
            FireLaser();
        }
        
        CalculateMovement();
    }


    private void CalculateMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontalInput, verticalInput, 0).normalized;
        float speedMultiplier = 1f;

        if (IsThrusterActive)
        {
            speedMultiplier *= thrusterMultiplier;
        }

        transform.Translate((moveDirection * ((_speed * speedMultiplier) * Time.deltaTime)));    
        

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
        _canFireLaserTimer = Time.time + fireRate;
        
        //Check if Power-up is active or if default laser
        var prefabToUse = _isTripleShotEnabled ? tripleShotPrefab : laserPrefab;
        Instantiate(prefabToUse, laserSpawnPosition.position, Quaternion.identity);
        PlayOneShot(laserFireAudio);
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
    
    
    private void UpdateShipVisuals(int currentHealth)
    {
        //Update the ship damage visuals based on currentHealth
        switch(currentHealth)
        {
            case 3:
                rightEngineRef.SetActive(false);
                leftEngineRef.SetActive(false);
                break;
            case 2:
                rightEngineRef.SetActive(true);
                leftEngineRef.SetActive(false);
                break;
            case 1:
                rightEngineRef.SetActive(true);
                leftEngineRef.SetActive(true);
                break;
        }
    }


    private void PlayOneShot(AudioClip clip)
    {
        if (clip)
        {
            _audioSource.PlayOneShot(clip);
        }
    }
}
