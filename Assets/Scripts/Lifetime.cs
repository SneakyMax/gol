using UnityEngine;

namespace Assets.Scripts
{
    public class Lifetime : MonoBehaviour
    {
        public float Seconds;

        public void Start()
        {
            Destroy(gameObject, Seconds);
        }
    }
}