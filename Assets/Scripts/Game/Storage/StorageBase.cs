using System.Collections.Generic;
using UnityEngine;
using Game.Common;

namespace Game.Storage
{
    public abstract class StorageBase : MonoBehaviour
    {
        [SerializeField] protected Transform itemHolder;

        protected int MaxCapacity = 50;

        protected readonly Stack<Item> Items = new();
        protected ItemType CurrentItemType { get; private set; } = ItemType.None;

        public bool CanAddItem()
        {
            return Items.Count < MaxCapacity;
        }

        public bool CanRemoveItem()
        {
            return Items.Count > 0;
        }

        public bool TryAddItem(Item item)
        {
            if (!CanAddItem() || item is null)
                return false;

            if (Items.Count > 0 && CurrentItemType != ItemType.None && item.itemType != CurrentItemType)
            {
                return false;
            }

            PositionItem(item);

            if (Items.Count == 1 && CurrentItemType == ItemType.None)
            {
                CurrentItemType = item.itemType;
            }

            return true;
        }

        public Item PeekItem()
        {
            return CanRemoveItem() ? Items.Peek() : null;
        }

        public void PushItem(Item item)
        {
            if (item is null) return;

            Items.Push(item);

            if (Items.Count == 1)
            {
                CurrentItemType = item.itemType;
            }
        }

        public virtual bool TryRemoveItem(out Item item)
        {
            if (!CanRemoveItem())
            {
                item = null;
                return false;
            }

            item = Items.Pop();

            if (Items.Count == 0)
            {
                CurrentItemType = ItemType.None;
            }

            return true;
        }

        public int GetCurrentCount()
        {
            return Items.Count;
        }

        public Transform GetItemHolder()
        {
            return itemHolder;
        }

        protected abstract void PositionItem(Item item);
    }
}