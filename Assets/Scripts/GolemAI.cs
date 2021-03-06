﻿using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public enum GolemState
    {
        Idle,
        Moving
    }

    public class GolemAI : MonoBehaviour
    {
        public Vector3 GolemPosition { get { return gameObject.transform.position; } }

        public GolemController Controller { get { return GetComponent<GolemController>(); } }
        public GolemGameplay Gameplay { get { return GetComponent<GolemGameplay>(); } }

        [AssignedInUnity]
        public float ReachedTargetRadius;

        public StateMachine StateMachine { get; private set; }

        public void Awake()
        {
            StateMachine = gameObject.AddComponent<StateMachine>();
            StateMachine.Add<GolemIdle>();
            StateMachine.Add<GolemMoveToTarget>();            
        }

        public void Start()
        {
            StateMachine.ChangeState<GolemIdle>();
        }
    }

    public class GolemIdle : State<GolemAI>
    {
        public override void Enter()
        {
            Parent.Controller.ClearTargetPosition();
            StartCoroutine(CheckForInteractable());
            Parent.Controller.Animator.SetBool("IsMoving", false);
        }

        private IEnumerator CheckForInteractable()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.5f);

                var interactables = World.Instance.Interactables;
                if (interactables.Count == 0)
                    continue;

                var closestInteractable = interactables
                    .Where(x => x.Active)
                    .OrderBy(x => Vector3.Distance(Parent.GolemPosition, x.FlatPosition)).FirstOrDefault();

                if (closestInteractable != null)
                {
                    Parent.Gameplay.SeenSomething();
                    StateMachine.GetState<GolemMoveToTarget>().Target = closestInteractable;
                    StateMachine.ChangeState<GolemMoveToTarget>();
                }
            }
        }
    }

    public class GolemMoveToTarget : State<GolemAI>
    {
        public GolObject Target { get; set; }

        public override void Enter()
        {
            Parent.Controller.Animator.SetBool("IsMoving", true);

            if (Target == null || Target.Active == false)
                StateMachine.ChangeState<GolemIdle>();

            StartCoroutine(CheckForInteractable());
        }

        public override void Update()
        {
            if (Target == null || Target.Active == false)
            {
                StateMachine.ChangeState<GolemIdle>();
                return;
            }

            var targetPosition = Target.FlatPosition;

            Parent.Controller.SetTargetPosition(targetPosition);

            if (Vector3.Distance(Parent.GolemPosition, targetPosition) < Parent.ReachedTargetRadius)
            {
                switch (Target.Name)
                {
                    case "Rock":
                        StateMachine.GetState<GolemAbsorbingRock>().Rock = Target;
                        StateMachine.ChangeState<GolemAbsorbingRock>();
                        break;
                    case "Magic":
                        StateMachine.GetState<GolemAbsorbingMagic>().Magic = Target;
                        StateMachine.ChangeState<GolemAbsorbingMagic>();
                        break;
                    case "Toy":
                        StateMachine.GetState<GolemInRangeOfToy>().Toy = Target;
                        StateMachine.ChangeState<GolemInRangeOfToy>();
                        break;
                }
            }
        }

        private IEnumerator CheckForInteractable()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);

                var interactables = World.Instance.Interactables;
                if (interactables.Count == 0)
                    continue;

                var closestInteractable = interactables
                    .Where(x => x.Active)
                    .OrderBy(x => Vector3.Distance(Parent.Controller.transform.parent.position, x.FlatPosition)).FirstOrDefault();

                Target = closestInteractable;
            }
        }

        public override void Exit()
        {
        }
    }

    public class GolemAbsorbingRock : State<GolemAI>
    {
        public GolObject Rock { get; set; }

        public override void Enter()
        {
            Parent.Gameplay.StartConsume(Need.Rock);
            Parent.Controller.SetTargetPosition(Rock.FlatPosition);
            Parent.Controller.GatherRockParticles.gameObject.SetActive(true);
            Parent.Controller.Animator.SetBool("IsConsumingRock", true);
        }

        public override void Update()
        {
            if (Rock == null || Rock.Active == false || Parent.GolemPosition.DistanceTo(Rock.FlatPosition) > Parent.ReachedTargetRadius)
            {
                StateMachine.ChangeState<GolemIdle>();
            }
        }

        public override void Exit()
        {
            Parent.Gameplay.StopConsume();
            Parent.Controller.GatherRockParticles.gameObject.SetActive(false);
            Parent.Controller.Animator.SetBool("IsConsumingRock", false);
        }
    }

    public class GolemAbsorbingMagic : State<GolemAI>
    {
        public GolObject Magic { get; set; }

        public override void Enter()
        {
            Parent.Gameplay.StartConsume(Need.Magic);
            Parent.Controller.SetTargetPosition(Magic.FlatPosition);
            Parent.Controller.GatherMagicParticles.gameObject.SetActive(true);
            Parent.Controller.Animator.SetBool("IsConsumingMagic", true);
        }

        public override void Update()
        {
            if (Magic == null || Magic.Active == false || Parent.GolemPosition.DistanceTo(Magic.FlatPosition) > Parent.ReachedTargetRadius)
            {
                StateMachine.ChangeState<GolemIdle>();
            }
        }

        public override void Exit()
        {
            Parent.Gameplay.StopConsume();
            Parent.Controller.GatherMagicParticles.gameObject.SetActive(false);
            Parent.Controller.Animator.SetBool("IsConsumingMagic", false);
        }
    }

    public class GolemInRangeOfToy : State<GolemAI>
    {
        public GolObject Toy { get; set; }

        private Castle castle;
        private bool isSmashing;

        public GolemInRangeOfToy()
        {
            GolemAnimationEvents.Instance.Smash += SmashHappened;
        }

        private void SmashHappened()
        {
            if (castle != null)
            {
                castle.Smash();
                isSmashing = false;
            }
        }

        public override void Enter()
        {
            Parent.Controller.SetTargetPosition(Toy.FlatPosition);
            Parent.Controller.Animator.SetBool("IsMoving", false);
            castle = Toy.GetComponentInChildren<Castle>();
            isSmashing = false;
        }

        public override void Update()
        {
            if (Toy == null || Toy.Active == false || Parent.GolemPosition.DistanceTo(Toy.FlatPosition) > Parent.ReachedTargetRadius)
            {
                StateMachine.ChangeState<GolemIdle>();
                return;
            }

            var forward = ((Vector2) Parent.Controller.Body.forward).normalized;
            var toCastle = ((Vector2)(Toy.FlatPosition - Parent.GolemPosition)).normalized;

            // Must be mostly facing castle
            if (Vector2.Dot(forward, toCastle) < 0.8)
                return;

            if (castle.CanBeSmashed && !isSmashing)
            {
                Debug.Log("SMASH");
                Parent.Controller.Animator.SetTrigger("Smash");
                isSmashing = true;
            }
        }

        public override void Exit()
        {
        }
    }

    public class GolemDead : State<GolemAI>
    {
        public override void Enter()
        {
            Parent.Controller.SetTargetPosition(Parent.GolemPosition);
        }
    }

    public class EndGame : State<GolemAI>
    {
        public override void Enter()
        {
            Parent.Controller.Animator.SetBool("IsMoving", false);
            Parent.Controller.SetTargetPosition(Parent.GolemPosition);
        }
    }
}
