using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using Data.PowerUps;
using Gameplay.Models;

namespace Services
{
    public sealed class PowerUpUpgradeService
    {
        private readonly SaveLoadService _saveLoad;
        private readonly WalletService _wallet;
        private bool _isBuying;

        public PowerUpProgression Progression { get; }

        public PowerUpUpgradeService(SaveLoadService saveLoad, WalletService wallet)
        {
            _saveLoad = saveLoad ?? throw new ArgumentNullException(nameof(saveLoad));
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            _saveLoad.Data.PowerUps ??= new PowerUpProgressData();
            _saveLoad.Data.PowerUps.Levels ??= new System.Collections.Generic.Dictionary<string, int>();
            Progression = new PowerUpProgression(_saveLoad.Data.PowerUps.Levels);
        }

        public async UniTask<bool> TryBuyNextLevelAsync(PowerUpDefinition definition)
        {
            if (_isBuying || definition == null || definition.MaxLevel == 0 ||
                string.IsNullOrWhiteSpace(definition.Id))
                return false;

            int currentLevel = Progression.GetLevel(definition);
            if (currentLevel >= definition.MaxLevel)
                return false;

            int nextLevel = currentLevel + 1;
            long price = definition.GetSettings(nextLevel).UpgradeCost;
            if (price <= 0 || !_wallet.TrySpend(price))
                return false;

            _isBuying = true;
            try
            {
                Progression.TrySetLevel(definition, nextLevel);
                await _saveLoad.SaveAsync();
                return true;
            }
            catch
            {
                Progression.TrySetLevel(definition, currentLevel);
                _wallet.Add(price);
                throw;
            }
            finally
            {
                _isBuying = false;
            }
        }
    }
}
