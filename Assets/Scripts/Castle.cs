using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Assets.Scripts
{
    [UnityComponent]
    public class Castle : MonoBehaviour
    {
        private class BodyInfo
        {
            public Rigidbody Rigidbody { get; set; }
            public Vector3 StartPosition { get; set; }
            public Quaternion StartRotation { get; set; }
            public GameObject GameObject { get; set; }
        }

        private IList<BodyInfo> allBodies;

        public float ResetTime = 1.0f;
        public float TimeUntilReset = 2.0f;

        public bool CanBeSmashed { get; private set; }

        public Castle()
        {
            allBodies = new List<BodyInfo>();
        }

        [UnityMessage]
        public void Awake()
        {
            foreach (Transform child in transform)
            {
                var newCollider = child.gameObject.AddComponent<MeshCollider>();
                var mesh = child.GetComponent<MeshFilter>().mesh;
                newCollider.inflateMesh = true;
                newCollider.convex = true;
                newCollider.sharedMesh = mesh;

                var newBody = child.gameObject.AddComponent<Rigidbody>();
                newBody.useGravity = false;

                allBodies.Add(new BodyInfo
                {
                    Rigidbody = newBody,
                    StartPosition = child.transform.localPosition,
                    StartRotation = child.transform.localRotation,
                    GameObject = child.gameObject
                });

                newBody.isKinematic = true;
            }
        }

        [UnityMessage]
        public void Start()
        {
            CanBeSmashed = true;
        }

        public void ResetPieces()
        {
            foreach (var body in allBodies)
            {
                body.Rigidbody.isKinematic = true;
                body.GameObject.transform.DOLocalMove(body.StartPosition, ResetTime).SetEase(Ease.InOutSine);
                body.GameObject.transform.DOLocalRotate(body.StartRotation.eulerAngles, ResetTime).SetEase(Ease.InOutSine);
            }
        }

        public void Smash()
        {
            if (CanBeSmashed == false)
                return;

            EnablePhysics();
            CanBeSmashed = false;
            GolemGameplay.Instance.OnSmash();

            Delay.Of(TimeUntilReset, () =>
            {
                ResetPieces();
                Delay.Of( ResetTime, () =>
                {
                    CanBeSmashed = true;
                });
            });
        }

        public void EnablePhysics()
        {
            foreach (var body in allBodies)
            {
                body.Rigidbody.isKinematic = false;
            }
        }

        [UnityMessage]
        public void FixedUpdate()
        {
            foreach (var body in allBodies)
            {
                if (body.Rigidbody.isKinematic)
                    return;
                body.Rigidbody.AddForce(transform.up * -9.8f, ForceMode.Acceleration);
            }
        }
    }
}
