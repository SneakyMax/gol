using UnityEngine;

namespace Assets.Scripts
{
    public class GolObject : MonoBehaviour
    {
        public string Name;

        public bool Active { get { return gameObject.activeInHierarchy; } }

        public Vector3 Position { get { return gameObject.transform.position; } }

        public Vector3 FlatPosition { get { return gameObject.GolemLevelPosition(); } }

        public void Start()
        {
            World.Register(this, Name);
        }
    }
}
