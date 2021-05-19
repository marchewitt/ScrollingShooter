using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private bool _isGameOver;

   
    // Update is called once per frame
    void Update()
    {
        if (_isGameOver)
        {
            if(Input.GetKeyDown(KeyCode.R)){
                SceneManager.LoadScene("Game");
            }else if (Input.GetKeyDown(KeyCode.Escape))
            {
                SceneManager.LoadScene("MainMenu");
            }
        }
    }

    public void GameOver()
    {
        _isGameOver = true;
    }

    
}
