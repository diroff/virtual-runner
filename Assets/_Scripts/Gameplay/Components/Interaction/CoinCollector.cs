using System;
using UnityEngine;
using Gameplay.Mechanics.Collectables;

namespace Gameplay.Components.Interaction
{
    public class CoinCollector : MonoBehaviour
    {
        public event Action<Coin> OnCoinCollected;

        public void Collect(Coin coin)
        {
            if (coin == null)
                return;

            OnCoinCollected?.Invoke(coin);
        }
    }
}