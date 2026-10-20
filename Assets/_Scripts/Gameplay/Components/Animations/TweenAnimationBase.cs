using DG.Tweening;
using UnityEngine;

namespace Gameplay.Components.Animations
{
    public abstract class TweenAnimationBase : MonoBehaviour
    {
        [SerializeField] protected Transform _target;
        [SerializeField] private bool _playOnAwake = true;

        protected Tween Tween;

        protected virtual void Reset()
        {
            _target = transform;
        }

        protected virtual void Awake()
        {
            if (_target == null)
                _target = transform;

            CaptureState();
            RebuildTween(false);

            if (_playOnAwake)
                Play();
        }

        protected virtual void OnDestroy()
        {
            Tween?.Kill();
        }

        protected abstract void CaptureState();

        protected abstract void RestoreState();

        protected abstract Tween BuildTween();

        protected void RebuildTween(bool preservePlayState = true)
        {
            bool wasPlaying = preservePlayState && IsPlaying();

            Tween?.Kill();
            Tween = BuildTween().Pause();

            if (wasPlaying)
                Tween.Play();
        }

        public void Play()
        {
            if (Tween == null || !Tween.IsActive())
                RebuildTween(false);

            Tween.Play();
        }

        public void Stop(bool restoreState = false)
        {
            if (Tween == null)
                return;

            Tween.Pause();

            if (restoreState)
                RestoreState();
        }

        public bool IsPlaying()
        {
            return Tween != null && Tween.IsPlaying();
        }
    }
}