using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Common.Components.UI.Animations.Buttons
{
    [DisallowMultipleComponent]
    public sealed class UIButtonClickScaleAnimation : UIButtonScaleAnimationBase
    {
        [SerializeField] private bool _waitForCompletion = true;
        [SerializeField, Min(0f)] private float _scaleMultiplier = 1.12f;
        [SerializeField, Min(0f)] private float _expandDuration = 0.07f;
        [SerializeField, Min(0f)] private float _returnDuration = 0.15f;
        [SerializeField] private Ease _returnEase = Ease.OutQuad;

        protected override async UniTask<bool> OnBeforeButtonClickAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            StopTween();
            SetMultiplier(1f);

            if (_expandDuration <= 0f && _returnDuration <= 0f)
                return true;

            var sequence = BuildSequence();

            if (_waitForCompletion)
            {
                try
                {
                    return await PlayTweenAsync(sequence, cancellationToken);
                }
                finally
                {
                    SetMultiplier(1f);
                }
            }

            PlayTween(sequence);
            return true;
        }

        private Sequence BuildSequence()
        {
            var sequence = DOTween.Sequence()
                .Pause()
                .SetAutoKill(true)
                .Append(BuildScaleTween(_scaleMultiplier, _expandDuration, Ease.OutQuad))
                .Append(BuildScaleTween(1f, _returnDuration, _returnEase));

            if (!_waitForCompletion)
                sequence.OnKill(() => SetMultiplier(1f));

            return sequence;
        }
    }
}
