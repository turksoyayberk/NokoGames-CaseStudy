using Core;
using UnityEngine;
using Utilities;
using Game.Common;
using Zenject;

namespace Game.Storage
{
    public class CharacterStorage : StorageBase
    {
        private const float AnimationDuration = 0.15f;

        [Inject] private SoundManager _soundManager;

        private void Awake()
        {
            MaxCapacity = 50;
        }

        public bool CanCarryItemType(ItemType itemType)
        {
            if (Items.Count == 0 || CurrentItemType == ItemType.None)
                return true;

            return itemType == CurrentItemType;
        }

        protected override void PositionItem(Item item)
        {
            if (itemHolder is null || item is null)
                return;

            _soundManager.PlaySound(SoundType.Collect);

            PushItem(item);

            var targetPosition = new Vector3(
                0,
                item.GetColliderBoundSizeY() * (Items.Count - 1),
                0
            );

            item.transform.SetParent(itemHolder);
            AnimationUtils.MoveItemWithBounce(null, item, targetPosition, AnimationDuration);
        }
    }
}