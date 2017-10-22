using UnityEngine;

namespace Assets.Scripts
{
    [UnityComponent]
    public class Rise : MonoBehaviour
    {
        [Range(0, 5)]
        public float Speed = 1.0f;

        public void Update()
        {
            transform.position = transform.position + (transform.up * Speed * Time.deltaTime);
        }
    }
}
