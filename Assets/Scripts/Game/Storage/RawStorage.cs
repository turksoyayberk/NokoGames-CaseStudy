using Utilities;
using Game.Common;

namespace Game.Storage
{
    public class RawStorage : StorageBase
    {
        private const float HorizontalSpacing = 0.4f;
        private const float VerticalSpacing = 0.4f;
        private const float AnimationDuration = 0.2f;

        private void Awake()
        {
            MaxCapacity = 50;
        }

        protected override void PositionItem(Item item)
        {
            if (itemHolder is null || item is null)
                return;

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