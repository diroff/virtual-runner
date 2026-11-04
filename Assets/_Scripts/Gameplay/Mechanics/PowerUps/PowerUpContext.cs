using Gameplay.Models;
using UnityEngine;

namespace Gameplay.Mechanics.PowerUps
{
    public sealed class PowerUpContext : MonoBehaviour
    {
        [SerializeField] private GameObject _player;
        [SerializeField] private Transform _collectionTarget;

        public GameObject Player => _player != null ? _player : gameObject;
        public Transform CollectionTarget => _collectionTarget != null ? _collectionTarget : transform;
        public Score Score { get; private set; }

        public void Initialize(Score score)
        {
            Score = score;
        }
    }
}
