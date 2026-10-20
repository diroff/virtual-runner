using Abstractions.Interfaces;
using Data;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Abstractions.Components.Inventory
{
    public abstract class InventoryComponent : MonoBehaviour
    {
        [SerializeField] protected List<ItemStack> _items = new();

        public event Action<ItemStack> OnItemAdded;
        public event Action<ItemStack> OnItemRemoved;
        public event Action OnInventoryChanged;

        #region Public API

        public bool CanAdd(ItemDefinition item, int amount)
        {
            return item != null && amount > 0;
        }

        public bool CanRemove(ItemDefinition item, int amount)
        {
            return GetCount(item) >= amount;
        }

        public bool Add(ItemDefinition item, int amount = 1)
        {
            if (!CanAdd(item, amount))
                return false;

            AddInternal(item, amount);
            return true;
        }

        public void Add(ItemDefinition item)
        {
            Add(item, 1);
        }

        public bool Remove(ItemDefinition item, int amount = 1)
        {
            if (!CanRemove(item, amount))
                return false;

            RemoveInternal(item, amount);
            return true;
        }

        public int GetCount(ItemDefinition item)
        {
            int total = 0;

            foreach (var stack in _items)
                if (stack.Item == item)
                    total += stack.Count;

            return total;
        }

        public bool HasItem(ItemDefinition item)
        {
            return GetCount(item) > 0;
        }

        public bool HasItem(ItemDefinition item, int amount)
        {
            return GetCount(item) >= amount;
        }

        public bool HasItems(params ItemDefinition[] items)
        {
            foreach (var item in items)
            {
                if (!HasItem(item))
                    return false;
            }

            return true;
        }

        public bool HasItems(IEnumerable<ItemDefinition> items)
        {
            foreach (var item in items)
            {
                if (!HasItem(item))
                    return false;
            }

            return true;
        }

        public IReadOnlyList<ItemStack> GetItems() => _items;

        #endregion

        #region Internal logic

        private void AddInternal(ItemDefinition item, int amount)
        {
            if (item.IsStackable)
                amount = FillExistingStacks(item, amount);

            CreateNewStacks(item, amount);

            OnInventoryChanged?.Invoke();
        }

        private int FillExistingStacks(ItemDefinition item, int amount)
        {
            foreach (var stack in _items)
            {
                if (stack.Item != item)
                    continue;

                amount -= AddToStack(stack, amount, item.MaxStack);

                if (amount <= 0)
                    break;
            }

            return amount;
        }

        private int AddToStack(ItemStack stack, int amount, int maxStack)
        {
            int added = Mathf.Min(amount, maxStack - stack.Count);

            if (added <= 0)
                return 0;

            stack.Count += added;

            OnItemAdded?.Invoke(stack);

            return added;
        }

        private void CreateNewStacks(ItemDefinition item, int amount)
        {
            while (amount > 0)
            {
                int stackSize = item.IsStackable ? Mathf.Min(amount, item.MaxStack): 1;

                var stack = new ItemStack(item, stackSize);

                _items.Add(stack);

                OnItemAdded?.Invoke(stack);

                amount -= stackSize;
            }
        }

        private void RemoveInternal(ItemDefinition item, int amount)
        {
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                var stack = _items[i];
                if (stack.Item != item) continue;

                int take = Mathf.Min(amount, stack.Count);

                stack.Count -= take;
                amount -= take;

                if (stack.Count <= 0)
                {
                    _items.RemoveAt(i);
                    OnItemRemoved?.Invoke(stack);
                }

                if (amount <= 0)
                {
                    OnInventoryChanged?.Invoke();
                    return;
                }
            }

            OnInventoryChanged?.Invoke();
        }

        private int GetFreeStackSpace(ItemDefinition item)
        {
            int free = 0;

            foreach (var stack in _items)
            {
                if (stack.Item != item) continue;
                free += item.MaxStack - stack.Count;
            }

            return free;
        }

        #endregion
    }
}