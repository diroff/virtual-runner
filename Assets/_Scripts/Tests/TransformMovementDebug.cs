using Abstractions.Components.Movement;
using Cysharp.Threading.Tasks;
using Gameplay.Mechanics.Collectables;
using Gameplay.Mechanics.LevelGeneration;
using Gameplay.Mechanics.Scoring;
using Gameplay.Models;
using TMPro;
using UnityEngine;

namespace Tests
{
    public class TransformMovementDebug : MonoBehaviour
    {
        [SerializeField] private MovementBase _movement;
        [SerializeField] private LevelSegmentSpawner _levelSpawner;
        [SerializeField] private DistanceScoreTracker _distanceScoreTracker;
        [SerializeField] private CoinCollectionHandler _coinCollectionHandler;

        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _coinsText;

        private Score _score;
        private Wallet _wallet;

        private void Start()
        {
            if (_movement == null || _distanceScoreTracker == null)
            {
                Debug.LogError($"{nameof(TransformMovementDebug)}: Movement or DistanceScoreTracker is not assigned.", this);
                return;
            }

            _score = new Score();
            _score.OnValueChanged += HandleScoreChanged;

            _wallet = new Wallet();
            _wallet.OnBalanceChanged += OnBalanceChanged;

            _coinCollectionHandler.Initialize(_wallet);

            _distanceScoreTracker.Initialize(_score);
            _distanceScoreTracker.OnMultiplierChanged += HandleMultiplierChanged;

            OnBalanceChanged(_wallet.Balance, _wallet.Balance);
            HandleScoreChanged(_score.Value, _score.Value);

            _levelSpawner.StartRunAsync().Forget();

            if (!_movement.StartMovement())
                return;

            _distanceScoreTracker.StartScoring();
        }

        private void OnDisable()
        {
            if (_distanceScoreTracker != null)
            {
                _distanceScoreTracker.StopScoring();
                _distanceScoreTracker.OnMultiplierChanged -= HandleMultiplierChanged;
            }

            if (_movement != null)
                _movement.StopMovement();
        }

        private void OnDestroy()
        {
            if (_score != null)
                _score.OnValueChanged -= HandleScoreChanged;

            if(_wallet != null )
                _wallet.OnBalanceChanged -= OnBalanceChanged;
        }

        private void OnBalanceChanged(long previous, long current)
        {
            _coinsText.text = "Coins: " + current;
            //Debug.Log($"Coin: {previous} -> {current}");
        }

        private void HandleScoreChanged(long previousValue, long currentValue)
        {
            _scoreText.text = "Score: " + currentValue;
            //Debug.Log($"Score: {previousValue} -> {currentValue}");
        }

        private void HandleMultiplierChanged(float multiplier)
        {
           //Debug.Log($"Score multiplier: x{multiplier}");
        }

        [ContextMenu("Add")]
        public void AddMult()
        {
            _coinCollectionHandler.AddMultiplier(1);
        }

        [ContextMenu("Remove")]
        public void RemoveMult()
        {
            _coinCollectionHandler.RemoveMultiplier(1);
        }
    }
}