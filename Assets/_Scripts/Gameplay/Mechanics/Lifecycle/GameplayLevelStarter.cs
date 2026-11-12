using Abstractions.Components.Movement;
using Abstractions.Mechanics.Lifecycle;
using Cysharp.Threading.Tasks;
using Gameplay.Mechanics.Collectables;
using Gameplay.Mechanics.Creatures;
using Gameplay.Mechanics.LevelGeneration;
using Gameplay.Mechanics.PowerUps;
using Gameplay.Mechanics.Scoring;
using Gameplay.Models;
using Reflex.Attributes;
using System.Threading;
using UnityEngine;

namespace Gameplay.Mechanics.Lifecycle
{
    public class GameplayLevelStarter : LevelStarter
    {
        [Inject] private Player _player;
        [Inject] private LevelSegmentSpawner _levelSpawner;
        [Inject] private CoinCollectionHandler _coinCollectionHandler;
        [Inject] private DistanceScoreTracker _distanceScoreTracker;

        [Inject] private PowerUpContext _powerUpContext;
        [Inject] private PowerUpHandler _powerUpHandler;
        [Inject] private MovementBase _movement;

        private Score _score;
        private Wallet _wallet;


        protected override void OnBeforeStart()
        {
            base.OnBeforeStart();

            _score = new Score();
            _wallet = new Wallet();

            _coinCollectionHandler.Initialize(_wallet);
            _distanceScoreTracker.Initialize(_score);

            _powerUpContext.Initialize(_score);
        }

        protected override async UniTask OnStartProcessAsync(CancellationToken cancellationToken)
        {
            _powerUpHandler.StartHandling();

            await _levelSpawner.StartRunAsync(cancellationToken);

            if (!_levelSpawner.IsRunning || cancellationToken.IsCancellationRequested)
            {
                _powerUpHandler.StopHandling();
                return;
            }

            if (!_movement.StartMovement())
            {
                _levelSpawner.StopRun();
                _powerUpHandler.StopHandling();
                return;
            }

            _distanceScoreTracker.StartScoring();
        }
    }
}