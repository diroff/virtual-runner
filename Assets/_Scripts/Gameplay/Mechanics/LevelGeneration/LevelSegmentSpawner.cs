using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Services;
using UnityEngine;

namespace Gameplay.Mechanics.LevelGeneration
{
    public class LevelSegmentSpawner : MonoBehaviour
    {
        private readonly struct ActiveSegment
        {
            public LevelSegment Segment { get; }
            public float StartDistance { get; }
            public float EndDistance => StartDistance + Segment.Length;

            public ActiveSegment(LevelSegment segment, float startDistance)
            {
                Segment = segment;
                StartDistance = startDistance;
            }
        }

        private readonly struct VariantBagKey : IEquatable<VariantBagKey>
        {
            private readonly LevelSegment _prefab;
            private readonly SegmentDifficulty _difficulty;

            public VariantBagKey(LevelSegment prefab, SegmentDifficulty difficulty)
            {
                _prefab = prefab;
                _difficulty = difficulty;
            }

            public bool Equals(VariantBagKey other)
            {
                return _prefab == other._prefab && _difficulty == other._difficulty;
            }

            public override bool Equals(object obj)
            {
                return obj is VariantBagKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return ((_prefab != null ? _prefab.GetInstanceID() : 0) * 397) ^ (int)_difficulty;
            }
        }

        [Header("References")]
        [SerializeField] private PoolService _poolService;
        [SerializeField] private Transform _playerRoot;
        [SerializeField] private Transform _spawnOrigin;

        [Header("Segments")]
        [SerializeField] private LevelSegment _startSegmentPrefab;
        [SerializeField] private List<LevelSegment> _segmentPrefabs = new();
        [SerializeField, Min(0)] private int _preloadCountPerPrefab = 3;
        [SerializeField, Min(1)] private int _preloadBatchSize = 2;

        [Header("Generation")]
        [SerializeField, Min(0.1f)] private float _spawnAheadDistance = 80f;
        [SerializeField, Min(0f)] private float _despawnBehindDistance = 30f;
        [SerializeField] private SegmentDifficulty _initialDifficulty = SegmentDifficulty.Easy;

        private readonly List<LevelSegment> _availableSegmentPrefabs = new();
        private readonly List<ActiveSegment> _activeSegments = new();
        private readonly List<int> _variantCandidates = new();

        private readonly ShuffleBag<LevelSegment> _segmentBag = new();
        private readonly Dictionary<VariantBagKey, ShuffleBag<int>> _variantBags = new();

        private CancellationTokenSource _startCancellation;

        private Vector3 _runOriginPosition;
        private Vector3 _runForward;
        private Quaternion _runRotation;
        private float _playerDistance;
        private SegmentDifficulty _currentDifficulty;

        private bool _isStarting;
        private bool _isRunning;

        public bool IsRunning => _isRunning;
        public int ActiveSegmentCount => _activeSegments.Count;
        public SegmentDifficulty CurrentDifficulty => _currentDifficulty;

        public async UniTask StartRunAsync(CancellationToken cancellationToken = default)
        {
            if (_isStarting)
                return;

            if (_isRunning)
                StopRun();

            if (!ValidateSetup())
                return;

            _isStarting = true;

            CancellationTokenSource startCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _startCancellation = startCancellation;

            try
            {
                await PreloadPoolsAsync(startCancellation.Token);
                startCancellation.Token.ThrowIfCancellationRequested();

                Transform origin = _spawnOrigin != null ? _spawnOrigin : transform;

                _runOriginPosition = origin.position;
                _runForward = Vector3.ProjectOnPlane(origin.forward, Vector3.up).normalized;
                _runRotation = Quaternion.LookRotation(_runForward, Vector3.up);

                _currentDifficulty = _initialDifficulty;

                _segmentBag.Reset();
                _variantBags.Clear();

                _playerDistance = GetPlayerDistance();
                _isRunning = true;

                if (!SpawnSegment(_startSegmentPrefab, 0f, _currentDifficulty))
                {
                    _isRunning = false;
                    return;
                }

                RecyclePassedSegments();
                EnsureSegmentsAhead();
            }
            catch (OperationCanceledException) when (startCancellation.IsCancellationRequested)
            {

            }
            catch
            {
                _isRunning = false;
                ReturnActiveSegments();
                throw;
            }
            finally
            {
                _isStarting = false;

                if (ReferenceEquals(_startCancellation, startCancellation))
                    _startCancellation = null;

                startCancellation.Dispose();
            }
        }

        public void StopRun()
        {
            _startCancellation?.Cancel();
            _isRunning = false;

            ReturnActiveSegments();
        }

        public void SetDifficulty(SegmentDifficulty difficulty)
        {
            _currentDifficulty = difficulty;
        }

        private void Update()
        {
            if (!_isRunning)
                return;

            if (_playerRoot == null || _poolService == null)
            {
                StopRun();
                return;
            }

            _playerDistance = GetPlayerDistance();

            RecyclePassedSegments();
            EnsureSegmentsAhead();
        }

        private async UniTask PreloadPoolsAsync(CancellationToken cancellationToken)
        {
            HashSet<GameObject> uniquePrefabs = new();
            List<GameObject> prefabs = new();

            AddPrefab(_startSegmentPrefab);

            foreach (LevelSegment prefab in _availableSegmentPrefabs)
                AddPrefab(prefab);

            for (int i = 0; i < prefabs.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await _poolService.PreloadAsync(prefabs[i], _preloadCountPerPrefab, _preloadBatchSize, cancellationToken);

                if (i < prefabs.Count - 1)
                    await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, cancellationToken);
            }

