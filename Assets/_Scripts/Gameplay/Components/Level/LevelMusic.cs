using Reflex.Attributes;
using Services;
using UnityEngine;

namespace Gameplay.Components.Level
{
    public class LevelMusic : MonoBehaviour
    {
        [SerializeField] private AudioClip _musicClip;

        [Inject] private AudioMusicService _audioMusicService;

        private void Start()
        {
            _audioMusicService.PlayMusic(_musicClip);
        }
    }
}