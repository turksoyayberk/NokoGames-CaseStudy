using Core;
using UnityEngine;
using Utilities;
using Zenject;
using Game.Common;
using Pool;

namespace Game.Storage
{
    public class TrashStorage : StorageBase
    {
        private ItemPool _itemPool;
        private SoundManager _soundManager;

        [Inject]
        private void Constructor(ItemPool itemPool, SoundManager soundManager)
        {
            _itemPool = itemPool;
            _soundManager = soundManager;
        }

        protected override void PositionItem(Item item)
        {
            if (itemHolder is null || item is null)
                return;

            _soundManager.PlaySound(SoundType.Coins);

            EventBus.Publish(new GameEvents.CoinAmountEvent());

            AnimationUtils.MoveItemWithBounce(_itemPool.ReturnItem, item, Vector3.zero, 0.1f);
            item.transform.SetParent(itemHolder);
        }
    }
}