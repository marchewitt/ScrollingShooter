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
    private float _currentHeat;

    [Header("Player Config")]
    [Tooltip("This is players max health as well as the starting health")]
    [SerializeField] private int maxHealth = 3;
    private int _currentHealth;
    [Tooltip("Heat as in Engine Heat is used by Thrusters")]
    [SerializeField] private float maxHeat = 40f;
    [Tooltip("Base Heat cooldown per second if no heat is applied")]
    [SerializeField] private float baseHeatCooldownRate = 5f;
    [SerializeField] private float baseSpeed = 3.5f;
    [SerializeField] private GameObject rightEngineRef;
    [SerializeField] private GameObject leftEngineRef;
    
    [Header("Thruster Config")]
    [Tooltip("1f would be no change, 1.3f is 30% faster")]
    [SerializeField] private float thrusterMultiplier = 1.4f;
    [SerializeField] private GameObject thrusterVFX;
    [Tooltip("Amount of Heat thrusters apply to the ship engine per second - see Player.MaxHeat")] [SerializeField]
    private float thrusterHeatGen = 10f;
    private bool _isThrusterActive = false;
    private bool _isEngineOverheated = false;
    [Range(0f,1f)] [Tooltip("When engine is overheated, we can use this field to reduce player speed")]
    [SerializeField] private float overheatedSpeedMultiplier = 0.7f;

    [Header("Laser Settings")] 
    
    [SerializeField] private AudioClip laserFireAudio;
    
    [SerializeField] private Transform laserSpawnPosition;
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private GameObject tripleShotPrefab;
    [SerializeField] private float fireRate = 0.15f;
    private float _canFireLaserTimer;

    [Header("TripleShot PowerUp")]
    private bool _isTripleShotEnabled = false;
    private Coroutine _tripleShotTimerRef;

    [Header("Speed PowerUp")]
    [Tooltip("1.3f would be 30% faster")]
    [SerializeField] private float speedUpMultiplier = 2f;
    private Coroutine _speedTimerRef;

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
        get => _currentHealth;
        set
        {
            _currentHealth = value;
            if(_uiManager){ _uiManager.UpdateLives(_currentHealth);}

            if (_currentHealth <= 0)
            {
                OnPlayerDestroy();
            }
            else if (_currentHealth > maxHealth)
            {
                _currentHealth = maxHealth;
            }
            
            UpdateShipVisuals(_currentHealth);
            
        }
    }

    private bool IsThrusterActive
    {
        get => _isThrusterActive;
        set
        {
            _isThrusterActive = value;
            if (thrusterVFX)
            {
                thrusterVFX.SetActive(_isThrusterActive);
            }
        }
    }
    
    private bool IsSpeedUpActive { get; set; }

    private float EngineHeat
    {
        get => _currentHeat;
        set
        {
            _currentHeat = value;
            if (_currentHeat <= 0)
            {
                _currentHeat = 0;
                if (IsEngineOverheated)
                {
                    IsEngineOverheated = false;
                }
            }
            else if (_currentHeat > maxHeat)
            {
                _currentHeat = maxHeat;
                IsEngineOverheated = true;
            }
            
            if(_uiManager){
                _uiManager.UpdateEngineHeat(_currentHeat / maxHeat);
            }
        }
    }

    public bool IsEngineOverheated
    {
        get => _isEngineOverheated;
        set
        {
            _isEngineOverheated = value;
            //TODO: Play SFX for when it overheats and reaches 0%
            if (_uiManager)
            {
                _uiManager.IsEngineOverheated = _isEngineOverheated;
            }
        }
    }


    public void Awake()
    {
        shieldsVFXRef.SetActive(false);
        _speed = baseSpeed;
        Health = maxHealth;
        
        if(rightEngineRef == null){Debug.LogError("rightEngineRef was null");}
        rightEngineRef.SetActive(false);
        if(leftEngineRef == null){Debug.LogError("leftEngineRef was null");}
        leftEngineRef.SetActive(false);
        if(thrusterVFX == null){Debug.LogError("leftEngineRef was null");}
        thrusterVFX.SetActive(false);
        
        
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
        
        StartCoroutine(CalculateEngineHeat());
    }

    private void Update()
    {
        
        IsThrusterActive = (IsEngineOverheated == false) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        

        if (Input.GetKeyDown(KeyCode.Space) && Time.time > _canFireLaserTimer)
        {
            FireLaser();
        }
        
        CalculateMovement();
    }

    [Tooltip("Temp value to shift calculation speed")]
    public float engineTimerRate = 0.15f;
    private IEnumerator CalculateEngineHeat()
    {
        while(true){
            float heatModifer = 0;
            if (IsThrusterActive)
            {
                heatModifer += thrusterHeatGen;
            }

            if (heatModifer <= 0)
            {
                //If we have a negative or zero heatModifer we combine base cooldown rate into it
                heatModifer -= baseHeatCooldownRate;
            }
            
            EngineHeat += heatModifer;
            yield return new WaitForSeconds(engineTimerRate);
        }
        
    }


    private void CalculateMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontalInput, verticalInput, 0).normalized;


        #region Speed Modifers
        /*
         * Note: Desired math of Speed modified is based on  multipliers
         * So a 2x speed modifier and 2x speed modifer stack to 4x
         * So a base speed of 10 would = 40 (10 speed * (2 * 2) = 40).
         *
         * We are NOT doing additive speed as that would be (10 speed + (10 + 10) = 30)
         * This is so if we have a "negative" modifier of say 50% slow down we would get a true 50%
         */  
        float speedMultiplier = 1f;

        if (IsThrusterActive)
        {
            speedMultiplier *= thrusterMultiplier;
        }

        if (IsEngineOverheated)
        {
            speedMultiplier *= overheatedSpeedMultiplier;
        }

        if (IsSpeedUpActive)
        {
            speedMultiplier *= speedUpMultiplier;
        }

        _speed = baseSpeed * speedMultiplier;
        #endregion
        

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
        _tripleShotTimerRef = StartCoroutine(PowerUpTimer_TripleShot(powerUp.Duration));
        
    }

    private IEnumerator PowerUpTimer_TripleShot(float timer)
    {
        yield return new WaitForSeconds(timer);
        _isTripleShotEnabled = false;
    }


    public void CollectPowerUp_SpeedUp(PowerUp powerUp)
    {
        if (_speedTimerRef != null)
        {
            StopCoroutine(_speedTimerRef);
        }

        IsSpeedUpActive = true;
        _speedTimerRef = StartCoroutine(PowerUpTimer_SpeedUp(powerUp.Duration));
        //_speedTimerRef;
    }

    private IEnumerator PowerUpTimer_SpeedUp(float timer)
    {
        yield return new WaitForSeconds(timer);
        IsSpeedUpActive = false;
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

    public void CollectPowerUp_ExtraLife(PowerUp powerUp)
    {
        Health += 1;
    }
}
