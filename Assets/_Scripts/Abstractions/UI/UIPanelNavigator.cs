using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Abstractions.UI
{
    public sealed class UIPanelNavigator : MonoBehaviour
    {
        [SerializeField] private UIPanelBase _initialWindow;
        [SerializeField] private bool _bringPopupsToFront = true;

        private readonly List<UIPanelBase> _windows = new();
        private readonly List<UIPanelBase> _popups = new();
        private readonly HashSet<UIPanelBase> _ownedPanels = new();
        private bool _isNavigating;
        private bool _isDestroyed;
        private bool _isInitialized;

        public UIPanelBase CurrentWindow => GetLast(_windows);
        public UIPanelBase CurrentPopup => GetLast(_popups);
        public UIPanelBase CurrentPanel => CurrentPopup != null ? CurrentPopup : CurrentWindow;
        public bool IsNavigating => _isNavigating;

        public event Action OnNavigationChanged;

        private void Start()
        {
            Initialize();

            if (_initialWindow != null && CurrentPanel == null)
                OpenAsync(_initialWindow).Forget();
        }

        private void LateUpdate()
        {
            if (!_isNavigating && (_windows.Exists(panel => panel == null) ||
                                   _popups.Exists(panel => panel == null)))
                NavigateAsync((windows, popups) => { }, null, default).Forget();
        }

        public void Initialize()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;

            if (_initialWindow == null)
                return;

            if (_initialWindow.NavigationType != UIPanelNavigationType.Window)
                throw new InvalidOperationException("Initial panel must be a window.");

            _initialWindow.Initialize();

            if (_initialWindow.IsOpen)
                _windows.Add(_initialWindow);
        }

        public UniTask OpenAsync(UIPanelBase panel, CancellationToken cancellationToken = default)
        {
            if (panel == null)
                throw new ArgumentNullException(nameof(panel));

            return NavigateAsync((windows, popups) =>
            {
                if (panel.NavigationType == UIPanelNavigationType.Popup)
                {
                    popups.Remove(panel);
                    popups.Add(panel);
                    return;
                }

                popups.Clear();
                int index = windows.IndexOf(panel);

                if (index >= 0)
                    windows.RemoveRange(index + 1, windows.Count - index - 1);
                else
                    windows.Add(panel);
            }, panel, cancellationToken);
        }

        public UniTask CloseAsync(UIPanelBase panel, CancellationToken cancellationToken = default)
        {
            if (panel == null)
                throw new ArgumentNullException(nameof(panel));

            return NavigateAsync((windows, popups) => RemovePanel(windows, popups, panel), panel,
                cancellationToken);
        }

        public UniTask BackAsync(CancellationToken cancellationToken = default)
        {
            return NavigateAsync((windows, popups) =>
            {
                if (popups.Count > 0)
                    popups.RemoveAt(popups.Count - 1);
                else if (windows.Count > 0)
                    windows.RemoveAt(windows.Count - 1);
            }, null, cancellationToken);
        }

        public UniTask CloseAllAsync(CancellationToken cancellationToken = default)
        {
            return NavigateAsync((windows, popups) =>
            {
                windows.Clear();
                popups.Clear();
            }, null, cancellationToken);
        }

        private async UniTask NavigateAsync(Action<List<UIPanelBase>, List<UIPanelBase>> change,
            UIPanelBase additionalPanel, CancellationToken cancellationToken)
        {
            Initialize();

            using var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken,
                this.GetCancellationTokenOnDestroy());
            var token = source.Token;

            if (_isNavigating)
                await UniTask.WaitUntil(() => !_isNavigating, cancellationToken: token);

            token.ThrowIfCancellationRequested();
            _isNavigating = true;

            var states = new Dictionary<UIPanelBase, bool>();
            bool committed = false;

            try
            {
                RemoveDestroyedPanels(_windows);
                RemoveDestroyedPanels(_popups);
                var windows = new List<UIPanelBase>(_windows);
                var popups = new List<UIPanelBase>(_popups);
                change(windows, popups);

                CaptureStates(states, _windows);
                CaptureStates(states, _popups);
                CaptureStates(states, windows);
                CaptureStates(states, popups);
                CaptureState(states, additionalPanel);

                foreach (var panel in states.Keys)
                    BlockPanel(panel);

                var visible = GetVisiblePanels(windows, popups);
                var previous = GetVisiblePanels(_windows, _popups);

                for (int i = previous.Count - 1; i >= 0; i--)
                {
                    var panel = previous[i];

                    if (panel != null && !visible.Contains(panel))
                        await panel.HideDirectAsync(token);
                }

                foreach (var panel in states.Keys)
                {
                    if (panel != null && !visible.Contains(panel) && !previous.Contains(panel))
                        await panel.HideDirectAsync(token);
                }

                foreach (var panel in visible)
                {
                    token.ThrowIfCancellationRequested();

                    if (panel == null)
                        continue;

                    if (_bringPopupsToFront && panel.NavigationType == UIPanelNavigationType.Popup)
                        panel.Root.SetAsLastSibling();

                    if (!panel.IsOpen || panel.IsTransitioning || !panel.Root.gameObject.activeSelf)
                        await panel.ShowDirectAsync(token);
                }

                token.ThrowIfCancellationRequested();
                RemoveDestroyedPanels(windows);
                RemoveDestroyedPanels(popups);
                _windows.Clear();
                _windows.AddRange(windows);
                _popups.Clear();
                _popups.AddRange(popups);
                committed = true;
            }
            finally
            {
                try
                {
                    if (!_isDestroyed)
                    {
                        if (!committed)
                        {
                            foreach (var state in states)
                            {
                                if (state.Key == null)
                                    continue;

                                try
                                {
                                    state.Key.SetVisibilityImmediate(state.Value);
                                }
                                catch (Exception exception)
                                {
                                    Debug.LogException(exception, state.Key);
                                }
                            }
                        }

                        if (!committed && _bringPopupsToFront)
                        {
                            foreach (var popup in _popups)
                            {
                                if (popup != null)
                                    popup.Root.SetAsLastSibling();
                            }
                        }

                        RefreshBlocks();
                    }
                }
                finally
                {
                    _isNavigating = false;
                }
            }

            if (committed && !_isDestroyed)
                OnNavigationChanged?.Invoke();
        }

        private void BlockPanel(UIPanelBase panel)
        {
            if (panel == null)
                return;

            _ownedPanels.Add(panel);
            panel.SetBlocked(this, true);
        }

        private void RefreshBlocks()
        {
            RemoveDestroyedPanels(_windows);
            RemoveDestroyedPanels(_popups);
            var current = CurrentPanel;

            foreach (var panel in new List<UIPanelBase>(_ownedPanels))
            {
                bool keepBlocked = panel != null && panel != current &&
                                   (_windows.Contains(panel) || _popups.Contains(panel));

                if (keepBlocked)
                    continue;

                _ownedPanels.Remove(panel);

                if (panel != null)
                    panel.SetBlocked(this, false);
            }

            foreach (var panel in _windows)
            {
                if (panel != current)
                    BlockPanel(panel);
            }

            foreach (var panel in _popups)
            {
                if (panel != current)
                    BlockPanel(panel);
            }
        }

        private static void CaptureStates(Dictionary<UIPanelBase, bool> states, List<UIPanelBase> panels)
        {
            foreach (var panel in panels)
                CaptureState(states, panel);
        }

        private static void CaptureState(Dictionary<UIPanelBase, bool> states, UIPanelBase panel)
        {
            if (panel == null || states.ContainsKey(panel))
                return;

            panel.Initialize();
            states.Add(panel, panel.IsOpen);
        }

        private static List<UIPanelBase> GetVisiblePanels(List<UIPanelBase> windows, List<UIPanelBase> popups)
        {
            var visible = new List<UIPanelBase>();
            var window = GetLast(windows);

            if (window != null)
                visible.Add(window);

            visible.AddRange(popups);
            return visible;
        }

        private static void RemovePanel(List<UIPanelBase> windows, List<UIPanelBase> popups, UIPanelBase panel)
        {
            if (GetLast(windows) == panel)
                popups.Clear();

            windows.Remove(panel);
            popups.Remove(panel);
        }

        private static UIPanelBase GetLast(List<UIPanelBase> panels)
        {
            for (int i = panels.Count - 1; i >= 0; i--)
            {
                if (panels[i] != null)
                    return panels[i];
            }

            return null;
        }

        private static void RemoveDestroyedPanels(List<UIPanelBase> panels)
        {
            panels.RemoveAll(panel => panel == null);
        }

        private void OnDestroy()
        {
            _isDestroyed = true;

            foreach (var panel in new List<UIPanelBase>(_ownedPanels))
            {
                if (panel != null)
                    panel.SetBlocked(this, false);
            }

            _ownedPanels.Clear();
        }
    }
}
