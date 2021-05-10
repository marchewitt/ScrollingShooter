using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreTextRef;

        [SerializeField] private Image livesImageRef;
        [SerializeField] private Sprite[] livesSprites;

        [SerializeField] private Canvas gameOverCanvas;
        // Start is called before the first frame update
        private void Start()
        {
            if (scoreTextRef == null)
            {
                Debug.LogError("ScoreTextRef is null");
            }
            scoreTextRef.text = $"Score: 0";
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
