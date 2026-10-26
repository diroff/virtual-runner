using Abstractions.Interfaces;
using Cysharp.Threading.Tasks;
using Gameplay.Models;
using System;

namespace Services
{
    public class WalletService : IService
    {
        private Wallet _wallet;

        public Wallet Wallet => _wallet;
        public long Balance => _wallet?.Balance ?? 0;

        public event Action<long, long> OnBalanceChanged;
        public event Action<long> OnMoneyAdded;
        public event Action<long> OnMoneyRemoved;

        public UniTask Initialize()
        {
            long savedBalance = LoadBalance();

            _wallet = new Wallet(savedBalance);
            _wallet.OnBalanceChanged += HandleBalanceChanged;
            _wallet.OnMoneyAdded += HandleMoneyAdded;
            _wallet.OnMoneyRemoved += HandleMoneyRemoved;

            return UniTask.CompletedTask;
        }

        public bool CanAfford(long amount)
        {
            return _wallet != null && _wallet.CanAfford(amount);
        }

        public void Add(long amount)
        {
            EnsureInitialized();
            _wallet.Add(amount);
        }

        public void Remove(long amount)
        {
            EnsureInitialized();
            _wallet.Remove(amount);
        }

        public bool TrySpend(long amount)
        {
            EnsureInitialized();
            return _wallet.TrySpend(amount);
        }

        public void SetBalance(long balance)
        {
            EnsureInitialized();
            _wallet.SetBalance(balance);
        }

        private void HandleBalanceChanged(long previousBalance, long currentBalance)
        {
            OnBalanceChanged?.Invoke(previousBalance, currentBalance);
        }

        private void HandleMoneyAdded(long amount)
        {
            OnMoneyAdded?.Invoke(amount);
        }

        private void HandleMoneyRemoved(long amount)
        {
            OnMoneyRemoved?.Invoke(amount);
        }

        private void EnsureInitialized()
        {
            if (_wallet == null)
                throw new InvalidOperationException($"{nameof(WalletService)} has not been initialized.");
        }

        private long LoadBalance()
        {

            return 0;
        }
    }
}