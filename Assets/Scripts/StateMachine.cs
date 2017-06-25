using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public abstract class State
    {
        public StateMachine StateMachine { get; set; }

        public GameObject GameObject { get { return StateMachine.gameObject; } }

        private readonly IList<Coroutine> coroutines;

        protected State()
        {
            coroutines = new List<Coroutine>();
        }

        public virtual void Enter()
        {
        }

        public virtual void Update()
        {
            
        }

        public void OnExit()
        {
            foreach (var coroutine in coroutines)
            {
                StateMachine.StopCoroutine(coroutine);
            }
            coroutines.Clear();
            Exit();
        }

        public virtual void Exit()
        {
            
        }

        public virtual void OnCollisionEnter(Collision collision)
        {
            
        }

        public void OnEnable()
        {
            Enter();
        }

        public void OnDisable()
        {
            
        }

        public T Get<T>() where T : MonoBehaviour
        {
            return StateMachine.GetComponent<T>();
        }

        protected void StartCoroutine(IEnumerator coroutine)
        {
            coroutines.Add(StateMachine.StartCoroutine(coroutine));
        }
    }

    public abstract class State<T> : State where T : MonoBehaviour
    {
        public T Parent { get { return StateMachine.GetComponent<T>(); } }
    }

    public class StateMachine : MonoBehaviour
    {
        public State CurrentState { get; set; }

        private IList<State> states;

        public void Awake()
        {
            states = new List<State>();
        }

        public void Add<T>() where T : State, new()
        {
            var state = new T
            {
                StateMachine = this
            };
            states.Add(state);
        }

        public void ChangeState<T>() where T : State, new()
        {
            if (CurrentState != null)
            {
                CurrentState.OnExit();
            }

            CurrentState = GetState<T>();
            CurrentState.Enter();
        }

        public void Update()
        {
            if (CurrentState != null)
                CurrentState.Update();
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (CurrentState != null)
                CurrentState.OnCollisionEnter(collision);
        }

        public void OnEnable()
        {
            if (CurrentState != null)
                CurrentState.OnEnable();
        }

        public void OnDisable()
        {
            if (CurrentState != null)
                CurrentState.OnDisable();
        }

        public T GetState<T>() where T : State, new()
        {
            var state = states.OfType<T>().FirstOrDefault();
            if (state == null)
            {
                Add<T>();
                state = states.OfType<T>().FirstOrDefault();
            }
            return state;
        }
    }
}
