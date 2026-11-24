using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Abstractions.UI
{
    public abstract class UIButtonAnimationBase : MonoBehaviour
    {
        [SerializeField] private UIButtonBase _button;

        protected UIButtonBase Button => _button;
        protected Tween Tween { get; private set; }
        private bool _isInitialized;

        protected virtual void OnEnable()
        {
            if (_button == null)
                _button = GetComponentInParent<UIButtonBase>();

            if (_button == null)
                return;

            _button.Initialize();

            if (_button.TargetButton == null)
                return;

            _isInitialized = InitializeAnimation();

            if (!_isInitialized)
                return;

            _button.RegisterAnimation(this);
            _button.OnStateChanged += OnButtonStateChanged;
            _button.OnClicked += OnButtonClicked;
            OnButtonStateChanged();
        }

        protected virtual void OnDisable()
        {
            if (!_isInitialized)
                return;

            _isInitialized = false;

            if (_button != null)
            {
                _button.UnregisterAnimation(this);
                _button.OnStateChanged -= OnButtonStateChanged;
                _button.OnClicked -= OnButtonClicked;
            }

            StopTween();
            RestoreState();
        }

        protected abstract bool InitializeAnimation();
        protected abstract void RestoreState();
        protected virtual void OnButtonStateChanged() { }
        protected virtual void OnButtonClicked() { }

        protected virtual UniTask<bool> OnBeforeButtonClickAsync(CancellationToken cancellationToken)
        {
            return UniTask.FromResult(true);
        }

        internal async UniTask<bool> PlayBeforeClickAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_isInitialized || !isActiveAndEnabled)
                return true;

            return await OnBeforeButtonClickAsync(cancellationToken);
        }

        protected void PlayTween(Tween tween)
        {
            PrepareTween(tween);
            StartTween(tween);
        }

        protected async UniTask<bool> PlayTweenAsync(Tween tween, CancellationToken cancellationToken)
        {
            if (tween == null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return true;
            }

            bool completed = false;
            bool waitRegistered = false;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                PrepareTween(tween, true);
                tween.onComplete += () => completed = true;

                var completion = tween.ToUniTask(cancellationToken: cancellationToken);
                waitRegistered = true;
                StartTween(tween);

                await completion;
                cancellationToken.ThrowIfCancellationRequested();
                return completed;
            }
            finally
            {
                if (Tween == tween)
                    Tween = null;

                if (!waitRegistered && tween.IsActive())
                    tween.Kill();
            }
        }

        private void PrepareTween(Tween tween, bool waitForCompletion = false)
        {
            StopTween();

            if (tween == null)
                return;

            Tween = tween.Pause().SetUpdate(true).SetRecyclable(false);

            if (waitForCompletion)
                tween.SetAutoKill(true);

            tween.onKill += () =>
            {
                if (Tween == tween)
                    Tween = null;
            };
        }

        private void StartTween(Tween tween)
        {
            if (tween != null && Tween == tween && tween.IsActive())
                tween.Play();
        }

        protected void StopTween()
        {
            var tween = Tween;
            Tween = null;
            tween?.Kill();
        }
    }
}
