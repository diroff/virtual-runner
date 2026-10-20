using Abstractions.Interfaces;
using System;

namespace Data
{
    [Serializable]
    public class ItemStack
    {
        public ItemDefinition Item;
        public int Count;

        public ItemStack(ItemDefinition item, int count)
        {
            Item = item;
            Count = count;
        }

        public bool IsEmpty => Item == null || Count <= 0;
    }
}