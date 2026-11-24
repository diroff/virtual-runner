using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Abstractions.UI
{
    [DisallowMultipleComponent]
    public abstract class UIPanelBase : MonoBehaviour
    {
        [SerializeField] private RectTransform _root;
        [SerializeField] private UIPanelNavigator _navigator;

        private readonly HashSet<object> _blockOwners = new();
        private UIPanelAnimationBase[] _animations;
        private CanvasGroup _canvasGroup;
        private CancellationTokenSource _transition;
        private bool _isInitialized;
        private bool _isOpen;
        private bool _isBlocked;
        private bool _isApplyingVisibility;
        private bool _isDestroyed;

        public RectTransform Root => _root;
        public CanvasGroup CanvasGroup => _canvasGroup;
        public virtual UIPanelNavigationType NavigationType => UIPanelNavigationType.Window;
        public bool IsOpen => _isOpen;
        public bool IsTransitioning => _transition != null;
        public bool IsBlocked => _isBlocked || _blockOwners.Count > 0;
        public bool IsInteractable => IsOpen && !IsTransitioning && !IsBlocked &&
                                      _root != null && _root.gameObject.activeInHierarchy;

        public event Action OnShowing;
        public event Action OnHiding;
        public event Action OnShown;
        public event Action OnHidden;
        public event Action<bool> OnBlockedChanged;

        protected virtual void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (_isInitialized)
                return;

            if (_root == null)
                _root = transform as RectTransform;

            if (_root == null)
                throw new InvalidOperationException($"{GetType().Name}: RectTransform is not assigned.");

            _canvasGroup = _root.GetComponent<CanvasGroup>();

            if (_canvasGroup == null)
                _canvasGroup = _root.gameObject.AddComponent<CanvasGroup>();

            var animations = new List<UIPanelAnimationBase>(GetComponents<UIPanelAnimationBase>());

            if (_root.gameObject != gameObject)
                animations.AddRange(_root.GetComponents<UIPanelAnimationBase>());

            _animations = animations.ToArray();
            _isOpen = _root.gameObject.activeSelf;
            _isInitialized = true;

            OnInitialize();
            SetAnimationState(_isOpen);
            RefreshInteraction();
        }

        protected virtual void OnInitialize() { }
        protected virtual void OnBeforeShow() { }
        protected virtual void OnBeforeHide() { }

        protected virtual void OnEnable()
        {
            if (_isInitialized && !IsTransitioning)
                RefreshInteraction();
        }

        protected virtual void OnDisable()
        {
            if (_isApplyingVisibility || !_isInitialized || _root == null)
                return;

            CancelTransition();
            SetAnimationState(_isOpen);
            RefreshInteraction();

            if (_root.gameObject != gameObject)
                SetRootActive(_isOpen);
        }

        protected virtual void OnDestroy()
        {
            _isDestroyed = true;
            CancelTransition();
            _blockOwners.Clear();
        }

        public UniTask Show()
        {
            return ShowAsync();
        }

        public UniTask Hide()
        {
            return HideAsync();
        }

        public UniTask ShowAsync(CancellationToken cancellationToken = default)
        {
            return _navigator != null
                ? _navigator.OpenAsync(this, cancellationToken)
                : ShowDirectAsync(cancellationToken);
        }

        public UniTask HideAsync(CancellationToken cancellationToken = default)
        {
            return _navigator != null
                ? _navigator.CloseAsync(this, cancellationToken)
                : HideDirectAsync(cancellationToken);
        }

        public void SetBlocked(bool value)
        {
            Initialize();
            bool wasBlocked = IsBlocked;
            _isBlocked = value;
            RefreshBlockState(wasBlocked);
        }

        public void SetBlocked(object owner, bool value)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));

            Initialize();
            bool wasBlocked = IsBlocked;

            if (value)
                _blockOwners.Add(owner);
            else
                _blockOwners.Remove(owner);

            RefreshBlockState(wasBlocked);
        }

        private void RefreshBlockState(bool wasBlocked)
        {
            RefreshInteraction();

            if (wasBlocked != IsBlocked)
                OnBlockedChanged?.Invoke(IsBlocked);
        }

        internal UniTask ShowDirectAsync(CancellationToken cancellationToken)
        {
            return SetVisibilityAsync(true, cancellationToken);
        }

        internal UniTask HideDirectAsync(CancellationToken cancellationToken)
        {
            return SetVisibilityAsync(false, cancellationToken);
        }

        internal void SetVisibilityImmediate(bool isVisible)
        {
            Initialize();
            CancelTransition();
            bool changed = _isOpen != isVisible;
            ApplyStableState(isVisible);

            if (changed)
                NotifyVisibility(isVisible);
        }

        private async UniTask SetVisibilityAsync(bool isVisible, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Initialize();

            if (_isDestroyed)
                return;

            if (!IsTransitioning && _isOpen == isVisible && _root.gameObject.activeSelf == isVisible)
                return;

            CancelTransition();
            var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken,
                this.GetCancellationTokenOnDestroy());
            var token = source.Token;
            _transition = source;
            Sequence sequence = null;
            bool completed = false;

            try
            {
                RefreshInteraction();

                if (isVisible)
                {
                    OnShowing?.Invoke();
                    token.ThrowIfCancellationRequested();
                    OnBeforeShow();
                    token.ThrowIfCancellationRequested();
                    SetRootActive(true);
                }
                else
                {
                    OnHiding?.Invoke();
                    token.ThrowIfCancellationRequested();
                    OnBeforeHide();
                }

                token.ThrowIfCancellationRequested();
                RefreshInteraction();
                sequence = BuildSequence(isVisible);

                if (sequence != null)
                    await sequence.ToUniTask(cancellationToken: token);

                token.ThrowIfCancellationRequested();
                completed = true;
            }
            finally
            {
                sequence?.Kill();

                if (_transition == source)
                {
                    _transition = null;
                    source.Dispose();

                    if (!_isDestroyed && _root != null)
                        ApplyStableState(completed ? isVisible : _isOpen);
                }
            }

            if (completed && !_isDestroyed && !IsTransitioning && _isOpen == isVisible)
                NotifyVisibility(isVisible);
        }

        private void NotifyVisibility(bool isVisible)
        {
            if (isVisible)
                OnShown?.Invoke();
            else
                OnHidden?.Invoke();
        }

        private Sequence BuildSequence(bool isVisible)
        {
            Sequence sequence = null;

            try
            {
                foreach (var animation in _animations)
                {
                    if (animation == null || !animation.enabled)
                        continue;

                    animation.Initialize(this);
                    var tween = animation.BuildTween(isVisible);

                    if (tween == null)
                        continue;

                    if (sequence == null)
                        sequence = DOTween.Sequence().Pause().SetUpdate(true).SetRecyclable(false);

                    sequence.Join(tween);
                }

                sequence?.Play();
                return sequence;
            }
            catch
            {
                sequence?.Kill();
                throw;
            }
        }

        private void SetAnimationState(bool isVisible)
        {
            foreach (var animation in _animations)
            {
                if (animation == null || !animation.enabled)
                    continue;

                animation.Initialize(this);
                animation.SetState(isVisible);
            }
        }

        private void ApplyStableState(bool isVisible)
        {
            _isOpen = isVisible;
            SetAnimationState(isVisible);
            RefreshInteraction();
            SetRootActive(isVisible);
        }

        private void SetRootActive(bool value)
        {
            if (_root.gameObject.activeSelf == value)
                return;

            _isApplyingVisibility = true;

            try
            {
                _root.gameObject.SetActive(value);
            }
            finally
            {
                _isApplyingVisibility = false;
            }
        }

        private void RefreshInteraction()
        {
            if (_canvasGroup == null)
                return;

            bool canInteract = _isOpen && !IsTransitioning && !IsBlocked && enabled;
            _canvasGroup.interactable = canInteract;
            _canvasGroup.blocksRaycasts = canInteract;
        }

        private void CancelTransition()
        {
            var source = _transition;
            _transition = null;
            source?.Cancel();
            source?.Dispose();
        }
    }
}
