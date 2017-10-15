using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class GolemController : MonoBehaviour
    {
        private Vector3 Position { get { return transform.position; } }

        private GolemGameplay Gameplay { get { return GetComponent<GolemGameplay>(); } }

        public GameObject DebugTarget;

        public Transform RollBall;

        public Transform Body;

        public ParticleSystem MoveParticles;

        public ParticleSystem GatherRockParticles;

        public ParticleSystem GatherMagicParticles;

        public Transform MagicPointsParent;

        public float BodyTurnSpeed = 1;

        public float MaxSpeed = 10;

        public float Acceleration = 1;

        public float SlowdownDistance = 2;

        public float StopDistance = 0.5f;

        public float DampeningAmount = 0.5f;

        public float StopTurningAt = 1;

        public float MinMoveParticles = 0.1f;

        public float MaxDistanceFromCenter = 1.0f;

        [Header("Roll Ball")]
        public float MinRollScale = 0.5f;

        public float MaxRollScale = 2.0f;

        [Range(0, 1)]
        public float StartOverflowing = 0.8f;

        public float StartUnderflowing = 0.2f;

        private float CurrentSpeed { get { return currentVelocity.magnitude; } }

        [Header("Body"), Range(0, 1)]
        public float StartOverMagic = 0.8f;

        [Range(0, 1)]
        public float StartUnderMagic = 0.2f;

        [Header("Magic")]
        public float MinMagicSize = 1;

        public float MaxMagicSize = 5.5f;

        private Vector3 currentVelocity;

        private Quaternion targetFacing;

        private Vector3? targetPosition;

        private GameObject debugTargetPosition;

        private float rockScale;
        private float bodyScale;

        public bool IsStopped { get; private set; }

        [Range(1, 10)]
        public float OverRotation = 1;

        private IList<ParticleSystem> magicPoints;

        public Animator Animator { get; private set; }

        public ParticleSystem ExplodeDust;

        public ParticleSystem ExplodeMagic;

        public float MinBodySize = 0.8f;

        public float MaxBodySize = 1.1f;

        public void Start()
        {
            magicPoints = MagicPointsParent.GetComponentsInChildren<ParticleSystem>();
            Animator = GetComponentInChildren<Animator>();
        }

        public void OnEnable()
        {
            StartCoroutine(EverySecond());
            StartCoroutine(EveryQuarterSecond());
        }

        public IEnumerator EverySecond()
        {
            yield return new WaitForSeconds(1);
        }

        public IEnumerator EveryQuarterSecond()
        {
            while (true)
            {
                AdjustBallMaterial();
                AdjustBodyMaterial();

                yield return new WaitForSeconds(0.25f);
            }
        }

        private void AdjustBallMaterial()
        {
            var ballMaterial = RollBall.GetComponent<MeshRenderer>().material as ProceduralMaterial;
            if (ballMaterial == null)
                return;

            var onlyHigh = Mathf.Clamp(Gameplay.CurrentRockPercent, StartOverflowing, 1.0f);
            var overflowing = Utils.ChangeScale(StartOverflowing, 1, 0, 1, onlyHigh);

            if (overflowing > 0.0f)
            {
                ballMaterial.SetProceduralFloat("overflowing", overflowing);
                ballMaterial.RebuildTextures();
            }
        }

        private void AdjustBodyMaterial()
        {
            var meshRenderer = (Renderer)Body.GetComponentInChildren<SkinnedMeshRenderer>() ?? Body.GetComponentInChildren<MeshRenderer>();
            var bodyMaterial = meshRenderer == null ? null : meshRenderer.material as ProceduralMaterial;
            if (bodyMaterial == null)
                return;

            var onlyHigh = Mathf.Clamp(Gameplay.CurrentMagicPercent, StartOverMagic, 1.0f);
            var onlyLow = Mathf.Clamp(Gameplay.CurrentMagicPercent, 0.0f, StartUnderMagic);

            var overMagic = Utils.ChangeScale(StartOverMagic, 1, 0, 1, onlyHigh);
            var underMagic = 1.0f - Utils.ChangeScale(0, StartUnderMagic, 0, 1, onlyLow);
            
            bodyMaterial.SetProceduralFloat("overAmount", overMagic);
            bodyMaterial.SetProceduralFloat("drainedAmount", underMagic);
            bodyMaterial.RebuildTextures();
        }

        public void SetTargetPosition(Vector3 position)
        {
            targetPosition = position;
        }

        public void Update()
        {
            if (IsStopped)
                return;

            AdjustParticleSystem();
            AdjustBallSize();
            AdjustBodySize();
        }

        private void AdjustParticleSystem()
        {
            var emission = MoveParticles.emission;
            emission.enabled = currentVelocity.magnitude >= MinMoveParticles;

            foreach (var magicPoint in magicPoints)
            {
                var main = magicPoint.main;
                main.startSize = Mathf.Lerp(MinMagicSize, MaxMagicSize, Gameplay.CurrentMagicPercent);
            }
        }

        private void AdjustBallSize()
        {
            rockScale = Mathf.Lerp(MinRollScale, MaxRollScale, Gameplay.CurrentRockPercent);
            RollBall.transform.localScale = new Vector3(rockScale, rockScale, rockScale);
        }

        private void AdjustBodySize()
        {
            bodyScale = Mathf.Lerp(MinBodySize, MaxBodySize, Gameplay.CurrentRockPercent);
            Body.transform.localScale = new Vector3(bodyScale, bodyScale, bodyScale);
        }

        public void FixedUpdate()
        {
            if (IsStopped)
                return;

            Move();
            RotateBall();
            RotateTorso();
        }

        private void Move()
        {
            if (DistanceToTarget < StopDistance)
            {
                StopInstantly();
            }
            else if (CurrentSpeed > MaxSpeedForDistance)
            {
                SlowDown();
            }
            else
            {
                AccelerateTowardsTarget();
            }

            var distanceFromParent = transform.localPosition.DistanceTo(transform.parent.localPosition);
            if (distanceFromParent >= MaxDistanceFromCenter)
            {
                transform.localPosition = (transform.localPosition - transform.parent.localPosition).normalized *
                                          MaxDistanceFromCenter;
            }

            Dampening();

            transform.position += currentVelocity * Time.fixedDeltaTime;
        }

        private void RotateBall()
        {
            const int ballSize = 1;
            var movement = currentVelocity.magnitude * Time.fixedDeltaTime;
            var circumference = ballSize * RollBall.localScale.x * Mathf.PI;
            var rotationPercent = movement / circumference;

            var left = Vector3.Cross(currentVelocity.normalized, Vector3.up);
            var rotation = Quaternion.AngleAxis(-rotationPercent * 360 * OverRotation, left);

            RollBall.rotation = rotation * RollBall.rotation;
        }

        private void RotateTorso()
        {
            if (targetPosition == null)
                return;

            var currentFacing = Body.rotation.eulerAngles.y;
            var faceTarget = Quaternion.LookRotation((targetPosition.Value - Position).normalized, Vector3.up).eulerAngles.y;

            var turnRightDegrees = (currentFacing - faceTarget).NormalizeDegrees();
            var turnLeftDegrees = (faceTarget - currentFacing).NormalizeDegrees();

            if (turnRightDegrees < StopTurningAt || turnLeftDegrees < StopTurningAt)
                return;

            var turnDegrees = Mathf.Min(turnLeftDegrees, turnRightDegrees);

            var turnAmount = BodyTurnSpeed * Time.fixedDeltaTime * Mathf.Max(0.5f, ( turnDegrees / 180 ) * 2);

            var directionChange = turnRightDegrees < turnLeftDegrees ?
                Quaternion.AngleAxis(Mathf.Min(-turnAmount, turnRightDegrees), Vector3.up) : 
                Quaternion.AngleAxis(Mathf.Min(turnAmount, turnLeftDegrees), Vector3.up);

            Body.rotation = directionChange * Body.rotation;
        }

        private void Dampening()
        {
            currentVelocity -= currentVelocity * DampeningAmount * Time.fixedDeltaTime;
        }

        private void AccelerateTowardsTarget()
        {
            if (targetPosition == null)
                return;

            var directionToTarget = (targetPosition.Value - Position).normalized;
            currentVelocity += directionToTarget * (Time.fixedDeltaTime * Acceleration);
        }

        private void StopInstantly()
        {
            currentVelocity = Vector3.zero;
        }

        private void SlowDown()
        {
            
        }

        public void Stop()
        {
            IsStopped = true;
            StopAllCoroutines();
        }

        private float MaxSpeedForDistance
        {
            get { return DistanceToTarget > SlowdownDistance ? MaxSpeed : MaxSpeed * (DistanceToTarget / SlowdownDistance); }
        }

        private float DistanceToTarget
        {
            get { return Vector3.Distance(Position, targetPosition ?? new Vector3()); }
        }

        public void ClearTargetPosition()
        {
            targetPosition = null;
        }
    }
}