using UnityEngine;

namespace Gameplay.Mechanics.PowerUps.Weapons
{
    public class BlasterView : MonoBehaviour
    {
        [SerializeField] private BlasterBase _blaster;
        [SerializeField] private ParticleSystem _shotEffect;

        private void OnEnable()
        {
            _blaster.OnShot += OnShot;
        }

        private void OnDisable()
        {
            _blaster.OnShot -= OnShot;
        }

        private void OnShot()
        {
            _shotEffect?.Play();
        }
    }
}