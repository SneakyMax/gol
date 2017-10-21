using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    [UnityComponent, RequireComponent(typeof(Text))]
    public class TimeRemaining : MonoBehaviour
    {
        private Text text;

        public void Start()
        {
            text = GetComponent<Text>();
        }

        public void Update()
        {
            text.text = String.Format("{0:0}:{1:00}",
                GolemGameplay.Instance.TimeRemaining.Minutes,
                GolemGameplay.Instance.TimeRemaining.Seconds);
        }
    }
}
