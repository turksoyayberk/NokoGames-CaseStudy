using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Zenject;
using Game.Storage;

namespace Game.AI
{
    public class AIController : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        private NavMeshAgent _navMeshAgent;
        private CharacterStorage _characterStorage;

        private const float InteractionDistance = 2f;
        private const float StoppingDistance = 1f;
        private const float IdleWaitTime = 2f;

        private AIState _currentState;
        private Transform _currentTarget;
        private bool _isWaiting;
        private Coroutine _waitCoroutine;
        private Vector3 _lastPosition;

        private static readonly int IsRunningHash = Animator.StringToHash("isRunning");

        [Inject]
        private void Constructor(AIManager aiManager)
        {
            aiManager.RegisterAI(this);
        }

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            ForceStopWaiting();
        }

        private void Update()
        {
            UpdateAnimations();
            CheckTargetReached();
            CheckPositionChanged();
        }

        private void Initialize()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _characterStorage = GetComponent<CharacterStorage>();

            _navMeshAgent.stoppingDistance = StoppingDistance;
            _currentState = AIState.GoingToRawStorage;
            _isWaiting = false;
            _lastPosition = transform.position;
        }

        private void UpdateAnimations()
        {
            var isMoving = _navMeshAgent.velocity.magnitude > 0.1f;
            animator.SetBool(IsRunningHash, isMoving);
        }

        private void CheckTargetReached()
        {
            if (_currentTarget is not null && !_isWaiting && HasReachedDestination())
            {
                _isWaiting = true;
                _waitCoroutine = StartCoroutine(WaitAtLocation());
            }
        }

        private void CheckPositionChanged()
        {
            if (Vector3.Distance(_lastPosition, transform.position) > 1.0f)
            {
                ForceStopWaiting();

                if (_navMeshAgent.isActiveAndEnabled && _navMeshAgent.isOnNavMesh)
                {
                    _navMeshAgent.Warp(transform.position);

                    if (_currentTarget is not null)
                    {
                        _navMeshAgent.SetDestination(_currentTarget.position);
                    }
                }
            }

            _lastPosition = transform.position;
        }

        private void ForceStopWaiting()
        {
            if (_isWaiting)
            {
                _isWaiting = false;

                if (_waitCoroutine != null)
                {
                    StopCoroutine(_waitCoroutine);
                    _waitCoroutine = null;
                }

                if (_navMeshAgent.isActiveAndEnabled)
                {
                    _navMeshAgent.isStopped = false;
                }
            }
        }

        public void SetTarget(Transform target)
        {
            if (target == null)
                return;

            _currentTarget = target;
            if (_navMeshAgent.isActiveAndEnabled && _navMeshAgent.isOnNavMesh)
            {
                _navMeshAgent.SetDestination(_currentTarget.position);
            }
        }

        public void SetState(AIState newState)
        {
            _currentState = newState;
            ForceStopWaiting();
        }

        public AIState GetCurrentState()
        {
            return _currentState;
        }

        public bool HasReachedDestination()
        {
            if (_currentTarget == null)
                return false;

            var distanceToTarget = Vector3.Distance(transform.position, _currentTarget.position);
            return distanceToTarget <= InteractionDistance;
        }

        public bool IsWaiting()
        {
            return _isWaiting;
        }

        public CharacterStorage GetCharacterStorage()
        {
            return _characterStorage;
        }

        public IEnumerator WaitAtLocation(float waitTime = IdleWaitTime)
        {
            if (_navMeshAgent.isActiveAndEnabled)
                _navMeshAgent.isStopped = true;

            yield return new WaitForSeconds(waitTime);

            if (_navMeshAgent.isActiveAndEnabled)
                _navMeshAgent.isStopped = false;

            _isWaiting = false;
        }

        private void OnTransformParentChanged()
        {
            ForceStopWaiting();
        }
    }

    public enum AIState
    {
        GoingToRawStorage,
        WaitingAtRawStorage,
        GoingToProcessingStorage,
        WaitingAtProcessingStorage,
        GoingToProcessedStorage,
        WaitingAtProcessedStorage,
        GoingToTrashStorage,
        WaitingAtTrashStorage
    }
}