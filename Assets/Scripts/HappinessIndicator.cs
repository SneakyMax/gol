using UnityEngine;

namespace Assets.Scripts
{
    [UnityComponent, RequireComponent(typeof(RectTransform))]
    public class HappinessIndicator : MonoBehaviour
    {
        public float XMin;
        public float XMax;

        [UnityMessage]
        public void Update()
        {
            var xPosition = GetXPosition();

            var pos = (RectTransform) transform;
            pos.anchoredPosition = new Vector2( xPosition, pos.anchoredPosition.y );
        }

        private float GetXPosition()
        {
            var happiness = GolemGameplay.Instance != null ? GolemGameplay.Instance.Happiness : 0.0f;
            return Mathf.Lerp(XMin, XMax, happiness);
        }
    }
}