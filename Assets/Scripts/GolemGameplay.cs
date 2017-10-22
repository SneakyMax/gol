using System;
using System.Diagnostics.CodeAnalysis;
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
        public static GolemGameplay Instance { get; private set; }

        [AssignedInUnity]
        public int MaxRock = 1000;

        [AssignedInUnity]
        public int MaxMagic = 1000;

        [AssignedInUnity]
        public int MaxFun = 1000;

        [Range(1, 100)]
        public float MinDecay = 10;
        
        [Range(1, 100)]
        public float MaxDecay = 50;

        [Range(5, 200)]
        public float MinConsume = 50;

        [Range(5, 200)]
        public float MaxConsume = 120;

        [AssignedInUnity]
        public float DeathBuffer = 1f;

        [Range(0, 1)]
        public float UpperSadStart = 0.85f;

        [Range(0, 1)]
        public float LowerSadStart = 0.15f;

        private float rockDecay;
        private float magicDecay;
        private float funDecay;

        private float rockConsume;
        private float magicConsume;
        private float funConsume;

        [AssignedInUnity]
        public float CurrentRock;

        [AssignedInUnity]
        public float CurrentMagic;

        [AssignedInUnity]
        public float CurrentFun;

        public float CurrentRockPercent { get { return CurrentRock / MaxRock; } }
        public float CurrentMagicPercent { get { return CurrentMagic / MaxMagic; } }
        public float CurrentFunPercent { get { return CurrentFun / MaxFun; } }

        private bool consumingRock;
        private bool consumingMagic;

        public bool IsStopped { get; set; }

        public GolemController Controller { get; private set; }

        [AssignedInUnity]
        public GameObject ShowMoreMagic;

        [AssignedInUnity]
        public GameObject ShowLessMagic;

        [AssignedInUnity]
        public GameObject ShowMoreRock;

        [AssignedInUnity]
        public GameObject ShowLessRock;

        [AssignedInUnity]
        public GameObject ShowSad;

        [AssignedInUnity]
        public Transform HappyStartPosition;

        [AssignedInUnity]
        public GameObject HappyPrefab;

        [AssignedInUnity]
        public bool DeathEnabled;

        [AssignedInUnity]
        public int GameplayLengthSeconds = 120;

        private bool maybeDead;

        public TimeSpan TimeRemaining { get; private set; }

        private float startTime;

        public float RockHappiness;
        public float MagicHappiness;
        public float FunHappiness;

        public bool GameStarted { get; private set; }

        public float Happiness { get; private set; }
        public int PlayCount { get; private set; }

        public float LastSmash { get; set; }


        public void Awake()
        {
            Controller = GetComponent<GolemController>();
            Instance = this;
            IsStopped = true;
        }

        public void Start()
        {
            CurrentRock = MaxRock / 2.0f; // + Random.Range(-MaxRock / 5.0f, MaxRock / 5.0f);
            CurrentMagic = MaxMagic / 2.0f; // + Random.Range(-MaxMagic / 5.0f, MaxMagic / 5.0f);

            rockDecay = Random.Range(MinDecay, MaxDecay);
            magicDecay = Random.Range(MinDecay, MaxDecay);

            rockConsume = Random.Range(MinConsume, MaxConsume);
            magicConsume = Random.Range(MinConsume, MaxConsume);
            
            LastSmash = Time.time;
        }

        public void StartGameplay()
        {
            startTime = Time.time;
            GameStarted = true;
            IsStopped = false;
            ScoreTracker.Instance.StartRound();
        }

        public void Update()
        {
            if (IsStopped)
                return;

            ConsumeAndDecay();
            CheckDeath();
            CheckSad();
            UpdateTimeRemaining();
            CheckEndOfGame();
            CalculateHappiness();
        }

        private void CheckEndOfGame()
        {
            if (TimeRemaining.Ticks >= 0)
                return;

            IsStopped = true;
            GameStarted = false;
            ScoreTracker.Instance.EndRound();
            GetComponent<GolemAI>().StateMachine.ChangeState<EndGame>();
        }

        private void UpdateTimeRemaining()
        {
            TimeRemaining = TimeSpan.FromSeconds(GameplayLengthSeconds - (Time.time - startTime));
        }


        private void CheckSad()
        {
            var moreRock = CurrentRockPercent < LowerSadStart;
            var lessRock = CurrentRockPercent > UpperSadStart;
            var moreMagic = CurrentMagicPercent < LowerSadStart;
            var lessMagic = CurrentMagicPercent > UpperSadStart;

            if (ShowMoreRock.activeInHierarchy != moreRock)
                ShowMoreRock.SetActive(moreRock);

            if (ShowLessRock.activeInHierarchy != lessRock)
                ShowLessRock.SetActive(lessRock);

            if (ShowMoreMagic.activeInHierarchy != moreMagic)
                ShowMoreMagic.SetActive(moreMagic);

            if (ShowLessMagic.activeInHierarchy != lessMagic)
                ShowLessMagic.SetActive(lessMagic);
        }

        private void CalculateHappiness()
        {
            RockHappiness = CalculateRockHappiness();
            MagicHappiness = CalculateMagicHappiness();
            FunHappiness = CalculateFunHappiness();

            var needs = Mathf.Clamp(Mathf.Min(RockHappiness, MagicHappiness), 0, 1);

            var needsWeight = Mathf.Lerp(0.75f, 1, 1.0f - Mathf.Pow(needs, 2));
            var otherWeight = 1.0f - needsWeight;

            var needsNum = needs * needsWeight;
            var other = FunHappiness * otherWeight;

            Happiness = needsNum + other;
        }
        
        private float CalculateRockHappiness()
        {
            var distanceFromSweetSpot = Mathf.Abs((MaxRock / 2.0f) - CurrentRock);

            // TODO lerp
            return 1.0f - (distanceFromSweetSpot / (MaxRock / 2.0f));
        }

        private float CalculateMagicHappiness()
        {
            var sweetSpotRange = 100;
            var distanceFromSweetSpot = Mathf.Abs((MaxMagic / 2.0f) - CurrentMagic);
            if (distanceFromSweetSpot < sweetSpotRange)
                return 1.0f;

            // TODO lerp
            return 1.0f - (distanceFromSweetSpot / (MaxMagic / 2.0f));
        }

        private float CalculateFunHappiness()
        {
            var timeSinceLastSmash = TimeSpan.FromSeconds(Time.time - LastSmash);
            var idealSmashRate = (float)GameplayLengthSeconds / ScoreTracker.Instance.NumberOfPlayForMaxScore;
            var funHappiness = 1.0f - Mathf.Clamp((float)timeSinceLastSmash.TotalSeconds / (idealSmashRate * 5), 0, 1);

            // Inverted quadratic
            var curved = 1.0f - Mathf.Pow(1.0f - funHappiness, 2);
            return curved;
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
            if (!DeathEnabled)
                return DeadType.NotDead;

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
                        ReallyDead(bufferDeadness);
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

        public void OnSmash()
        {
            PlayCount += 1;
            LastSmash = Time.time;
            Happy();
        }

        private void Happy()
        {
            Instantiate(HappyPrefab, HappyStartPosition.position, Quaternion.identity);
        }

        public void SeenSomething()
        {
            if (GameStarted == false)
            {
                StartGameplay();
            }
        }
    }
}
