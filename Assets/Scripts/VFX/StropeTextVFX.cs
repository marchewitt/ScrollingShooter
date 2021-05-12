using UnityEngine;
using System.Collections;
using TMPro;

namespace VFX
{
    public class StropeTextVFX : MonoBehaviour
    {
        [SerializeField] private TMP_Text targetTMP;
        [SerializeField] private float strobeTimer = 0.5f;
        private Coroutine _routineRef;
        private void OnEnable()
        {
            if(targetTMP){
                _routineRef = StartCoroutine(Strobe());
            }
            else
            {
                Debug.LogWarning("targetTMP is null");
            }
        }

        private void OnDisable()
        {
            if (_routineRef != null)
            {
                StopCoroutine(_routineRef);                
            }
        }

        private IEnumerator Strobe()
        {
            string originalText = targetTMP.text;
            bool isOn = true;
            while (true)
            {
                isOn = !(isOn);
                yield return new WaitForSeconds(strobeTimer);
                targetTMP.text = isOn ? originalText : " ";
            }
        }
    }
}
