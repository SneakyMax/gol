using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ScoreTracker : MonoBehaviour
    {
        public static ScoreTracker Instance { get; private set; }

        public bool IsRunning { get; set; }
        
        public float LastSmashTime { get; set; }

        [AssignedInUnity]
        public GameObject ScoreScreen;

        [AssignedInUnity]
        public Text ScoreText;

        [AssignedInUnity]
        public Image GradeImage;

        public Sprite S;
        public Sprite APlus;
        public Sprite A;
        public Sprite AMinus;
        public Sprite BPlus;
        public Sprite B;
        public Sprite BMinus;
        public Sprite CPlus;
        public Sprite C;
        public Sprite CMinus;
        public Sprite DPlus;
        public Sprite D;
        public Sprite DMinus;
        public Sprite FPlus;
        public Sprite F;
        public Sprite FMinus;

        public Text CatchForNewGame;
        private int numCaught;
        public int NumToCatch = 5;

        public float RockRange = 5;
        public float RockHeight = 5;
  
        public enum Grade
        {
            S,
            APlus,
            A,
            AMinus,
            BPlus,
            B,
            BMinus,
            CPlus,
            C,
            CMinus,
            DPlus,
            D,
            DMinus,
            FPlus,
            F,
            FMinus
        }

        public class Problem
        {
            public int Id { get; private set; }
            public int Weight { get; private set; }
            public string Text { get; private set; }
            public Problem(int id, int weight, string text)
            {
                Id = id;
                Weight = weight;
                Text = text;
            }
        }

        public static class Problems
        {
            public static Problem MAGIC_LOW = new Problem(0, 10, "Did not feed enough magic");
            public static Problem MAGIC_HIGH = new Problem(1, 10, "Fed too much magic.");
            public static Problem ROCK_LOW = new Problem(2, 10, "Did not feed enough rock.");
            public static Problem ROCK_HIGH = new Problem(3, 10, "Fed too much rock.");
            public static Problem MAGIC_CRITICALLY_LOW = new Problem(4, 100, "Starving for magic!");
            public static Problem MAGIC_CRITICALLY_HIGH = new Problem(5, 100, "Overflowing with magic!");
            public static Problem ROCK_CRITICALLY_LOW = new Problem(6, 100, "Starving for rock!");
            public static Problem ROCK_CRITICALLY_HIGH = new Problem(7, 100, "Exploding from too much rock!");

            public static Problem NO_PLAY_AT_ALL = new Problem(8, 8000, "Did not play with the golem..");
            public static Problem LITTLE_PLAY = new Problem(9, 1000, "Hardly played with the golem.");
            public static Problem SOME_PLAY = new Problem(10, 300, "Golem needed more play time.");
            public static Problem NOT_ENOUGH_PLAY = new Problem(11, 100, "Could have played with the golem more.");

            public static IList<Problem> AllProblems = new List<Problem>
            {
                MAGIC_LOW,
                MAGIC_HIGH,
                ROCK_LOW,
                ROCK_HIGH,
                MAGIC_CRITICALLY_LOW,
                MAGIC_CRITICALLY_HIGH,
                ROCK_CRITICALLY_LOW,
                ROCK_CRITICALLY_HIGH,
                NO_PLAY_AT_ALL,
                LITTLE_PLAY,
                SOME_PLAY,
                NOT_ENOUGH_PLAY
            };

            public static IDictionary<int, Problem> ProblemMap = AllProblems.ToDictionary(x => x.Id, x => x);
        }

        public GameObject[] ChangeToBaskets;
        public GameObject BasketPrefab;
        public GameObject RockPrefab;
        public GameObject BottomUI;

        public int NumberOfPlayForMaxScore = 30;

        public int WorstScore = 10000;

        private readonly IList<int> scores;
        private Coroutine checker;

        public ScoreTracker()
        {
            scores = new List<int>(2000);
        }

        public void Awake()
        {
            Instance = this;
        }

        public void StartRound()
        {
            checker = StartCoroutine(CheckCoroutine());
            scores.Clear();
        }

        public void EndRound()
        {
            StopCoroutine(checker);

            FinalCheck();
            var grade = GetGrade();
            var score = GetScore();

            Debug.Log(String.Format("You got a {0} with a score of {1}!", grade, score));

            ShowScoreScreen();
        }

        private Sprite GetGradeSprite(Grade grade)
        {
            switch (grade)
            {
                case Grade.S:
                    return S;
                case Grade.APlus:
                    return APlus;
                case Grade.A:
                    return A;
                case Grade.AMinus:
                    return AMinus;
                case Grade.BPlus:
                    return BPlus;
                case Grade.B:
                    return B;
                case Grade.BMinus:
                    return BMinus;
                case Grade.CPlus:
                    return CPlus;
                case Grade.C:
                    return C;
                case Grade.CMinus:
                    return CMinus;
                case Grade.DPlus:
                    return DPlus;
                case Grade.D:
                    return D;
                case Grade.DMinus:
                    return DMinus;
                case Grade.FPlus:
                    return FPlus;
                case Grade.F:
                    return F;
                case Grade.FMinus:
                    return FMinus;
                default:
                    return FMinus;
            }
        }

        public void ShowScoreScreen()
        {
            ScoreScreen.SetActive(true);
            ScoreText.text = String.Join("\n", Top3Problems().Select(x => "• " + x.Text).ToArray());
            GradeImage.sprite = GetGradeSprite(GetGrade());

            var group = ScoreScreen.GetComponent<CanvasGroup>();

            const float duration = 0.5f;

            group.alpha = 0;
            group.transform.localScale = new Vector3(2.0f, 2.0f);

            group.DOFade(1, duration).SetEase(Ease.InQuad);
            group.transform.DOScale(new Vector3(1.0f, 1.0f), duration).SetEase(Ease.InQuad);

            Delay.Of(5.0f, () =>
            {
                group.transform.DOScale(0.4f, 1.0f).SetEase(Ease.InOutQuad);
                ((RectTransform) group.transform).DOAnchorPos(new Vector2(-595, 330), 1.0f).SetEase(Ease.InOutQuad);
            });

            ChangeTokensToBaskets();
            StartCoroutine(SpawnRocks());

            CatchForNewGame.gameObject.SetActive(true);
            CatchForNewGame.text = String.Format("Catch {0} for New Game", NumToCatch - numCaught);
            BottomUI.SetActive(false);
        }

        private void ChangeTokensToBaskets()
        {
            foreach (var obj in ChangeToBaskets)
            {
                foreach (Transform child in obj.transform)
                {
                    Destroy(child.gameObject);
                }

                Instantiate(BasketPrefab, obj.transform, false);
            }
        }

        private IEnumerator SpawnRocks()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);

                try
                {
                    var validObjs = ChangeToBaskets.Where(x =>
                    {
                        var basket = x.GetComponentInChildren<Basket>();
                        return basket != null && basket.gameObject != null && basket.gameObject.activeInHierarchy;
                    }).ToArray();

                    if (validObjs.Length == 0)
                        continue;

                    var randomObj = validObjs[Random.Range(0, validObjs.Length - 1)];

                    var dir = Random.Range(0, Mathf.PI * 2);
                    var dist = Random.Range(0.5f, RockRange + 0.5f);

                    var xPos = dist * Mathf.Cos(dir);
                    var yPos = dist * Mathf.Sin(dir);

                    var pos = new Vector3(xPos, RockHeight, yPos);
                    Instantiate(RockPrefab, randomObj.transform.position + pos, Random.rotationUniform);
                }
                catch (Exception)
                {
                    Debug.Log("Caught error");
                }
            }
        }

        private IList<Problem> Top3Problems()
        {
            return scores.GroupBy(x => x).Select(x => new
            {
                Problem = Problems.ProblemMap[x.Key],
                Count = x.Count()
            }).Select(x => new
            {
                x.Problem,
                TotalScore = x.Problem.Weight * x.Count
            }).OrderByDescending(x => x.TotalScore)
                .Take(3)
                .Select(x => x.Problem)
                .ToList();
        }

        private int GetScore()
        {
            return scores.Sum(x => Problems.ProblemMap[x].Weight);
        }

        private Grade GetGrade()
        {
            var totalScore = GetScore();

            if ( totalScore == 0)
                return Grade.S;

            var grades = new List<Grade>
            {
                Grade.APlus,
                Grade.A,
                Grade.AMinus,
                Grade.BPlus,
                Grade.B,
                Grade.BMinus,
                Grade.CPlus,
                Grade.C,
                Grade.CMinus,
                Grade.DPlus,
                Grade.D,
                Grade.DMinus,
                Grade.FPlus,
                Grade.F,
                Grade.FMinus
            };

            var scorePercent = (float) totalScore / WorstScore;
            var curved = Mathf.Pow(scorePercent, 1 / 2.2f);

            return grades[Mathf.RoundToInt(curved * (grades.Count - 1))];
        }

        private void FinalCheck()
        {
            var gameplay = GolemGameplay.Instance;
            if ( gameplay.PlayCount == 0)
                AddProblem(Problems.NO_PLAY_AT_ALL);
            else if (gameplay.PlayCount < 5)
                AddProblem(Problems.LITTLE_PLAY);
            else if (gameplay.PlayCount < 10)
                AddProblem(Problems.SOME_PLAY);
            else if ( gameplay.PlayCount < 15)
                AddProblem(Problems.NOT_ENOUGH_PLAY);
        }

        private IEnumerator CheckCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.5f);
                Check();
            }
        }

        private void Check()
        {
            var gameplay = GolemGameplay.Instance;

            if ( gameplay.CurrentRock < 400 )
                AddProblem(Problems.ROCK_LOW);
            if ( gameplay.CurrentRock < 200 )
                AddProblem(Problems.ROCK_CRITICALLY_LOW);
            if ( gameplay.CurrentRock > 600)
                AddProblem(Problems.ROCK_HIGH);
            if ( gameplay.CurrentRock > 800)
                AddProblem(Problems.ROCK_CRITICALLY_HIGH);

            if (gameplay.CurrentMagic < 400)
                AddProblem(Problems.MAGIC_LOW);
            if (gameplay.CurrentMagic < 200)
                AddProblem(Problems.MAGIC_CRITICALLY_LOW);
            if (gameplay.CurrentMagic > 600)
                AddProblem(Problems.MAGIC_HIGH);
            if (gameplay.CurrentMagic > 800)
                AddProblem(Problems.MAGIC_CRITICALLY_HIGH);
        }

        public void AddProblem(Problem problem)
        {
            scores.Add(problem.Id);
        }

        public void CaughtRock()
        {
            numCaught++;
            CatchForNewGame.text = String.Format("Catch {0} for New Game", NumToCatch - numCaught);

            if (numCaught >= NumToCatch)
            {
                World.Instance.Restart();
            }
        }
    }
}
