using Abstractions.Components.Movement;
using Cysharp.Threading.Tasks;
using Gameplay.Mechanics.LevelGeneration;
using System;
using UnityEngine;

namespace Tests
{
    public class TransformMovementDebug : MonoBehaviour
    {
        [SerializeField] private MovementBase _movement;
        [SerializeField] private LevelSegmentSpawner _levelSpawner;

        private void Start()
        {
            if (_movement == null)
            {
                Debug.LogError($"{nameof(TransformMovementDebug)}: Movement is not assigned.", this);
                return;
            }

            _levelSpawner.StartRunAsync().Forget();
            _movement.StartMovement();
        }

        private void OnDisable()
        {
            if (_movement != null)
                _movement.StopMovement();
        }
    }
}