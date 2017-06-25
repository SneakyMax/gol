using UnityEngine;
using Vuforia;

namespace Assets.Scripts
{
    public class TrackableDisabler : MonoBehaviour, ITrackableEventHandler
    {
        public void Start()
        {
            var mTrackableBehaviour = GetComponent<TrackableBehaviour>();
            if (mTrackableBehaviour)
            {
                mTrackableBehaviour.RegisterTrackableEventHandler(this);
            }
        }

        public void OnTrackableStateChanged(TrackableBehaviour.Status previousStatus, TrackableBehaviour.Status newStatus)
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(newStatus == TrackableBehaviour.Status.DETECTED || newStatus == TrackableBehaviour.Status.TRACKED);
            }
        }
    }
}
