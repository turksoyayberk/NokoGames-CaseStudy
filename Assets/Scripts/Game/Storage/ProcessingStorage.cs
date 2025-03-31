using Core;
using Utilities;
using Game.Common;
using Zenject;

namespace Game.Storage
{
    public class ProcessingStorage : StorageBase
    {
        private const float HorizontalSpacing = 0.4f;
        private const float VerticalSpacing = 0.4f;

        private const float AnimationDuration = 0.2f;

        [Inject] private SoundManager _soundManager;

        private void Awake()
        {
            MaxCapacity = 50;
        }

        protected override void PositionItem(Item item)
        {
            if (itemHolder is null || item is null)
                return;

            _soundManager.PlaySound(SoundType.Collect);

            PushItem(item);

            var itemIndex = Items.Count - 1;

            var heightPerLayer = item.GetColliderBoundSizeY();

            var targetPosition = PositionUtils.CalculateGridPosition(
                itemIndex,
                VerticalSpacing,
                HorizontalSpacing,
                heightPerLayer
            );

            item.transform.SetParent(itemHolder);

            AnimationUtils.MoveItemWithBounce(null, item, targetPosition, AnimationDuration);
        }
    }
}