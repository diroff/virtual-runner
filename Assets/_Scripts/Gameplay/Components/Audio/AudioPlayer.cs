using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Gameplay.Components.Audio
{
    public class AudioPlayer : MonoBehaviour
    {
        [SerializeField] private AudioSource _source;

        [Header("Clips")]
        [SerializeField] private List<AudioClip> _clips;

        [Header("Settings")]
        [SerializeField] private bool _randomClip = true;
        [SerializeField] private bool _randomPitch;
        [SerializeField] private Vector2 _pitchRange = new(0.95f, 1.05f);

        protected bool HasClips => _clips != null && _clips.Count > 0;

        protected virtual void Awake()
        {
            if (_source == null)
                _source = GetComponent<AudioSource>();
        }

        private void Start()
        {
            if (_source.outputAudioMixerGroup == null)
                Debug.LogError("No mixer group!");
        }

        private void Reset()
        {
            _source = GetComponent<AudioSource>();
        }

        public void Play()
        {
            Play(GetNextClip());
        }

        public void PlayOneShot()
        {
            PlayOneShot(GetNextClip());
        }

        public void Play(AudioClip clip)
        {
            if (clip == null || _source == null)
                return;

            PrepareSource();

            _source.clip = clip;
            _source.Play();
        }

        public void PlayOneShot(AudioClip clip)
        {
            if (clip == null || _source == null)
                return;

            PrepareSource();
            _source.PlayOneShot(clip);
        }

        public void Play(IEnumerable<AudioClip> clips)
        {
            if (clips == null)
                return;

            foreach (AudioClip clip in clips)
                PlayOneShot(clip);
        }

        public UniTask PlayAsync(CancellationToken cancellationToken = default)
        {
            return PlayAsync(GetNextClip(), cancellationToken);
        }

        public async UniTask PlayAsync(AudioClip clip, CancellationToken cancellationToken = default)
        {
            if (clip == null || _source == null)
                return;

            cancellationToken.ThrowIfCancellationRequested();

            Play(clip);

            if (!_source.isPlaying)
                return;

            await UniTask.WaitWhile(() => _source != null && _source.isPlaying,cancellationToken: cancellationToken);
        }

        public void Stop()
        {
            if (_source == null)
                return;

            _source.Stop();
        }

        private void PrepareSource()
        {
            _source.pitch = _randomPitch ? Random.Range(_pitchRange.x, _pitchRange.y) : 1f;
        }

        private AudioClip GetNextClip()
        {
            if (!HasClips)
                return null;

            if (!_randomClip || _clips.Count == 1)
                return _clips[0];

            return _clips[Random.Range(0, _clips.Count)];
        }
    }
}