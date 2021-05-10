using TMPro;
using UnityEngine;

namespace UI
{
    
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreTextRef;
        
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

    }
}
