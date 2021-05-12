using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreTextRef;

        [SerializeField] private Image livesImageRef;
        [SerializeField] private Sprite[] livesSprites;

        [SerializeField] private Canvas gameOverCanvas;
        
        private void Start()
        {
            if (gameOverCanvas)
            {
                gameOverCanvas.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("gameOverCanvas is null");
            }
            if (scoreTextRef == null)
            {
                Debug.LogError("ScoreTextRef is null");
            }
            else
            {
                scoreTextRef.text = $"Score: 0";    
            }
        }


        public void UpdateScore(int value)
        {
            scoreTextRef.text = $"Score: {value}";
        }
        
        public void UpdateLives(int value)
        {
            if(value < 0 || value> livesSprites.Length) {
                return;
            }
            var sprite = livesSprites[value];
            livesImageRef.sprite = sprite;
        }

        public void UpdateGameOver(bool activateGameOver)
        {
            gameOverCanvas.gameObject.SetActive(activateGameOver);
        }
    }
}