using System;
using System.Collections;
using Config;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;


[RequireComponent(typeof(Animator))]
public class Enemy : MonoBehaviour
{
    private Animator _animatorRef;
    private Player _playerRef;
    private BoxCollider2D _collider2DRef;
    [SerializeField] private int pointsWorth = 10;
    [Tooltip("Enemy's movement speed")]
    [SerializeField] private float baseSpeed = 4.0f;
    private float _speed;

    #region Animations
    private static readonly int OnDestroyHash = Animator.StringToHash("OnDestroy"); //Animation Trigger
    #endregion

    private void Awake()
    {
        _animatorRef = gameObject.GetComponent<Animator>();
        if(_animatorRef == null){Debug.LogError("_animatorRef was null in " + gameObject);}

        _collider2DRef = gameObject.GetComponent<BoxCollider2D>();
        if(_collider2DRef == null){Debug.LogError("_collider2DRef was null in " + gameObject);}
    }
    private void Start()
    {
        _playerRef = GameObject.FindWithTag("Player").GetComponent<Player>();
        if(_playerRef == null){Debug.LogError("_playerRef was null in " + gameObject);}

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
            if( _playerRef ) { _playerRef.AddScore(pointsWorth); }
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
        StartCoroutine(OnEnemyDestroyDelay());
        //Destroy(gameObject);
        //Respawn(transform.position.x);
    }

    /// <summary>
    /// Delaying for animation
    /// </summary>
    /// <returns></returns>
    private IEnumerator OnEnemyDestroyDelay()
    {
        _collider2DRef.enabled = false;
        _animatorRef.SetTrigger(OnDestroyHash);
        var clipLength = 2.6333f;//Not working as intended: _animatorRef.GetCurrentAnimatorStateInfo((0)).length;
        _speed = 0f;
        yield return new WaitForSeconds(clipLength);
        Respawn(transform.position.x);
    }
    
    private void Respawn(float oldX)
    {
        _collider2DRef.enabled = true;
        _speed = baseSpeed;
        var randomX = oldX;
        while (oldX - 0.1f <= randomX && randomX <= oldX + 0.1f)
        {
            //Random spawn position until not close to old x position.
            randomX = Random.Range(ScreenBounds.ScreenLeft, ScreenBounds.ScreenRight);
        }

        transform.position = new Vector3(randomX, ScreenBounds.ScreenTop + 3f, 0);
    }
}
