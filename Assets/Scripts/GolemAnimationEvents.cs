using System;
using UnityEngine;

namespace Assets.Scripts
{
    [UnityComponent]
    public class GolemAnimationEvents : MonoBehaviour
    {
        public static GolemAnimationEvents Instance { get; private set; }

        public event Action Smash;

        public void Awake()
        {
            Instance = this;
        }

        public void SmashHappened()
        {
            if (Smash != null)
                Smash();
        }
    }
}