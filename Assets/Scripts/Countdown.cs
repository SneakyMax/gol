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
            var timeRemaining = GolemGameplay.Instance != null ? GolemGameplay.Instance.TimeRemaining : TimeSpan.Zero;
            GetComponentInChildren<Text>().text = String.Format("{0:0}:{1:00}", timeRemaining.Minutes, timeRemaining.Seconds);
        }
    }
}