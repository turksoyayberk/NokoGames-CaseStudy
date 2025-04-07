using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Game.Storage;

namespace Game.AI
{
    public class AIManager : MonoBehaviour
    {
        [SerializeField] private Transform rawStoragePoint;
        [SerializeField] private Transform processingStoragePoint;
        [SerializeField] private Transform processedStoragePoint;
        [SerializeField] private Transform trashStoragePoint;

        private const float StuckCheckDuration = 5f;
        private const float MinProgressDistance = 0.2f;
        private const float MaxWaitTimeAtStorage = 15f;
        private const float WaitTimeAfterTransfer = 0.5f;
        private const float AiWaitDuration = 2f;
        private const int MaxInventoryCapacity = 50;

        private readonly List<AIController> _aiControllers = new();
        private readonly Dictionary<AIController, Vector3> _lastPositions = new();
        private readonly Dictionary<AIController, float> _stuckTimers = new();
        private readonly Dictionary<AIController, int> _initialItemCounts = new();
        private readonly Dictionary<AIController, int> _cycleItemCount = new();
        private readonly Dictionary<AIController, float> _waitTimers = new();
        private readonly Dictionary<AIController, int> _lastProcessedCount = new();

        private Dictionary<AIState, Action<AIController>> _stateHandlers;

        private void Start()
        {
            InitializeStateHandlers();
            StartCoroutine(ManageAI());
        }

        public void RegisterAI(AIController aiController)
        {
            if (!_aiControllers.Contains(aiController))
            {
                _aiControllers.Add(aiController);
                _lastPositions[aiController] = aiController.transform.position;
                _stuckTimers[aiController] = 0f;
                _initialItemCounts[aiController] = 0;
                _cycleItemCount[aiController] = 0;
                _waitTimers[aiController] = 0f;
                _lastProcessedCount[aiController] = 0;
            }
        }

        private void InitializeStateHandlers()
        {
            _stateHandlers = new Dictionary<AIState, Action<AIController>>
            {
                { AIState.GoingToRawStorage, HandleGoingToRawStorage },
                { AIState.WaitingAtRawStorage, HandleWaitingAtRawStorage },
                { AIState.GoingToProcessingStorage, HandleGoingToProcessingStorage },
                { AIState.WaitingAtProcessingStorage, HandleWaitingAtProcessingStorage },
                { AIState.GoingToProcessedStorage, HandleGoingToProcessedStorage },
                { AIState.WaitingAtProcessedStorage, HandleWaitingAtProcessedStorage },
                { AIState.GoingToTrashStorage, HandleGoingToTrashStorage },
                { AIState.WaitingAtTrashStorage, HandleWaitingAtTrashStorage }
            };
        }

        private IEnumerator ManageAI()
        {
            while (true)
            {
                foreach (var ai in _aiControllers)
                {
                    if (ai is null) continue;

                    UpdateAIState(ai);
                    CheckIfStuck(ai);
                }

                yield return new WaitForSeconds(0.5f);
            }
        }

        private void CheckIfStuck(AIController ai)
        {
            if (ai.IsWaiting() || ai is null)
            {
                _stuckTimers[ai] = 0f;
                return;
            }

            AIState currentState = ai.GetCurrentState();

            if (currentState == AIState.GoingToRawStorage ||
                currentState == AIState.GoingToProcessingStorage ||
                currentState == AIState.GoingToProcessedStorage ||
                currentState == AIState.GoingToTrashStorage)
            {
                var currentPosition = ai.transform.position;
                var lastPosition = _lastPositions[ai];

                if (Vector3.Distance(currentPosition, lastPosition) < MinProgressDistance)
                {
                    _stuckTimers[ai] += 0.5f;

                    if (_stuckTimers[ai] >= StuckCheckDuration)
                    {
                        ForceResetState(ai, currentState);
                        _stuckTimers[ai] = 0f;
                    }
                }
                else
                {
                    _stuckTimers[ai] = 0f;
                }

                _lastPositions[ai] = currentPosition;
            }
            else
            {
                _stuckTimers[ai] = 0f;
            }
        }

        private void ForceResetState(AIController ai, AIState currentState)
        {
            switch (currentState)
            {
                case AIState.GoingToRawStorage:
                    SetAIStateWithTarget(ai, AIState.GoingToRawStorage, rawStoragePoint);
                    break;
                case AIState.GoingToProcessingStorage:
                    SetAIStateWithTarget(ai, AIState.GoingToProcessingStorage, processingStoragePoint);
                    break;
                case AIState.GoingToProcessedStorage:
                    SetAIStateWithTarget(ai, AIState.GoingToProcessedStorage, processedStoragePoint);
                    break;
                case AIState.GoingToTrashStorage:
                    SetAIStateWithTarget(ai, AIState.GoingToTrashStorage, trashStoragePoint);
                    break;
            }
        }

