using System;
using System.Collections;
using Game.Storage;
using UnityEngine;
using Utilities;
using Zenject;

namespace Game.Common
{
    public class AssetTransformer : MonoBehaviour
    {
        [SerializeField] private ItemSO processedItemType;
        [SerializeField] private Transform transformPoint;

        private const float TransformDuration = 0.15f;
        private const float TransformWaitTime = 0.2f;
        private const float CycleCheckInterval = 0.2f;

        private const float ItemSpacingHorizontal = 0.4f;
        private const float ItemSpacingVertical = 0.4f;
        
        private bool _isAnimatorActive;
        private int _activeTransformCount;

        private Animator _animator;
        private ProcessingStorage _inputStorage;
        private ProcessedStorage _outputStorage;

        [Inject]
        private void Constructor(ProcessingStorage processingStorage, ProcessedStorage processedStorage)
        {
            _inputStorage = processingStorage;
            _outputStorage = processedStorage;
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            if (_animator) _animator.enabled = false;
        }

        private void Start()
        {
            StartCoroutine(TransformCycle());
        }

        private void UpdateAnimatorState(bool isProducing)
        {
            if (_animator is null) return;

            if (isProducing && !_isAnimatorActive)
            {
                _animator.enabled = true;
                _isAnimatorActive = true;
            }
            else if (!isProducing && _isAnimatorActive)
            {
                _animator.enabled = false;
                _isAnimatorActive = false;
            }
        }

        private IEnumerator TransformCycle()
        {
            var cycleWait = new WaitForSeconds(CycleCheckInterval);

            while (true)
            {
                if (!_outputStorage.CanAddItem())
                {
                    yield return cycleWait;
                    continue;
                }

                if (_inputStorage.TryRemoveItem(out var item))
                {
                    UpdateAnimatorState(true);
                    StartCoroutine(ProcessItemSequence(item));
                }
                else if (_activeTransformCount == 0)
                {
                    UpdateAnimatorState(false);
                }

                yield return cycleWait;
            }
        }

        private IEnumerator ProcessItemSequence(Item item)
        {
            _activeTransformCount++;

            var moveCompleted = false;

            MoveItemToTransformerPoint(item, () => moveCompleted = true);

            while (!moveCompleted)
            {
                yield return null;
            }

            yield return new WaitForSeconds(TransformWaitTime);
            TransformItem(item);
            TransferItemToTargetStorage(item);

            _activeTransformCount--;

            if (_activeTransformCount == 0 && _inputStorage.GetCurrentCount() == 0)
            {
                UpdateAnimatorState(false);
            }
        }

        private void TransformItem(Item item)
        {
            item.InitializeItem(processedItemType);
        }

        private void TransferItemToTargetStorage(Item item)
        {
            if (_outputStorage.CanAddItem())
            {
                MoveItemToProcessedStorage(item);
            }
            else
            {
                _inputStorage.TryAddItem(item);
            }
        }

        private void MoveItemToTransformerPoint(Item item, Action onComplete)
        {
            if (transformPoint is not null)
            {
                AnimationUtils.MoveItemWithBounce(
                    _ => onComplete?.Invoke(),
                    item,
                    transformPoint.localPosition,
                    TransformDuration
                );
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        private void MoveItemToProcessedStorage(Item item)
        {
            item.transform.SetParent(_outputStorage.GetItemHolder());

            var itemIndex = _outputStorage.GetCurrentCount();

            var targetLocalPosition = PositionUtils.CalculateGridPosition(
                itemIndex,
                ItemSpacingVertical,
                ItemSpacingHorizontal,
                item.GetColliderBoundSizeY()
            );

            AnimationUtils.MoveItemWithBounce(_ => _outputStorage.PushItem(item),
                item,
                targetLocalPosition,
                TransformDuration
            );
        }
    }
}