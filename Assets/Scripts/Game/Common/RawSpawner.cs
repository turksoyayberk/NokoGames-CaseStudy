using System.Collections;
using UnityEngine;
using Zenject;
using Pool;
using Game.Storage;

namespace Game.Common
{
    public class RawSpawner : MonoBehaviour
    {
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private ItemSO rawItemType;

        private const float SpawnInterval = 0.15f;

        private bool _isActive;
        private Coroutine _activeSpawnRoutine;
        private Animator _animator;
        private RawStorage _targetStorage;
        private ItemPool _itemPool;

        [Inject]
        private void Constructor(RawStorage rawStorage, ItemPool itemPool)
        {
            _targetStorage = rawStorage;
            _itemPool = itemPool;
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GameEvents.RawSpawnerStartEvent>(StartSpawning);
        }

        private void OnDisable()
        {
            EventBus.UnSubscribe<GameEvents.RawSpawnerStartEvent>(StartSpawning);
        }

        private void Start()
        {
            StartSpawning();
        }

        public void StartSpawning()
        {
            if (_isActive || !_targetStorage.CanAddItem()) return;

            _animator.enabled = true;
            _isActive = true;
            _activeSpawnRoutine = StartCoroutine(SpawnItemsRoutine());
        }

        public void StopSpawning()
        {
            if (!_isActive || _activeSpawnRoutine == null) return;

            _isActive = false;
            _animator.enabled = false;
            StopCoroutine(_activeSpawnRoutine);
            _activeSpawnRoutine = null;
        }

        private IEnumerator SpawnItemsRoutine()
        {
            while (_isActive)
            {
                if (_targetStorage.CanAddItem())
                {
                    SpawnAndStoreItem();
                }
                else
                {
                    StopSpawning();
                    break;
                }

                yield return new WaitForSeconds(SpawnInterval);
            }
        }

        private void SpawnAndStoreItem()
        {
            var item = _itemPool.SpawnItem(rawItemType, spawnPoint.position, Quaternion.identity);
            _targetStorage.TryAddItem(item);
        }
    }
}