        private void SetAIStateWithTarget(AIController ai, AIState state, Transform target)
        {
            ai.SetState(state);
            ai.SetTarget(target);
        }

        private void UpdateAIState(AIController ai)
        {
            AIState currentState = ai.GetCurrentState();
            if (_stateHandlers.TryGetValue(currentState, out var handler))
            {
                handler(ai);
            }
        }

        private void HandleGoingToRawStorage(AIController ai)
        {
            if (ai.GetCurrentState() != AIState.GoingToRawStorage)
                return;

            ai.SetTarget(rawStoragePoint);

            if (ai.HasReachedDestination() && ai.IsWaiting())
            {
                SetNextState(ai, AIState.WaitingAtRawStorage);
                _initialItemCounts[ai] = ai.GetCharacterStorage().GetCurrentCount();
                _lastProcessedCount[ai] = 0;
            }
        }

        private void HandleWaitingAtRawStorage(AIController ai)
        {
            if (ai.GetCurrentState() != AIState.WaitingAtRawStorage)
                return;

            CharacterStorage storage = ai.GetCharacterStorage();
            var currentCount = storage.GetCurrentCount();
            var initialCount = _initialItemCounts[ai];
            var itemsCollected = currentCount - initialCount;

            if (ai.IsWaiting())
                _waitTimers[ai] += 0.5f;

            var itemCountChanged = itemsCollected != _lastProcessedCount[ai];
            if (itemCountChanged)
            {
                _lastProcessedCount[ai] = itemsCollected;
                _waitTimers[ai] = 0f;
            }


            if (currentCount >= MaxInventoryCapacity ||
                (itemsCollected > 0 && _waitTimers[ai] >= WaitTimeAfterTransfer && ai.IsWaiting()))
            {
                _cycleItemCount[ai] = itemsCollected;
                SetNextState(ai, AIState.GoingToProcessingStorage);
            }
            else if (_waitTimers[ai] >= MaxWaitTimeAtStorage)
            {
                _cycleItemCount[ai] = Mathf.Max(1, itemsCollected);
                SetNextState(ai, AIState.GoingToProcessingStorage);
            }
            else if (ai.IsWaiting() && itemsCollected <= 0)
            {
                StartCoroutine(ai.WaitAtLocation(AiWaitDuration));
            }
        }

        private void HandleGoingToProcessingStorage(AIController ai)
        {
            if (ai.GetCurrentState() != AIState.GoingToProcessingStorage)
                return;

            ai.SetTarget(processingStoragePoint);

            if (ai.HasReachedDestination() && ai.IsWaiting())
            {
                SetNextState(ai, AIState.WaitingAtProcessingStorage);
                _initialItemCounts[ai] = ai.GetCharacterStorage().GetCurrentCount();
                _lastProcessedCount[ai] = 0;
            }
        }

        private void HandleWaitingAtProcessingStorage(AIController ai)
        {
            if (ai.GetCurrentState() != AIState.WaitingAtProcessingStorage)
                return;

            CharacterStorage storage = ai.GetCharacterStorage();

            var currentCount = storage.GetCurrentCount();
            var initialCount = _initialItemCounts[ai];
            var processedItems = initialCount - currentCount;
            var targetItems = _cycleItemCount[ai];

            if (ai.IsWaiting())
                _waitTimers[ai] += 0.5f;

            var itemCountChanged = processedItems != _lastProcessedCount[ai];
            if (itemCountChanged)
            {
                _lastProcessedCount[ai] = processedItems;
                _waitTimers[ai] = 0f;
            }

            StorageBase processedStorage = FindStorageAtPoint(processedStoragePoint);
            if (processedStorage != null && processedStorage.GetCurrentCount() <= 0 && initialCount <= 0)
            {
                SetNextState(ai, AIState.GoingToRawStorage);
                return;
            }

            if (currentCount <= 0 ||
                (processedItems >= targetItems && _waitTimers[ai] >= WaitTimeAfterTransfer && !ai.IsWaiting()))
            {
                SetNextState(ai, AIState.GoingToProcessedStorage);
            }
            else if (_waitTimers[ai] >= MaxWaitTimeAtStorage)
            {
                SetNextState(ai, AIState.GoingToProcessedStorage);
            }
            else if (ai.IsWaiting() && processedItems < targetItems)
            {
                StartCoroutine(ai.WaitAtLocation(AiWaitDuration));
            }
        }

