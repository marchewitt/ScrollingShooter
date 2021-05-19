using System;
using System.Collections;
using Cinemachine;
using UnityEngine;
using Config;
using Managers;
using UI;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(CinemachineImpulseSource))]
public class Player : MonoBehaviour
{
    //Manager Refs
    private SpawnManager _spawnManager;
    private UIManager _uiManager;
    private GameManager _gameManager;
    
    //Internal Refs
    private AudioSource _audioSource;
    private CinemachineImpulseSource _impulseSource;
    
    //Player Data
    private float _speed;
    private int _score;
    private float _currentHeat;
    private int _shieldStrength;
    private int _currentHealth;
    private int _currentAmmo;

    [Header("Player Config")]
    [Tooltip("This is players max health as well as their starting health")]
    [SerializeField] private int maxHealth = 3;
    
    [Tooltip("Heat as in Engine Heat is used by Thrusters")]
    [SerializeField] private float maxHeat = 50f;
    [Tooltip("Base Heat cooldown per tick if no heat is applied to engine")]
    [Range(0f, 10.0f)] [SerializeField] private float baseHeatCooldownRate = 5f;
    [SerializeField] private float baseSpeed = 3.5f;
    [SerializeField] private GameObject rightEngineRef;
    [SerializeField] private GameObject leftEngineRef;
    
    [Header("Thruster Config")]
    [Tooltip("1f would be no change, 1.3f is 30% faster")]
    [SerializeField] private float thrusterMultiplier = 1.8f;
    [SerializeField] private GameObject thrusterVFX;
    [Tooltip("Amount of Heat thrusters apply to the ship engine per second - see Player.MaxHeat")] [SerializeField]
    private float thrusterHeatGen = 6f;
    private bool _isThrusterActive;
    private bool _isEngineOverheated;
    [Range(0f,1f)] [Tooltip("When engine is overheated, we can use this field to reduce player speed")]
    [SerializeField] private float overheatedSpeedMultiplier = 0.7f;

    [Header("Laser Settings")]
    [Range(0, 30)] [SerializeField] private int maxLaserAmmo = 15;
    [FormerlySerializedAs("laserFireAudio")] [SerializeField] private AudioClip laserFireSFX;
    [SerializeField] private AudioClip noAmmoSFX;
    [SerializeField] private Transform laserSpawnPosition;
    [SerializeField] private GameObject laserPrefab;
    
    [SerializeField] private float fireRate = 0.15f;
    private float _canFireLaserTimer;
    [SerializeField] private GameObject laserContainer;

    [Header("TripleShot PowerUp")]
    [SerializeField] private GameObject tripleShotPrefab;
    private Coroutine _tripleShotTimerRef;

    [Header("Speed PowerUp")]
    [Tooltip("1.3f would be 30% faster")]
    [SerializeField] private float speedUpMultiplier = 2f;
    private Coroutine _speedTimerRef;

    [Header("Shield PowerUp")]
    [SerializeField] private SpriteRenderer shieldSpriteVFXRef;
    [FormerlySerializedAs("shieldStateColors")]
    [Tooltip("First color will be shield strength 1. Shield being at 0 is just visuals disabled")] 
    [SerializeField] private Color[] shieldStrengthColors;
    [SerializeField] private int shieldMaxStrength = 3;
    
    [Header("Misc Effects")]
    [Tooltip("OnHit we fire off ImpulseSource with this ScreenShakeForce")]
    [SerializeField] private float screenShakeForce = 2.5f;

    


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

            if (leftEngineRef && rightEngineRef)
            {
                #region Update Ship Visuals

                //Update the ship damage visuals based on currentHealth
                switch (_currentHealth)
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

                #endregion
            }

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

    private bool IsEngineOverheated
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

    private int ShieldStrength
    {
        get => _shieldStrength;
        set
        {
            _shieldStrength = value;
            _shieldStrength = Math.Max(_shieldStrength, 0); //prevent negative
            _shieldStrength = Math.Min(_shieldStrength, shieldMaxStrength); //prevent going over max

            if(shieldSpriteVFXRef){
                if (IsShieldOn && _shieldStrength <= shieldStrengthColors.Length)
                {
                    shieldSpriteVFXRef.color = shieldStrengthColors[_shieldStrength - 1];
                }
                shieldSpriteVFXRef.enabled = IsShieldOn;
            }
        }
    }
    private bool IsShieldOn => _shieldStrength > 0;

    private int Ammo
    {
        get => _currentAmmo;
        set
        {
            _currentAmmo = value;
            _currentAmmo = Math.Max(0, _currentAmmo);
            _currentAmmo = Math.Min(_currentAmmo, maxLaserAmmo);

            if (_uiManager)
            {
                _uiManager.UpdateAmmo(_currentAmmo, maxLaserAmmo);
            }
        }
    }
    private bool HasAmmo => _currentAmmo > 0;

