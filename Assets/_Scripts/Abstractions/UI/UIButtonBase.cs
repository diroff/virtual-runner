using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Abstractions.UI
{
    [DisallowMultipleComponent]
    public abstract class UIButtonBase : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private UnityEvent _onClick = new();

        protected Button Button { get; private set; }
        public Button TargetButton => Button;
        public UnityEvent ClickEvent => _onClick;
        public bool IsHovered { get; private set; }
        public bool IsPressed { get; private set; }
        public bool IsSelected { get; private set; }
        public bool IsInteractable { get; private set; }
        public bool IsBusy { get; private set; }
        public bool CanClick => IsInteractable && !IsBusy;

        public event Action OnClickStarted;
        public event Action OnClicked;
        public event Action<bool> OnHoverChanged;
        public event Action<bool> OnPressChanged;
        public event Action<bool> OnSelectionChanged;
        public event Action<bool> OnInteractableChanged;
        public event Action<bool> OnBusyChanged;
        public event Action OnStateChanged;

        private readonly List<UIButtonAnimationBase> _animations = new();
        private UIButtonInput _input;
        private CancellationTokenSource _cts;
        private bool _requestedInteractable;

        public void Initialize()
        {
            if (Button != null)
                return;

            Button = _button != null ? _button : GetComponent<Button>();
        }

        protected virtual void Awake()
        {
            Initialize();
        }

        protected virtual void OnEnable()
        {
            Initialize();

            if (Button == null)
            {
                Debug.LogError($"{GetType().Name}: Button is not assigned.", this);
                return;
            }

            _cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            _input = Button.GetComponent<UIButtonInput>();

            if (_input == null)
                _input = Button.gameObject.AddComponent<UIButtonInput>();

            _input.OnStateChanged += RefreshState;
            Button.onClick.AddListener(InvokeClick);
            RefreshState();
        }

        protected virtual void LateUpdate()
        {
            if (IsBusy && Button != null && Button.interactable)
            {
                _requestedInteractable = true;
                Button.interactable = false;
            }

            RefreshState();
        }

        protected virtual void OnDisable()
        {
            if (Button != null)
                Button.onClick.RemoveListener(InvokeClick);

            if (_input != null)
                _input.OnStateChanged -= RefreshState;

            _input = null;
            CancelClick();
            RefreshState();
        }

        protected virtual void OnDestroy()
        {
            CancelClick();
        }

        public void SetInteractable(bool value)
        {
            Initialize();

            if (Button == null)
                return;

            if (IsBusy)
            {
                _requestedInteractable = value;
                Button.interactable = false;
            }
            else
                Button.interactable = value;

            RefreshState();
        }

        public bool TryClick()
        {
            RefreshState();

            if (!CanClick || _cts == null)
                return false;

            ExecuteClickAsync(_cts).Forget();
            return true;
        }

        private void InvokeClick()
        {
            TryClick();
        }

        internal void RegisterAnimation(UIButtonAnimationBase animation)
        {
            if (!_animations.Contains(animation))
                _animations.Add(animation);
        }

        internal void UnregisterAnimation(UIButtonAnimationBase animation)
        {
            _animations.Remove(animation);
        }

        private async UniTask ExecuteClickAsync(CancellationTokenSource source)
        {
            using var clickSource = CancellationTokenSource.CreateLinkedTokenSource(source.Token);
            var cancellationToken = clickSource.Token;

            try
            {
                SetBusy(true);
                cancellationToken.ThrowIfCancellationRequested();
                OnClickStarted?.Invoke();
                cancellationToken.ThrowIfCancellationRequested();

                if (!await PlayClickAnimationsAsync(cancellationToken))
                    return;

                cancellationToken.ThrowIfCancellationRequested();

                if (!isActiveAndEnabled || Button == null || !Button.isActiveAndEnabled || !_requestedInteractable)
                    return;

                OnClicked?.Invoke();
                cancellationToken.ThrowIfCancellationRequested();
                _onClick?.Invoke();
                cancellationToken.ThrowIfCancellationRequested();
                await OnClickAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            finally
            {
                try
                {
                    clickSource.Cancel();
                }
                finally
                {
                    if (_cts == source)
                        SetBusy(false);
                }
            }
        }

        private async UniTask<bool> PlayClickAnimationsAsync(CancellationToken cancellationToken)
        {
            var animations = _animations.ToArray();
            var tasks = new List<UniTask<bool>>(animations.Length);

            foreach (var animation in animations)
            {
                if (animation != null && animation.isActiveAndEnabled)
                    tasks.Add(animation.PlayBeforeClickAsync(cancellationToken));
            }

            if (tasks.Count == 0)
                return true;

            var results = await UniTask.WhenAll(tasks);

            foreach (bool completed in results)
            {
                if (!completed)
                    return false;
            }

            return true;
        }

        protected virtual UniTask OnClickAsync(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }

        private void CancelClick()
        {
            var source = _cts;
            _cts = null;
            try
            {
                source?.Cancel();
            }
            finally
            {
                source?.Dispose();
                SetBusy(false);
            }
        }

        private void SetBusy(bool value)
        {
            if (IsBusy == value)
                return;

            IsBusy = value;

            if (Button != null)
            {
                if (value)
                {
                    _requestedInteractable = Button.interactable;
                    Button.interactable = false;
                }
                else
                    Button.interactable = _requestedInteractable;
            }

            RefreshState();

            if (IsBusy == value)
                OnBusyChanged?.Invoke(value);
        }

        private void RefreshState()
        {
            bool interactable = isActiveAndEnabled && _cts != null && Button != null &&
                                Button.isActiveAndEnabled && Button.IsInteractable();
            bool hovered = interactable && _input != null && _input.IsHovered;
            bool pressed = interactable && _input != null && _input.IsPressed;
            bool selected = interactable && _input != null && _input.IsSelected;

            bool interactableChanged = IsInteractable != interactable;
            bool hoverChanged = IsHovered != hovered;
            bool pressChanged = IsPressed != pressed;
            bool selectionChanged = IsSelected != selected;

            IsInteractable = interactable;
            IsHovered = hovered;
            IsPressed = pressed;
            IsSelected = selected;

            if (interactableChanged)
                OnInteractableChanged?.Invoke(interactable);
            if (hoverChanged)
                OnHoverChanged?.Invoke(hovered);
            if (pressChanged)
                OnPressChanged?.Invoke(pressed);
            if (selectionChanged)
                OnSelectionChanged?.Invoke(selected);

            if (interactableChanged || hoverChanged || pressChanged || selectionChanged)
                OnStateChanged?.Invoke();
        }
    }
}
