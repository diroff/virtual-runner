using System;

namespace Gameplay.Models
{
    public class Wallet
    {
        public long Balance { get; private set; }

        public event Action<long, long> OnBalanceChanged;
        public event Action<long> OnMoneyAdded;
        public event Action<long> OnMoneyRemoved;

        public Wallet(long initialBalance = 0)
        {
            Balance = Math.Max(0, initialBalance);
        }

        public bool CanAfford(long amount)
        {
            return amount >= 0 && Balance >= amount;
        }

        public void Add(long amount)
        {
            if (amount <= 0)
                return;

            long previousBalance = Balance;
            Balance = checked(Balance + amount);

            OnBalanceChanged?.Invoke(previousBalance, Balance);
            OnMoneyAdded?.Invoke(amount);
        }

        public void Remove(long amount)
        {
            if (amount <= 0)
                return;

            long removedAmount = Math.Min(amount, Balance);

            if (removedAmount == 0)
                return;

            long previousBalance = Balance;
            Balance -= removedAmount;

            OnBalanceChanged?.Invoke(previousBalance, Balance);
            OnMoneyRemoved?.Invoke(removedAmount);
        }

        public bool TrySpend(long amount)
        {
            if (amount <= 0 || !CanAfford(amount))
                return false;

            Remove(amount);
            return true;
        }

        public void SetBalance(long balance)
        {
            balance = Math.Max(0, balance);

            if (Balance == balance)
                return;

            long previousBalance = Balance;
            long difference = balance - previousBalance;

            Balance = balance;
            OnBalanceChanged?.Invoke(previousBalance, Balance);

            if (difference > 0)
                OnMoneyAdded?.Invoke(difference);
            else
                OnMoneyRemoved?.Invoke(-difference);
        }

        public void Reset()
        {
            SetBalance(0);
        }

        public long TransferTo(Wallet destination)
        {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            if (destination == this || Balance <= 0)
                return 0;

            long transferredAmount = Balance;

            destination.Add(transferredAmount);
            Reset();

            return transferredAmount;
        }
    }
}