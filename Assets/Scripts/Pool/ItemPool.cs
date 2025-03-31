using UnityEngine;
using Game.Common;

namespace Pool
{
    public class ItemPool : MonoBehaviour
    {
        [SerializeField] private Item itemPrefab;
        
        private const int InitializePoolSize = 60;

        private ObjectPool<Item> _objectPool;

        private void Awake()
        {
            _objectPool = new ObjectPool<Item>(
                itemPrefab,
                InitializePoolSize,
                transform,
                null,
                item => item.ResetItem()
            );
        }

        public void ReturnItem(Item item)
        {
            if (item is null) return;
            _objectPool.Return(item);
        }

        public Item SpawnItem(ItemSO itemType, Vector3 position, Quaternion rotation)
        {
            var item = _objectPool.Get();

            if (item is not null)
            {
                item.transform.position = position;
                item.transform.rotation = rotation;
                item.InitializeItem(itemType);
                return item;
            }

            return null;
        }
    }
}