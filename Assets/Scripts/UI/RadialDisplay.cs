using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class RadialDisplay : MonoBehaviour
{

    private Image _radialImage;
    [SerializeField] private float maxDeltaRate = 0.02f;
    
    /// <summary>
    /// TargetValue between 0f and 1f
    /// </summary>
    public float TargetValue { get; set; }

    private float _currentValue;    
    private float CurrentValue
    {
        get => _currentValue;
        set
        {
            _currentValue = value;
            if (_radialImage)
            {
                _radialImage.fillAmount = _currentValue;
            }
        }
    }

    private void Awake()
    {
        _radialImage = this.GetComponent<Image>();
        if (_radialImage == null || _radialImage.type != Image.Type.Filled)
        {
            Debug.LogError("RadialImage is null or invalid.");
        }
    }

    private void Update() => CurrentValue = Mathf.MoveTowards(CurrentValue, TargetValue, maxDeltaRate);
}
