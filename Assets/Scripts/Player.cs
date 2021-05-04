using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public class Player : MonoBehaviour
{
    [Tooltip("Players movement speed")]
    [SerializeField] private float speed = 3.5f;

    [Header("Laser Settings")]
    [SerializeField] private Transform laserSpawnPosition;
    [SerializeField] private GameObject laserPrefab;
    
    //Screen Bounds
    private const float ScreenRight = 8.5f;
    private const float ScreenLeft = -8.5f;
    private const float ScreenTop = 5.8f;
    private const float ScreenBottom = -3.8f;

    public void Start()
    {
        transform.position = Vector3.zero;
    }


    private void Update()
    {
        
        #region Calculate Movement Off Input

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput  = Input.GetAxis("Vertical");
        
        var moveDirection = new Vector3(horizontalInput, verticalInput, 0).normalized;
        
        transform.Translate((moveDirection * (speed * Time.deltaTime)));

        #region Wrap And Clamp Screen Bounds

        var position = transform.position;
        if (position.x > ScreenRight)
        {
            position = new Vector3(ScreenLeft, position.y, 0);
        } 
        else if (transform.position.x < ScreenLeft)
        {
            position = new Vector3(ScreenRight, position.y, 0);
        }

        transform.position = new Vector3(
            position.x,
            Mathf.Clamp(position.y, ScreenBottom, ScreenTop),
            0);
        
        #endregion

        #endregion
        
        #region Shoot Input

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Instantiate(laserPrefab, laserSpawnPosition.position, Quaternion.identity);
        }
        #endregion
        
    }
}
