using Abstractions.Components.Collectables;
using Gameplay.Components.Interaction;
using UnityEngine;

namespace Gameplay.Mechanics.Collectables
{
    public class Coin : CollectableBase
    {
        [field: SerializeField, Min(1)] public long Value { get; private set; } = 1;

        protected override void HandleInteraction(GameObject interactor)
        {
            base.HandleInteraction(interactor);

            var collector = interactor.GetComponentInParent<CoinCollector>();
            collector?.Collect(this);
        }
    }
}