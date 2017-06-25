using System;
using UnityEngine;

namespace Assets.Scripts
{
    public class Bob : MonoBehaviour
    {
        [Range(0, 5)]
        public float Rate = 1f;

        [Range(0, 2)]
        public float Distance = 1f;

        private Vector3 startPosition;

        public void Start()
        {
            startPosition = transform.localPosition;
        }

        public void FixedUpdate()
        {
            transform.localPosition = startPosition + new Vector3(0, Mathf.Sin(Time.time * Rate) * Distance, 0);
        }
    }
}