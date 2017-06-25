using System;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    public enum Need
    {
        Rock,
        Magic,
        Kitten
    }

    public enum DeadType
    {
        NotDead,
        NotEnoughRock,
        TooMuchRock,
        NotEnoughMagic,
        TooMuchMagic
    }

    public class GolemGameplay : MonoBehaviour
    {
        public int MaxRock = 1000;

        public int MaxMagic = 1000;

        [Range(1, 100)]
        public float MinDecay = 10;
        
        [Range(1, 100)]
        public float MaxDecay = 50;

        [Range(5, 200)]
        public float MinConsume = 50;

        [Range(5, 200)]
        public float MaxConsume = 120;

        public float DeathBuffer = 1f;

        [Range(0, 1)]
        public float UpperSadStart = 0.85f;

        [Range(0, 1)]
        public float LowerSadStart = 0.15f;

        private float rockDecay;
        private float magicDecay;

        private float rockConsume;
        private float magicConsume;

        public float CurrentRock;
        public float CurrentMagic; 

        public float CurrentRockPercent { get { return CurrentRock / MaxRock; } }
        public float CurrentMagicPercent { get { return CurrentMagic / MaxMagic; } }

        private bool consumingRock;
        private bool consumingMagic;

        public bool IsStopped { get; set; }

        public GolemController Controller { get; private set; }

        public GameObject ShowSad;

        private bool maybeDead;

        public void Awake()
        {
            Controller = GetComponent<GolemController>();
        }

        public void Start()
        {
            CurrentRock = MaxRock / 2.0f + Random.Range(-MaxRock / 5.0f, MaxRock / 5.0f);
            CurrentMagic = MaxMagic / 2.0f + Random.Range(-MaxMagic / 5.0f, MaxMagic / 5.0f);

            rockDecay = Random.Range(MinDecay, MaxDecay);
            magicDecay = Random.Range(MinDecay, MaxDecay);

            rockConsume = Random.Range(MinConsume, MaxConsume);
            magicConsume = Random.Range(MinConsume, MaxConsume);
        }

        public void Update()
        {
            if (IsStopped)
                return;

            ConsumeAndDecay();
            CheckDeath();
            CheckSad();
        }

        private void CheckSad()
        {
            var isSad =
                CurrentRockPercent < LowerSadStart ||
                CurrentRockPercent > UpperSadStart ||
                CurrentMagicPercent < LowerSadStart ||
                CurrentMagicPercent > UpperSadStart;

            if(ShowSad.activeInHierarchy != isSad)
                ShowSad.SetActive(isSad);
        }

        private void ConsumeAndDecay()
        {
            if (consumingRock)
            {
                CurrentRock += rockConsume * Time.deltaTime;
            }
            else
            {
                CurrentRock -= rockDecay * Time.deltaTime;
            }
            CurrentRock = Mathf.Clamp(CurrentRock, 0, MaxRock);

            if (consumingMagic)
            {
                CurrentMagic += magicConsume * Time.deltaTime;
            }
            else
            {
                CurrentMagic -= magicDecay * Time.deltaTime;
            }
            CurrentMagic = Mathf.Clamp(CurrentMagic, 0, MaxMagic);
        }

        private DeadType GetDeadType()
        {
            if (CurrentRock >= MaxRock)
                return DeadType.TooMuchRock;

            if (CurrentRock <= 0f)
                return DeadType.NotEnoughRock;

            if (CurrentMagic >= MaxMagic)
                return DeadType.TooMuchMagic;

            if (CurrentMagic <= 0f)
                return DeadType.NotEnoughMagic;

            return DeadType.NotDead;
        }

        private void CheckDeath()
        {
            if (maybeDead)
                return;

            var deadness = GetDeadType();
            if (deadness != DeadType.NotDead)
            {
                maybeDead = true;
                Delay.Of(DeathBuffer, () =>
                {
                    var bufferDeadness = GetDeadType();
                    if (bufferDeadness != DeadType.NotDead)
                    {
                        ReallyDead( bufferDeadness);
                    }
                    maybeDead = false;
                });
            }
        }

        private void ReallyDead(DeadType death)
        {
            ShowSad.SetActive(false);
            switch (death)
            {
                case DeadType.NotEnoughRock:
                    DeadFallOver();
                    break;
                case DeadType.TooMuchRock:
                    DeadExplosion();
                    break;
                case DeadType.NotEnoughMagic:
                    DeadCrumble();
                    break;
                case DeadType.TooMuchMagic:
                    DeadMagicExplosion();
                    break;
            }
        }

        private void DeadMagicExplosion()
        {
            Delay.Of(0.35f, () =>
            {
                Controller.ExplodeMagic.gameObject.SetActive(true);
                Controller.Body.GetComponent<StayOnTopOfBall>().enabled = false;
                Controller.Body.DOMoveY(0, 0.5f);
            });

            Controller.Animator.SetTrigger("Dead-Explosion");
            GameOver();
        }

        private void DeadCrumble()
        {
            Controller.Animator.SetTrigger("Dead-FallOver");
            GameOver();
        }

        private void DeadFallOver()
        {
            Controller.Animator.SetTrigger("Dead-FallOver");
            GameOver();
        }

        private void DeadExplosion()
        {
            Delay.Of(0.35f, () =>
            {
                Controller.ExplodeDust.gameObject.SetActive(true);
                Controller.Body.GetComponent<StayOnTopOfBall>().enabled = false;
                Controller.Body.DOMoveY(0, 0.5f);
            } );

            Controller.Animator.SetTrigger("Dead-Explosion");
            GameOver();
        }

        private void GameOver()
        {
            GetComponent<GolemAI>().StateMachine.ChangeState<GolemDead>();
            GetComponent<GolemController>().Stop();
            Stop();
            StopAllCoroutines();

            Delay.Of(1, () =>
            {
                World.Instance.Dead();
            });
        }

        public void Stop()
        {
            IsStopped = true;
        }

        public void StartConsume(Need need)
        {
            consumingRock = need == Need.Rock;
            consumingMagic = need == Need.Magic;
        }

        public void StopConsume()
        {
            consumingRock = false;
            consumingMagic = false;
        }
    }
}
