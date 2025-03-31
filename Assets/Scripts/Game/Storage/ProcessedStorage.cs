using Game.Common;
namespace Game.Storage
{
    public class ProcessedStorage : StorageBase
    {
        private void Awake()
        {
            MaxCapacity = 50;
        }

        protected override void PositionItem(Item item)
        {
        }
    }
}