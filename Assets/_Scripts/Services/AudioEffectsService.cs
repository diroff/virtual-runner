using Abstractions.Interfaces;
using Cysharp.Threading.Tasks;
using Gameplay.Components.Audio;
using UnityEngine;

namespace Services
{
    public class AudioEffectsService : MonoBehaviour, IService
    {
        [SerializeField] private AudioPlayer _uiNavigationPlayer;
        [SerializeField] private AudioPlayer _uiConfirmPlayer;
        [SerializeField] private AudioPlayer _uiCancelPlayer;
        [SerializeField] private AudioPlayer _tvNoicePlayer;

        private AudioSource _audioSource;

        public UniTask Initialize()
        {
            _audioSource = GetComponent<AudioSource>();

            DontDestroyOnLoad(gameObject);
            return UniTask.CompletedTask;
        }

        public void PlayUINavigation()
        {
            _uiNavigationPlayer?.PlayOneShot();
        }

        public void PlayUIConfirm()
        {
            _uiConfirmPlayer?.PlayOneShot();
        }

        public void PlayUICancel()
        {
            _uiCancelPlayer?.PlayOneShot();
        }

        public void PlayTvNoise()
        {
            _tvNoicePlayer?.PlayOneShot();
        }
    }
}