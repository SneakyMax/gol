using UnityEngine;

namespace Assets.Scripts
{
    [ExecuteInEditMode]
    public class StayOnGround : MonoBehaviour
    {
        public void Update()
        {
            const int diameter = 1;
            var scale = transform.localScale.x;

            transform.localPosition = new Vector3(transform.localPosition.x, diameter * scale / 2, transform.localPosition.z);
        }
    }
}