    private bool IsTripleShotEnabled { get; set; }
    
    private void Awake()
    {
        if(rightEngineRef == null){Debug.LogError("rightEngineRef was null");}
        rightEngineRef.SetActive(false);
        if(leftEngineRef == null){Debug.LogError("leftEngineRef was null");}
        leftEngineRef.SetActive(false);
        if(thrusterVFX == null){Debug.LogError("leftEngineRef was null");}
        thrusterVFX.SetActive(false);
        
        
        _audioSource = gameObject.GetComponent<AudioSource>();
        if(_audioSource == null){Debug.LogError("audioSource was null");}

        _impulseSource = gameObject.GetComponent<CinemachineImpulseSource>();
        if(_impulseSource == null){Debug.LogError("CineMachineImpulseSource was null");}
        
        if(laserFireSFX == null){
            Debug.LogError("laserFireAudio was null");
        }
        if(noAmmoSFX == null){
            Debug.LogError("noAmmoSFX was null");
        }

        
    }

    public void Start()
    {
        if (shieldSpriteVFXRef)
        {
            shieldSpriteVFXRef.enabled = false;            
        }
        else{Debug.LogError("ShieldSpriteVFXRef was null");}
        
        if(shieldMaxStrength > shieldStrengthColors.Length){Debug.LogError("ShieldMaxStrength is higher than shieldStateColors length.");}

        _spawnManager = GameObject.Find("Spawn_Manager").GetComponent<SpawnManager>();
        if(_spawnManager == null){Debug.LogError("SpawnManager was null");}
        _uiManager = GameObject.Find("Master_Canvas").GetComponent<UIManager>();
        if(_uiManager == null){Debug.LogError("UIManager was null on Master_Canvas");}
        _gameManager = GameObject.Find("Game_Manager").GetComponent<GameManager>();
        if(_gameManager == null){Debug.LogError("GameManager was null");}
        if(laserContainer == null){Debug.LogError("laserContainer was null");}
        
        InitConfig();
        StartCoroutine(CalculateEngineHeat());
    }

    private void InitConfig()
    {
        _speed = baseSpeed;
        Score = 0;
        Health = maxHealth;
        ShieldStrength = 0;
        EngineHeat = 0;
        Ammo = maxLaserAmmo;
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
            yield return new WaitForSeconds(0.15f);
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
        GameObject newObj = null;
        _canFireLaserTimer = Time.time + fireRate;
        if (IsTripleShotEnabled)
        {
            //TripleShot PowerUp does not use Ammo
            newObj = Instantiate(tripleShotPrefab, laserSpawnPosition.position, Quaternion.identity);
            PlayOneShot(laserFireSFX);
        }
        else if ( HasAmmo == false)
        {
            PlayOneShot(noAmmoSFX);
        }
        else{ 
            //DefaultLaser
            Ammo--;
            newObj = Instantiate(laserPrefab, laserSpawnPosition.position, Quaternion.identity);
            PlayOneShot(laserFireSFX);
        }

        //Put laser into container
        if (newObj)
        {
            newObj.transform.parent = laserContainer.transform;
        }
    }

    public void TakeDamage(int value)
    {
        if (IsShieldOn)
        {
            ShieldStrength--;
            return;
        }
        _impulseSource.GenerateImpulse(screenShakeForce);
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

    #region PowerUp Handling
    
    public void CollectPowerUp_TripleShot(PowerUp powerUp)
    {
        if (_tripleShotTimerRef != null)
        {
            StopCoroutine(_tripleShotTimerRef);
        }
        
        IsTripleShotEnabled = true;
        _tripleShotTimerRef = StartCoroutine(PowerUpTimer_TripleShot(powerUp.Duration));
    }

    private IEnumerator PowerUpTimer_TripleShot(float timer)
    {
        yield return new WaitForSeconds(timer);
        IsTripleShotEnabled = false;
    }


    public void CollectPowerUp_SpeedUp(PowerUp powerUp)
    {
        if (_speedTimerRef != null)
        {
            StopCoroutine(_speedTimerRef);
        }
        
        IsSpeedUpActive = true;
        _speedTimerRef = StartCoroutine(PowerUpTimer_SpeedUp(powerUp.Duration));
    }

    private IEnumerator PowerUpTimer_SpeedUp(float timer)
    {
        yield return new WaitForSeconds(timer);
        IsSpeedUpActive = false;
    }
    
    public void CollectPowerUp_Shield(PowerUp powerUp) => ShieldStrength++;
    
    public void CollectPowerUp_ExtraLife(PowerUp powerUp) => Health += 1;
    
    public void CollectPowerUp_Ammo(PowerUp powerUp) => Ammo = maxLaserAmmo;
    
    
    #endregion
    
    public void AddScore(int value)
    {
        if (value <= 0) { return; }
        Score += value;
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (clip)
        {
            _audioSource.PlayOneShot(clip);
        }
    }
}
