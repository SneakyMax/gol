using UnityEngine;

namespace Assets.Scripts
{
    [UnityComponent, RequireComponent(typeof(Rigidbody)), ExecuteInEditMode]
    public class SyncPosition : MonoBehaviour
    {
        [AssignedInUnity]
        public Transform Target;

        private Rigidbody myRigidbody;
        
        public void Start()
        {
            myRigidbody = GetComponent<Rigidbody>();
        }

        public void Update()
        {
#if UNITY_EDITOR
            transform.position = Target.transform.position;
#endif
        }
        
        public void FixedUpdate()
        {
            myRigidbody.MovePosition(Target.transform.position);
        }
    }
}