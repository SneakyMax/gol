using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class Delay : MonoBehaviour
    {
        private static Delay instance;

        public void Awake()
        {
            instance = this;
        }

        public static void Of(float seconds, Action action)
        {
            instance.StartCoroutine(DelayOf(seconds, action));
        }

        private static IEnumerator DelayOf(float seconds, Action action)
        {
            yield return new WaitForSeconds(seconds);
            action();
        }
    }
}