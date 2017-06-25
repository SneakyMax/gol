using UnityEngine;

namespace Assets.Scripts
{
    [ExecuteInEditMode]
    public class StayOnTopOfBall : MonoBehaviour
    {
        public GameObject Ball;

        public float Offset = 0;
 
        public void Update()
        {
            if (Ball == null)
                return;

            const int diameter = 1;
            var ballScale = Ball.transform.localScale.x;

            transform.localPosition = new Vector3(transform.localPosition.x, diameter * ballScale + Offset, transform.localPosition.z);
        }
    }
}