using Gameplay.Components.Interaction;
using Gameplay.Models;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Mechanics.Collectables
{
    public class CoinCollectionHandler : MonoBehaviour
    {
        [Header("Collection")]
        [SerializeField] private List<CoinCollector> _coinCollectors = new();
        [SerializeField, Min(0.01f)] private float _multiplier = 1f;

        private readonly HashSet<CoinCollector> _registeredCollectors = new();
        private Wallet _sessionWallet;
        private readonly Dictionary<UnityEngine.Object, float> _multiplierContributions = new();
        private float _baseMultiplier;

        public float Multiplier => _multiplier;
        public IReadOnlyCollection<CoinCollector> Collectors => _registeredCollectors;

        public event Action<long> OnCoinsCollected;
        public event Action<float> OnMultiplierChanged;
        public event Action<CoinCollector> OnCollectorRegistered;
        public event Action<CoinCollector> OnCollectorUnregistered;

        public void Initialize(Wallet sessionWallet)
        {
            _sessionWallet = sessionWallet ?? throw new ArgumentNullException(nameof(sessionWallet));
        }

        private void Awake()
        {
            _baseMultiplier = _multiplier;
        }

        private void OnEnable()
        {
            foreach (CoinCollector collector in _coinCollectors)
                RegisterCollector(collector);
        }

        private void OnDisable()
        {
            foreach (CoinCollector collector in _registeredCollectors)
                collector.OnCoinCollected -= HandleCoinCollected;

            _registeredCollectors.Clear();
        }

        private void OnDestroy()
        {
            foreach (CoinCollector collector in _registeredCollectors)
                collector.OnCoinCollected -= HandleCoinCollected;

            _registeredCollectors.Clear();
        }

        public bool RegisterCollector(CoinCollector collector)
        {
            if (collector == null || !_registeredCollectors.Add(collector))
                return false;

            collector.OnCoinCollected += HandleCoinCollected;
            OnCollectorRegistered?.Invoke(collector);

            return true;
        }

        public bool UnregisterCollector(CoinCollector collector)
        {
            if (collector == null || !_registeredCollectors.Remove(collector))
                return false;

            collector.OnCoinCollected -= HandleCoinCollected;
            OnCollectorUnregistered?.Invoke(collector);

            return true;
        }

        public void SetMultiplier(float multiplier)
        {
            if (float.IsNaN(multiplier) || float.IsInfinity(multiplier) || multiplier <= 0f)
                return;

            _baseMultiplier = multiplier;
            RecalculateMultiplier();
        }

        public void AddMultiplier(float amount)
        {
            if (float.IsNaN(amount) || float.IsInfinity(amount) || amount <= 0f)
                return;

            SetMultiplier(_baseMultiplier + amount);
        }

        public void RemoveMultiplier(float amount)
        {
            if (float.IsNaN(amount) || float.IsInfinity(amount) || amount <= 0f)
                return;

            SetMultiplier(_baseMultiplier - amount);
        }

        public void SetMultiplierContribution(UnityEngine.Object source, float amount)
        {
            if (source == null || float.IsNaN(amount) || float.IsInfinity(amount) || amount <= 0f)
                return;

            _multiplierContributions[source] = amount;
            RecalculateMultiplier();
        }

        public void RemoveMultiplierContribution(UnityEngine.Object source)
        {
            if (source != null && _multiplierContributions.Remove(source))
                RecalculateMultiplier();
        }

        private void RecalculateMultiplier()
        {
            float result = _baseMultiplier;
            foreach (float value in _multiplierContributions.Values)
                result *= value;

            if (Mathf.Approximately(result, _multiplier))
                return;

            _multiplier = result;
            OnMultiplierChanged?.Invoke(_multiplier);
        }

        private void HandleCoinCollected(Coin coin)
        {
            if (_sessionWallet == null || coin == null)
                return;

            long reward = (long)Math.Floor(coin.Value * (double)_multiplier);

            if (reward <= 0)
                return;

            _sessionWallet.Add(reward);
            OnCoinsCollected?.Invoke(reward);
        }
    }
}
