using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class Countdown : MonoBehaviour
    {
        private bool enabled;
        private float startTime;

        public void OnEnable()
        {
            enabled = true;
            startTime = Time.time;
        }

        public void Update()
        {
            GetComponentInChildren<Text>().text = String.Format("{0:0.0}", 5.0f - (Time.time - startTime));
        }
    }
}