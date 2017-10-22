using UnityEngine;

namespace Assets.Scripts
{
    public class FallingRock : MonoBehaviour
    {
        public float Speed = 0.5f;

        private Rigidbody myRigidbody;
        private Quaternion rotation;
        private float rotationSpeed;

        public void Start()
        {
            myRigidbody = GetComponent<Rigidbody>();
            rotation = Random.rotation;
            rotationSpeed = Random.value;
        }

        public void FixedUpdate()
        {
            myRigidbody.MovePosition(transform.position + new Vector3(0, -Speed * Time.fixedDeltaTime, 0));

            myRigidbody.MoveRotation(Quaternion.RotateTowards(transform.rotation, rotation * transform.rotation, rotationSpeed * 2));

            if (transform.position.y < 0)
                Destroy(gameObject);
        }
    }
}