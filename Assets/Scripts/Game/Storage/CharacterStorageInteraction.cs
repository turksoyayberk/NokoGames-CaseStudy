using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Game.Common;

namespace Game.Storage
{
    public class CharacterStorageInteraction : MonoBehaviour
    {
        [SerializeField] private LayerMask rawStorageMask;
        [SerializeField] private TransferDefinitionListSO transferDefinitions;

        private const float InteractionRadius = 1f;
        private const float PickupRate = 0.1f;
        private const float RawStorageExitCheckDelay = 0.1f;

        private float _transferTimer;
        private float _rawStorageCheckTimer;

        private bool _isInRawStorageArea;
        private CharacterStorage _characterStorage;
        private RawSpawner _rawSpawner;

        private readonly Dictionary<LayerMask, StorageBase> _nearestStorages = new();
        private readonly Collider[] _colliders = new Collider[5];

        [Inject]
        private void Constructor(RawSpawner rawSpawner)
        {
            _rawSpawner = rawSpawner;
        }

        private void Awake()
        {
            _characterStorage = GetComponent<CharacterStorage>();
        }

        private void Update()
        {
            FindAllNearestStorages();
            ProcessTransferTimer();
            CheckRawStorageAreaStatus();
        }

        private void ProcessTransferTimer()
        {
            _transferTimer += Time.deltaTime;

            if (_transferTimer < PickupRate) return;

            _transferTimer = 0f;
            ProcessAllTransfers();
        }

        private void ProcessAllTransfers()
        {
            foreach (var definition in transferDefinitions.transferDefinitionList)
            {
                ProcessTransfer(definition);
            }
        }

        private void CheckRawStorageAreaStatus()
        {
            _rawStorageCheckTimer += Time.deltaTime;
            if (_rawStorageCheckTimer >= RawStorageExitCheckDelay)
            {
                _rawStorageCheckTimer = 0f;

                var wasInRawStorage = _isInRawStorageArea;
                _isInRawStorageArea = false;

                var hitCount = Physics.OverlapSphereNonAlloc(transform.position, InteractionRadius, _colliders,
                    rawStorageMask);

                if (hitCount > 0)
                {
                    _isInRawStorageArea = true;

                    if (!wasInRawStorage && _characterStorage.CanAddItem())
                    {
                        _rawSpawner.StopSpawning();
                    }
                }
                else
                {
                    if (wasInRawStorage)
                    {
                        _rawSpawner.StartSpawning();
                    }
                }
            }
        }

        private void ProcessTransfer(TransferDefinitionSO definition)
        {
            if (!_nearestStorages.TryGetValue(definition.storageLayerMask, out var nearestStorage) ||
                nearestStorage == null)
                return;

            if (definition.direction == TransferDirection.FromStorageToCharacter)
            {
                TransferBetweenStorages(nearestStorage, _characterStorage, definition.itemType);
            }
            else // FromCharacterToStorage
            {
                TransferBetweenStorages(_characterStorage, nearestStorage, definition.itemType);
            }
        }

        private void FindAllNearestStorages()
        {
            foreach (var definition in transferDefinitions.transferDefinitionList)
            {
                var nearestStorage = FindNearestStorage(definition.storageLayerMask);

                _nearestStorages[definition.storageLayerMask] = nearestStorage;
            }
        }

        private StorageBase FindNearestStorage(LayerMask layerMask)
        {
            var hitCount = Physics.OverlapSphereNonAlloc(transform.position, InteractionRadius, _colliders, layerMask);

            var closestDistance = float.MaxValue;
            StorageBase closestStorage = null;

            for (var i = 0; i < hitCount; i++)
            {
                var storage = _colliders[i].GetComponent<StorageBase>();

                if (storage is not null && storage != _characterStorage)
                {
                    var distance = Vector3.Distance(transform.position, storage.transform.position);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestStorage = storage;
                    }
                }
            }

            return closestStorage;
        }

        private void TransferBetweenStorages(StorageBase source, StorageBase destination, ItemType itemType)
        {
            if (source is null || destination == null) return;

            if (!source.CanRemoveItem() || !destination.CanAddItem()) return;

            var topItem = source.PeekItem();
            if (topItem is null || topItem.itemType != itemType || !topItem.IsPlaced)
                return;

            if (destination is CharacterStorage characterStorage && !characterStorage.CanCarryItemType(itemType))
            {
                return;
            }

            if (!source.TryRemoveItem(out var item)) return;
            {
                item.SetPlaced(false);

                if (!destination.TryAddItem(item))
                {
                    // Eğer hedef depoda yer yoksa itemi geriye koy
                    source.TryAddItem(item);
                }
            }
        }
    }
}