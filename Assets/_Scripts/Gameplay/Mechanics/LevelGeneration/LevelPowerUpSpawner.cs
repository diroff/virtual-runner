using System;
using System.Collections.Generic;
using Data.LevelGeneration;
using Gameplay.Mechanics.PowerUps;
using Reflex.Attributes;
using Services;
using UnityEngine;

namespace Gameplay.Mechanics.LevelGeneration
{
    public sealed class LevelPowerUpSpawner : MonoBehaviour
    {
        private sealed class SpawnedPickup
        {
            public PowerUpPickup Pickup;
            public Action OnHidden;
        }

        [Inject] private LevelSegmentSpawner _segmentSpawner;
        [Inject] private PoolService _poolService;
        [SerializeField] private PowerUpSpawnProfile _profile;
        [SerializeField] private List<PowerUpPickup> _pickupPrefabs = new();
        [SerializeField, Min(0)] private int _preloadCountPerPrefab = 2;

        private readonly Dictionary<LevelSegment, SpawnedPickup> _activePickups = new();
        private readonly List<LevelSegment> _releaseBuffer = new();
        private readonly List<PowerUpPickup> _availablePrefabs = new();
        private readonly ShuffleBag<PowerUpPickup> _prefabBag = new();
        private readonly PowerUpSpawnSelector _selector = new();

        private void OnEnable()
        {
            if (_segmentSpawner == null)
                return;

            _segmentSpawner.OnRunStarted += HandleRunStarted;
            _segmentSpawner.OnRunStopped += ReturnAllPickups;
            _segmentSpawner.OnSegmentSpawned += HandleSegmentSpawned;
            _segmentSpawner.OnSegmentReturned += HandleSegmentReturned;

            ResetSpawning();
        }

        private void OnDisable()
        {
            if (_segmentSpawner != null)
            {
                _segmentSpawner.OnRunStarted -= HandleRunStarted;
                _segmentSpawner.OnRunStopped -= ReturnAllPickups;
                _segmentSpawner.OnSegmentSpawned -= HandleSegmentSpawned;
                _segmentSpawner.OnSegmentReturned -= HandleSegmentReturned;
            }

            ReturnAllPickups();
        }

        private void HandleRunStarted()
        {
            ResetSpawning();

            if (_poolService == null || _preloadCountPerPrefab == 0)
                return;

            foreach (PowerUpPickup prefab in _availablePrefabs)
                _poolService.Preload(prefab.gameObject, _preloadCountPerPrefab);
        }

        private void ResetSpawning()
        {
            ReturnAllPickups();
            _selector.Reset();
            _prefabBag.Reset();
            _availablePrefabs.Clear();

            if (_pickupPrefabs == null)
                return;

            HashSet<PowerUpPickup> unique = new();

            foreach (PowerUpPickup prefab in _pickupPrefabs)
                if (prefab != null && unique.Add(prefab))
                    _availablePrefabs.Add(prefab);
        }

        private void HandleSegmentSpawned(LevelSegment segment, float startDistance)
        {
            if (segment == null || _poolService == null || _profile == null || _availablePrefabs.Count == 0)
                return;

            PowerUpSpawnPoint point = segment.GetComponentInChildren<PowerUpSpawnPoint>();
            if (point == null || !_selector.Next(_profile.Evaluate(startDistance)))
                return;

            PowerUpPickup prefab = _prefabBag.Next(_availablePrefabs);
            PowerUpPickup pickup = _poolService.Get(prefab,
                instance => instance.transform.SetPositionAndRotation(point.transform.position, point.transform.rotation));

            if (pickup == null)
                return;

            SpawnedPickup entry = new() { Pickup = pickup };
            entry.OnHidden = () => ReturnPickup(segment);
            _activePickups.Add(segment, entry);
            pickup.OnHidden += entry.OnHidden;
            pickup.Respawn();
        }

        private void HandleSegmentReturned(LevelSegment segment)
        {
            if (segment != null)
                ReturnPickup(segment);
        }

        private void ReturnPickup(LevelSegment segment)
        {
            if (segment == null || !_activePickups.Remove(segment, out SpawnedPickup entry))
                return;

            if (entry.Pickup == null)
                return;

            entry.Pickup.OnHidden -= entry.OnHidden;
            if (_poolService != null)
                _poolService.Return(entry.Pickup);
            else
                entry.Pickup.gameObject.SetActive(false);
        }

        private void ReturnAllPickups()
        {
            _releaseBuffer.Clear();

            foreach (LevelSegment segment in _activePickups.Keys)
                _releaseBuffer.Add(segment);

            foreach (LevelSegment segment in _releaseBuffer)
                ReturnPickup(segment);

            _releaseBuffer.Clear();
        }
    }
}
