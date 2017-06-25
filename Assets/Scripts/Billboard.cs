using UnityEngine;

namespace Assets.Scripts
{
    [ExecuteInEditMode]
    public class Billboard : MonoBehaviour
    {
        public void Update()
        {
            transform.forward = -Camera.main.transform.forward;
        }
    }
}