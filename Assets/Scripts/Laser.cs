using UnityEngine;


public class Laser : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    
    
    private void Update()
    {
        transform.Translate(Vector3.up * (speed * Time.deltaTime));

        if (transform.position.y >= 8f)
        {
            OnDestroyLaser();
        }
    }

    public void OnDestroyLaser()
    {
        Destroy(transform.parent ? transform.parent.gameObject : gameObject); //destroy parent if TripleShot laser
    }
}
