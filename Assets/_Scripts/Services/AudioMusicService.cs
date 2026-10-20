using Abstractions.Interfaces;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Services
{
    public sealed class AudioMusicService : IService
    {
        private const string SourceObjectName = "MusicAudioSource";

        private readonly AudioMixerGroup _musicMixerGroup;

        private AudioSource _audioSource;

        public AudioClip CurrentTrack => _audioSource != null ? _audioSource.clip : null;

        public bool IsPlaying => _audioSource != null && _audioSource.isPlaying;

        public AudioMusicService(AudioMixerGroup musicMixerGroup)
        {
            if (musicMixerGroup == null)
                throw new ArgumentNullException(nameof(musicMixerGroup));

            _musicMixerGroup = musicMixerGroup;
        }

        public UniTask Initialize()
        {
            if (_audioSource != null)
                return UniTask.CompletedTask;

            var audioObject = new GameObject(SourceObjectName);
            UnityEngine.Object.DontDestroyOnLoad(audioObject);

            _audioSource = audioObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.loop = true;
            _audioSource.volume = 1f;
            _audioSource.spatialBlend = 0f;
            _audioSource.outputAudioMixerGroup = _musicMixerGroup;

            return UniTask.CompletedTask;
        }

        public void PlayMusic(AudioClip clip)
        {
            if (_audioSource == null)
                return;

            if (clip == null)
                return;

            if (_audioSource.isPlaying && _audioSource.clip == clip)
                return;

            _audioSource.Stop();
            _audioSource.clip = clip;
            _audioSource.Play();
        }

        public void StopMusic()
        {
            if (_audioSource == null)
                return;

            _audioSource.Stop();
            _audioSource.clip = null;
        }

        public void PauseMusic()
        {
            if (_audioSource == null || !_audioSource.isPlaying)
                return;

            _audioSource.Pause();
        }

        public void ResumeMusic()
        {
            if (_audioSource == null || _audioSource.clip == null || _audioSource.isPlaying)
                return;

            _audioSource.UnPause();
        }
    }
}