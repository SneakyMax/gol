using UnityEngine;

namespace Assets.Scripts
{
    public class Spin : MonoBehaviour
    {
        [Range(-720, 720)]
        public float SpinRate = 1;

        public void FixedUpdate()
        {
            transform.rotation *= Quaternion.AngleAxis(SpinRate * Time.fixedDeltaTime, Vector3.up);
        }
    }
}