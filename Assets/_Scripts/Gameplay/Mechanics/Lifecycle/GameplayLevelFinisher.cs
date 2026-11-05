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
    public class GameplayLevelFinisher : LevelFinisher
    {
        [Inject] protected Player Player;
        [Inject] private LevelSegmentSpawner _levelSpawner;
        [Inject] private CoinCollectionHandler _coinCollectionHandler;
        [Inject] private DistanceScoreTracker _distanceScoreTracker;

        private PowerUpHandler _powerUpHandler;
        private MovementBase _movement;

        protected override void Awake()
        {
            base.Awake();

            _movement = Player.GetComponentInChildren<MovementBase>();
            _powerUpHandler = Player.GetComponentInChildren<PowerUpHandler>();
        }

        protected override void OnBeforeFinish()
        {
            base.OnBeforeFinish();

            _powerUpHandler.StopHandling();

            _levelSpawner.StopRun();

            _movement.StopMovement();
            _distanceScoreTracker.StopScoring();
        }

        protected override async UniTask OnFinishProcessAsync(CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
            Debug.Log("Finish");
        }
    }
}