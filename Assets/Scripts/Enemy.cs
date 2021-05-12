using System.Collections;
using Config;
using UnityEngine;
using Random = UnityEngine.Random;


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(BoxCollider2D))]
public class Enemy : MonoBehaviour
{
    //External Refs
    private Player _player;
    
    //Internal Refs
    private AudioSource _audioSource;
    private Animator _animator;
    private BoxCollider2D _collider2D;
    
    [SerializeField] private int pointsWorth = 10;
    [Tooltip("Enemy's movement speed")]
    [SerializeField] private float baseSpeed = 4.0f;
    private float _speed;

    [SerializeField] private AudioClip explosionAudio; 
    
    #region Animations
    private static readonly int OnDestroyHash = Animator.StringToHash("OnDestroy"); //Animation Trigger
    #endregion

    private void Awake()
    {
        _audioSource = gameObject.GetComponent<AudioSource>();
        if(_audioSource == null){Debug.LogError("_audioSource was null in " + gameObject);}
            
        _animator = gameObject.GetComponent<Animator>();
        if(_animator == null){Debug.LogError("_animator was null in " + gameObject);}

        _collider2D = gameObject.GetComponent<BoxCollider2D>();
        if(_collider2D == null){Debug.LogError("_collider2D was null in " + gameObject);}
        
        if(explosionAudio == null){Debug.LogError("explosionAudio was null in " + gameObject);}
    }
    private void Start()
    {
        _player = GameObject.FindWithTag("Player").GetComponent<Player>();
        if(_player == null){Debug.LogError("_player was null in " + gameObject);}

        _speed = baseSpeed;
    }

    private void Update()
    {
        transform.Translate(Vector3.down * (_speed * Time.deltaTime));

        if (transform.position.y < ScreenBounds.ScreenBottom)
        {
            Respawn(transform.position.x);
        }
        
    }



    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("hit");
        if (other.CompareTag("Player_Attack"))
        {
            other.GetComponent<Laser>().OnDestroyLaser();
            if( _player ) { _player.AddScore(pointsWorth); }
            OnEnemyDestroy();
        }
        else if (other.CompareTag("Player"))
        {
            var player = other.gameObject.GetComponent<Player>();
            if (player)
            {
                player.TakeDamage(1);
            }
            else
            {
                Debug.LogError("Player was null!!!");
            }
            OnEnemyDestroy();
        }
    }

    private void OnEnemyDestroy()
    {
        StartCoroutine(OnEnemyDestroyDelay(2.6333f));
    }

    /// <summary>
    /// Delaying for animation
    /// </summary>
    /// <returns></returns>
    private IEnumerator OnEnemyDestroyDelay(float clipLength)
    {
        _collider2D.enabled = false;
        _animator.SetTrigger(OnDestroyHash);
        PlayOneShot(explosionAudio);
        _speed = 0f;
        yield return new WaitForSeconds(clipLength);
        Respawn(transform.position.x);
    }
    
    private void Respawn(float oldX)
    {
        _collider2D.enabled = true;
        _speed = baseSpeed;
        var randomX = oldX;
        while (oldX - 0.1f <= randomX && randomX <= oldX + 0.1f)
        {
            //Random spawn position until not close to old x position.
            randomX = Random.Range(ScreenBounds.ScreenLeft, ScreenBounds.ScreenRight);
        }

        transform.position = new Vector3(randomX, ScreenBounds.ScreenTop + 3f, 0);
    }
    
    private void PlayOneShot(AudioClip clip)
    {
        if (clip)
        {
            _audioSource.PlayOneShot(clip);
        }
    }
}
