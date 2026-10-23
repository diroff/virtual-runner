using System;
using System.Collections.Generic;
using Abstraction.Interfaces;
using Abstractions.Interfaces;
using UnityEngine;

namespace Gameplay.Mechanics.LevelGeneration
{
    public class LevelSegment : MonoBehaviour, IPoolable
    {
        [Serializable]
        private sealed class Variant
        {
            [SerializeField] private SegmentDifficulty _difficulty;
            [SerializeField] private GameObject _contentRoot;

            public SegmentDifficulty Difficulty => _difficulty;
            public GameObject ContentRoot => _contentRoot;
        }

        [Header("Segment")]
        [SerializeField, Min(0.1f)] private float _length = 20f;

        [Header("Variants")]
        [SerializeField] private Variant[] _variants = Array.Empty<Variant>();

        private int _activeVariantIndex = -1;

        private IRespawnable[] _sharedRespawnables = Array.Empty<IRespawnable>();
        private IRespawnable[][] _variantRespawnables = Array.Empty<IRespawnable[]>();

        public float Length => _length;

        public void OnCreated()
        {
            CacheRespawnables();
            SetActiveVariant(-1);
        }

        public void OnSpawned()
        {
            RespawnAll(_sharedRespawnables);

            if (_activeVariantIndex >= 0 && _activeVariantIndex < _variantRespawnables.Length)
                RespawnAll(_variantRespawnables[_activeVariantIndex]);
        }

        public void OnDespawned()
        {

        }

        public SegmentDifficulty GetVariantCandidates(SegmentDifficulty requestedDifficulty, List<int> candidateIndices)
        {
            candidateIndices.Clear();

            if (_variants == null || _variants.Length == 0)
                return requestedDifficulty;

            AddCandidateIndices((int)requestedDifficulty, candidateIndices);

            if (candidateIndices.Count > 0)
                return requestedDifficulty;

            int requestedRank = (int)requestedDifficulty;
            int resolvedRank = int.MaxValue;

            for (int i = 0; i < _variants.Length; i++)
            {
                if (!IsValidVariant(i))
                    continue;

                int rank = (int)_variants[i].Difficulty;

                if (rank > requestedRank && rank < resolvedRank)
                    resolvedRank = rank;
            }

            if (resolvedRank == int.MaxValue)
            {
                resolvedRank = int.MinValue;

                for (int i = 0; i < _variants.Length; i++)
                {
                    if (!IsValidVariant(i))
                        continue;

                    int rank = (int)_variants[i].Difficulty;

                    if (rank < requestedRank && rank > resolvedRank)
                        resolvedRank = rank;
                }
            }

            if (resolvedRank == int.MinValue)
                return requestedDifficulty;

            AddCandidateIndices(resolvedRank, candidateIndices);

            return (SegmentDifficulty)resolvedRank;
        }

        public void PrepareForSpawn(int variantIndex, Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            SetActiveVariant(variantIndex);
        }

        private void CacheRespawnables()
        {
            if (_variants == null)
                _variants = Array.Empty<Variant>();

            List<IRespawnable> shared = new();
            List<IRespawnable>[] variants = new List<IRespawnable>[_variants.Length];

            for (int i = 0; i < variants.Length; i++)
                variants[i] = new List<IRespawnable>();

            MonoBehaviour[] behaviours = GetComponentsInChildren<MonoBehaviour>(true);

            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour == null || behaviour is not IRespawnable respawnable)
                    continue;

                int variantIndex = FindVariantIndex(behaviour.transform);

                if (variantIndex >= 0)
                    variants[variantIndex].Add(respawnable);
                else
                    shared.Add(respawnable);
            }

            _sharedRespawnables = shared.ToArray();
            _variantRespawnables = new IRespawnable[variants.Length][];

            for (int i = 0; i < variants.Length; i++)
                _variantRespawnables[i] = variants[i].ToArray();
        }

        private void AddCandidateIndices(int difficultyRank, List<int> candidateIndices)
        {
            for (int i = 0; i < _variants.Length; i++)
            {
                if (IsValidVariant(i) && (int)_variants[i].Difficulty == difficultyRank)
                    candidateIndices.Add(i);
            }
        }

        private int FindVariantIndex(Transform target)
        {
            for (int i = 0; i < _variants.Length; i++)
            {
                if (IsValidVariant(i) && (target == _variants[i].ContentRoot.transform || target.IsChildOf(_variants[i].ContentRoot.transform)))
                    return i;
            }

            return -1;
        }

        private bool IsValidVariant(int index)
        {
            if (index < 0 || index >= _variants.Length || _variants[index] == null)
                return false;

            GameObject contentRoot = _variants[index].ContentRoot;

            return contentRoot != null && contentRoot != gameObject && contentRoot.transform.IsChildOf(transform);
        }

        private void SetActiveVariant(int variantIndex)
        {
            if (variantIndex < -1 || variantIndex >= _variants.Length || (variantIndex >= 0 && !IsValidVariant(variantIndex)))
                variantIndex = -1;

            _activeVariantIndex = variantIndex;

            for (int i = 0; i < _variants.Length; i++)
            {
                if (!IsValidVariant(i))
                    continue;

                bool isActive = i == _activeVariantIndex;

                if (_variants[i].ContentRoot.activeSelf != isActive)
                    _variants[i].ContentRoot.SetActive(isActive);
            }
        }

        private static void RespawnAll(IRespawnable[] respawnables)
        {
            foreach (IRespawnable respawnable in respawnables)
                respawnable.Respawn();
        }

        private void OnValidate()
        {
            _length = Mathf.Max(0.1f, _length);

            if (_variants == null)
                _variants = Array.Empty<Variant>();
        }
    }
}