using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    
    public class UIManager : MonoBehaviour
    {
        [Header("Score Panel")]
        [SerializeField] private TMP_Text scoreTextRef;
        
        [Header("Ammo Panel")]
        [SerializeField] private TMP_Text ammoCountTextRef;
        
        [Header("Engine Panel")]
        [SerializeField] private TMP_Text engineHeatTextRef;
        [SerializeField] private FillDisplay heatFillRef;
        [SerializeField] private GameObject overHeatedRef;

        [Header("Lives Panel")]
        [SerializeField] private Image livesImageRef;
        [SerializeField] private Sprite[] livesSprites;

        [Header("GameOver Canvas")]
        [SerializeField] private Canvas gameOverCanvas;

        private bool _isEngineOverheated = false;
        public bool IsEngineOverheated
        {
            get => _isEngineOverheated;
            set
            {
                _isEngineOverheated = value;
                
                if (overHeatedRef)
                {
                    overHeatedRef.SetActive(_isEngineOverheated);
                }
            }
        }

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
            if (scoreTextRef)
            {
                scoreTextRef.text = $"Score: 0";
            }
            else
            {
                Debug.LogError("ScoreTextRef is null");
            }

            if (engineHeatTextRef)
            {
                engineHeatTextRef.text = $"Heat:\n0%";    
            }
            else
            {
                Debug.LogError("engineHeatTextRef is null");
            }
            if (overHeatedRef)
            {
                overHeatedRef.SetActive(false);
            }
            else
            {
                Debug.LogWarning("No overHeated UI object set. Did you forget a reference?");
            }

            if (ammoCountTextRef)
            {
                //Updated by player
            }
            else
            {
                Debug.LogError("AmmoCountTextRef is null");
            }
        }


        public void UpdateScore(int value)
        {
            scoreTextRef.text = $"Score: {value}";
        }
        
        public void UpdateLives(int value)
        {
            if(value < 0 || value >= livesSprites.Length) {
                return;
            }
            var sprite = livesSprites[value];
            livesImageRef.sprite = sprite;
        }

        public void UpdateGameOver(bool activateGameOver)
        {
            gameOverCanvas.gameObject.SetActive(activateGameOver);
        }

        public void UpdateEngineHeat(float engineHeat)
        {
            heatFillRef.TargetValue = engineHeat;
            var engineHeatPercentage = engineHeat * 100;
            engineHeatTextRef.text = $"Heat:\n{engineHeatPercentage}%";
            //if value is over x, y, or z, change color
        }

        public void UpdateAmmo(int currentAmmo, int maxAmmo) => ammoCountTextRef.text = $"{currentAmmo} / {maxAmmo}";
    }
}