using System.Collections;
using UnityEngine;

namespace Helpers
{
    /// <summary>
    /// Useful for VFX and other elements that need to play then destroy themselves on a timer
    /// </summary>
    public class DestroyOnTimer : MonoBehaviour
    {
        [Tooltip("Value is in Seconds, can not be 0 or negative")]
        [SerializeField] private float destroyTimer = 0f;  
        
        private void Awake()
        {
            if (destroyTimer <= 0)
            {
                Debug.LogError($"destroyTimer on {gameObject} can not be 0 or negative");
            }
            else
            {
                Destroy(gameObject, destroyTimer);
            }
        }
    }
}