            void AddPrefab(LevelSegment segmentPrefab)
            {
                if (segmentPrefab != null && uniquePrefabs.Add(segmentPrefab.gameObject))
                    prefabs.Add(segmentPrefab.gameObject);
            }
        }

        private bool SpawnSegment(LevelSegment prefab, float startDistance, SegmentDifficulty requestedDifficulty)
        {
            if (prefab == null || prefab.Length <= 0f)
                return false;

            SegmentDifficulty resolvedDifficulty = prefab.GetVariantCandidates(requestedDifficulty, _variantCandidates);
            int variantIndex = -1;

            if (_variantCandidates.Count > 0)
            {
                VariantBagKey key = new(prefab, resolvedDifficulty);

                if (!_variantBags.TryGetValue(key, out ShuffleBag<int> bag))
                {
                    bag = new ShuffleBag<int>();
                    _variantBags.Add(key, bag);
                }

                variantIndex = bag.Next(_variantCandidates);
            }

            Vector3 position = _runOriginPosition + _runForward * startDistance;

            LevelSegment instance = _poolService.Get(prefab, segment => segment.PrepareForSpawn(variantIndex, position, _runRotation));

            if (instance == null)
                return false;

            _activeSegments.Add(new ActiveSegment(instance, startDistance));

            return true;
        }

        private void EnsureSegmentsAhead()
        {
            float targetDistance = _playerDistance + _spawnAheadDistance;

            while (_activeSegments.Count == 0 || _activeSegments[_activeSegments.Count - 1].EndDistance <= targetDistance)
            {
                float startDistance = _activeSegments.Count == 0 ? 0f : _activeSegments[_activeSegments.Count - 1].EndDistance;
                LevelSegment prefab = _segmentBag.Next(_availableSegmentPrefabs);

                if (!SpawnSegment(prefab, startDistance, _currentDifficulty))
                    break;
            }
        }

        private void RecyclePassedSegments()
        {
            while (_activeSegments.Count > 1 && _activeSegments[0].EndDistance < _playerDistance - _despawnBehindDistance)
            {
                LevelSegment segment = _activeSegments[0].Segment;

                if (_poolService != null)
                    _poolService.Return(segment);
                else if (segment != null)
                    segment.gameObject.SetActive(false);

                _activeSegments.RemoveAt(0);
            }
        }

        private float GetPlayerDistance()
        {
            return Vector3.Dot(_playerRoot.position - _runOriginPosition, _runForward);
        }

        private void ReturnActiveSegments()
        {
            for (int i = _activeSegments.Count - 1; i >= 0; i--)
            {
                LevelSegment segment = _activeSegments[i].Segment;

                if (segment == null)
                    continue;

                if (_poolService != null)
                    _poolService.Return(segment);
                else
                    segment.gameObject.SetActive(false);
            }

            _activeSegments.Clear();
        }

        private bool ValidateSetup()
        {
            if (_poolService == null)
            {
                Debug.LogError($"{nameof(LevelSegmentSpawner)}: PoolService is not assigned.", this);
                return false;
            }

            if (_playerRoot == null)
            {
                Debug.LogError($"{nameof(LevelSegmentSpawner)}: Player Root is not assigned.", this);
                return false;
            }

            if (_startSegmentPrefab == null || _startSegmentPrefab.Length <= 0f)
            {
                Debug.LogError($"{nameof(LevelSegmentSpawner)}: Start segment is missing or has an invalid length.", this);
                return false;
            }

            Transform origin = _spawnOrigin != null ? _spawnOrigin : transform;
            Vector3 forward = Vector3.ProjectOnPlane(origin.forward, Vector3.up);

            if (forward.sqrMagnitude <= Mathf.Epsilon)
            {
                Debug.LogError($"{nameof(LevelSegmentSpawner)}: Spawn Origin must point horizontally along the track.", this);
                return false;
            }

            _availableSegmentPrefabs.Clear();

            HashSet<LevelSegment> uniquePrefabs = new();

            foreach (LevelSegment prefab in _segmentPrefabs)
            {
                if (prefab == null || !uniquePrefabs.Add(prefab))
                    continue;

                if (prefab.Length <= 0f)
                {
                    Debug.LogError($"{nameof(LevelSegmentSpawner)}: Segment {prefab.name} has an invalid length.", prefab);
                    return false;
                }

                _availableSegmentPrefabs.Add(prefab);
            }

            if (_availableSegmentPrefabs.Count == 0)
            {
                Debug.LogError($"{nameof(LevelSegmentSpawner)}: No track segment prefabs are assigned.", this);
                return false;
            }

            return true;
        }

        private void OnDisable()
        {
            StopRun();
        }

        private void OnValidate()
        {
            _preloadCountPerPrefab = Mathf.Max(0, _preloadCountPerPrefab);
            _preloadBatchSize = Mathf.Max(1, _preloadBatchSize);
            _spawnAheadDistance = Mathf.Max(0.1f, _spawnAheadDistance);
            _despawnBehindDistance = Mathf.Max(0f, _despawnBehindDistance);
        }
    }
}