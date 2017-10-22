using System.Collections.Generic;
using NUnit.Framework.Internal.Execution;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class World : MonoBehaviour
    {
        public static World Instance { get; private set; }

        public GameObject MainCamera { get; private set; }

        private IDictionary<string, GolObject> objects;

        public GolObject Rock { get { return Get("Rock"); } }

        public GolObject Magic { get { return Get("Magic"); } }

        public GolObject Toy { get { return Get("Toy"); } }

        public GameObject Ded;

        public GameObject Countdown;

        public ICollection<GolObject> Interactables
        {
            get
            {
                var interactables = new List<GolObject>();
                if( Rock != null)
                    interactables.Add(Rock);
                if(Magic != null)
                    interactables.Add(Magic);
                if(Toy != null)
                    interactables.Add(Toy);
                return interactables;
            }
        }

        public void Awake()
        {
            objects = new Dictionary<string, GolObject>();
            Instance = this;

            MainCamera = GameObject.Find("ARCamera");
        }

        public void Start()
        {
            Ded.SetActive(false);
        }

        public static void Register(GolObject golObject, string name)
        {
            Instance.objects[name] = golObject;
        }

        public static GolObject Get(string name)
        {
            GolObject obj;
            Instance.objects.TryGetValue(name, out obj);
            return obj;
        }

        public void Dead()
        {
            Ded.SetActive(true);
            Countdown.SetActive(true);

            Delay.Of(5, Restart);
        }

        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
    }
}
