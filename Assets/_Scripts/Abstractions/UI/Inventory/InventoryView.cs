using Abstractions.Components.Inventory;
using UnityEngine;

namespace Abstractions.UI.Inventory
{
    public abstract class InventoryView : MonoBehaviour
    {
        protected InventoryComponent Inventory;

        public virtual void Bind(InventoryComponent inventory)
        {
            if (Inventory != null)
                Unsubscribe();

            Inventory = inventory;

            if (Inventory != null)
            {
                Subscribe();
                SyncFull();
            }
        }

        protected virtual void Subscribe()
        {
            Inventory.OnInventoryChanged += SyncDiff;
        }

        protected virtual void Unsubscribe()
        {
            Inventory.OnInventoryChanged -= SyncDiff;
        }

        protected abstract void SyncFull();
        protected abstract void SyncDiff();
    }
}