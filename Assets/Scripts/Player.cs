using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Player : MonoBehaviour
{
    [Tooltip("Players movement speed")]
    [SerializeField] float speed = 3.5f;
   
    
    //Screen Bounds
    private readonly float _screenRight = 8.5f;
    private readonly float _screenLeft = -8.5f;
    private readonly float _screenTop = 5.8f;
    private readonly float _screenBottom = -3.8f;

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
        if (position.x > _screenRight)
        {
            position = new Vector3(_screenLeft, position.y, 0);
        } 
        else if (transform.position.x < _screenLeft)
        {
            position = new Vector3(_screenRight, position.y, 0);
        }

        transform.position = new Vector3(
            position.x,
            Mathf.Clamp(position.y, _screenBottom, _screenTop),
            0);
        
        #endregion

        #endregion
    }
}
