using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Image))]
    public class FillDisplay : MonoBehaviour
    {
        protected Image FillImage;
        [SerializeField] protected float maxDeltaRate = 0.2f;
    
        /// <summary>
        /// TargetValue between 0f and 1f
        /// </summary>
        public float TargetValue { get; set; }

        private float _currentValue;

        protected float CurrentValue
        {
            get => _currentValue;
            set
            {
                _currentValue = value;
                if (FillImage)
                {
                    FillImage.fillAmount = _currentValue;
                }
            }
        }

        private void Awake()
        {
            FillImage = this.GetComponent<Image>();
            if (FillImage == null || FillImage.type != Image.Type.Filled)
            {
                Debug.LogError("FillImage is null or invalid.");
            }
        }

        private void Update() => CurrentValue = Mathf.MoveTowards(CurrentValue, TargetValue, maxDeltaRate);
    }
}
