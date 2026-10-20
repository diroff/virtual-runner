using Abstractions.Interfaces;
using UnityEngine;
using UnityEngine.Localization;

namespace Data
{
    [CreateAssetMenu(menuName = "Data/Item/Item definition")]
    public class ItemDefinition : ScriptableObject, IInventoryItem
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public LocalizedString Name { get; private set; }
        [field: SerializeField] public LocalizedString Description { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public int MaxStack { get; private set; } = 1;

        public bool IsStackable => MaxStack > 1;
    }
}