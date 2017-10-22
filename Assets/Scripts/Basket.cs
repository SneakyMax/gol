using UnityEngine;

namespace Assets.Scripts
{
    [UnityComponent]
    public class Basket : MonoBehaviour
    {
        public GameObject Happy;

        public void OnCollisionEnter(Collision collision)
        {
            if (collision.rigidbody.gameObject.GetComponent<FallingRock>() != null)
            {
                ScoreTracker.Instance.CaughtRock();
                Destroy(collision.rigidbody.gameObject);
                Instantiate(Happy, transform.position, Quaternion.identity);
            }
        }
    }
}