using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Image))]
    public class FillDisplayGradiant : FillDisplay
    {
        private Color _currentColor;
        [SerializeField] private Gradient gradientFill;

        private  Color CurrentColor
        {
            get => _currentColor;
            set 
            {
                _currentColor = value;
                if (FillImage)
                {
                    FillImage.color = _currentColor;
                }
            }
        }

        private void Update()
        {
            CurrentValue = Mathf.MoveTowards(CurrentValue, TargetValue, maxDeltaRate);
            CurrentColor = gradientFill.Evaluate(CurrentValue);
        }
    }
}