        private void HandleGoingToProcessedStorage(AIController ai)
        {
            if (ai.GetCurrentState() != AIState.GoingToProcessedStorage)
                return;

            ai.SetTarget(processedStoragePoint);

            if (ai.HasReachedDestination() && ai.IsWaiting())
            {
                SetNextState(ai, AIState.WaitingAtProcessedStorage);
                _initialItemCounts[ai] = ai.GetCharacterStorage().GetCurrentCount();
                _lastProcessedCount[ai] = 0;
            }
        }

        private void HandleWaitingAtProcessedStorage(AIController ai)
        {
            if (ai.GetCurrentState() != AIState.WaitingAtProcessedStorage)
                return;

            CharacterStorage storage = ai.GetCharacterStorage();

            var currentCount = storage.GetCurrentCount();
            var collectedItems = currentCount;
            var targetItems = _cycleItemCount[ai];

            if (ai.IsWaiting())
                _waitTimers[ai] += 0.5f;

            var itemCountChanged = collectedItems != _lastProcessedCount[ai];
            if (itemCountChanged)
            {
                _lastProcessedCount[ai] = collectedItems;
                _waitTimers[ai] = 0f;
            }

            StorageBase processedStorage = FindStorageAtPoint(processedStoragePoint);
            if (processedStorage != null && processedStorage.GetCurrentCount() <= 0 && collectedItems <= 0)
            {
                SetNextState(ai, AIState.GoingToRawStorage);
                return;
            }

            if (collectedItems > 0 && collectedItems < targetItems &&
                processedStorage != null && processedStorage.GetCurrentCount() <= 0)
            {
                _cycleItemCount[ai] = collectedItems;
                SetNextState(ai, AIState.GoingToTrashStorage);
                return;
            }

            if (currentCount >= MaxInventoryCapacity ||
                (collectedItems >= targetItems && _waitTimers[ai] >= WaitTimeAfterTransfer && ai.IsWaiting()))
            {
                SetNextState(ai, AIState.GoingToTrashStorage);
            }
            else if (_waitTimers[ai] >= MaxWaitTimeAtStorage)
            {
                SetNextState(ai, AIState.GoingToTrashStorage);
            }
            else if (ai.IsWaiting() && collectedItems < targetItems)
            {
                StartCoroutine(ai.WaitAtLocation(AiWaitDuration));
            }
        }

        private void HandleGoingToTrashStorage(AIController ai)
        {
            if (ai.GetCurrentState() != AIState.GoingToTrashStorage)
                return;

            ai.SetTarget(trashStoragePoint);

            if (ai.HasReachedDestination() && ai.IsWaiting())
            {
                SetNextState(ai, AIState.WaitingAtTrashStorage);
                _initialItemCounts[ai] = ai.GetCharacterStorage().GetCurrentCount();
                _lastProcessedCount[ai] = 0;
            }
        }

        private void HandleWaitingAtTrashStorage(AIController ai)
        {
            if (ai.GetCurrentState() != AIState.WaitingAtTrashStorage)
                return;

            CharacterStorage storage = ai.GetCharacterStorage();

            var currentCount = storage.GetCurrentCount();
            var initialCount = _initialItemCounts[ai];
            var disposedItems = initialCount - currentCount;
            var targetItems = _cycleItemCount[ai];

            if (ai.IsWaiting())
                _waitTimers[ai] += 0.5f;

            var itemCountChanged = disposedItems != _lastProcessedCount[ai];
            if (itemCountChanged)
            {
                _lastProcessedCount[ai] = disposedItems;
                _waitTimers[ai] = 0f;
            }

            if (currentCount <= 0 ||
                (disposedItems >= targetItems && _waitTimers[ai] >= WaitTimeAfterTransfer && ai.IsWaiting()))
            {
                SetNextState(ai, AIState.GoingToRawStorage);
            }
            else if (_waitTimers[ai] >= MaxWaitTimeAtStorage)
            {
                SetNextState(ai, AIState.GoingToRawStorage);
            }
            else if (ai.IsWaiting() && disposedItems < targetItems)
            {
                StartCoroutine(ai.WaitAtLocation(AiWaitDuration));
            }
        }

        private void SetNextState(AIController ai, AIState nextState)
        {
            ai.SetState(nextState);
            _waitTimers[ai] = 0f;
        }

        private StorageBase FindStorageAtPoint(Transform storagePoint)
        {
            StorageBase storage = storagePoint.GetComponent<StorageBase>();
            if (storage == null)
            {
                storage = storagePoint.GetComponentInChildren<StorageBase>();
            }

            return storage;
        }
    }
}