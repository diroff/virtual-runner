using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data.PowerUps;
using Gameplay.Mechanics.PowerUps.Lifetimes;
using Gameplay.Mechanics.PowerUps.Magnet;
using Gameplay.Mechanics.PowerUps.Magnet.Visuals;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Mechanics.PowerUps.Effects
{
    public sealed class MagnetBeamEffect : PowerUpEffectBase
    {
        [SerializeField] private InputActionReference _holdAction;
        [SerializeField] private Transform _beamOrigin;
        [SerializeField] private MagnetTargetingBase _targeting;
        [SerializeField] private MagnetAreaVisualBase _areaVisual;
        [SerializeField, Min(0.1f)] private float _pullSpeed = 10f;
        [SerializeField, Min(0.01f)] private float _collectRadius = 0.35f;

        private readonly List<MagnetAttractable> _found = new();
        private readonly Dictionary<MagnetAttractable, Vector3> _locked = new();
        private readonly List<MagnetAttractable> _release = new();

        private CancellationTokenSource _cts;
        private TimedPowerUpLifetime _lifetime;
        private PowerUpContext _context;
        private bool _actionEnabledHere;
        private bool _beamActive;

        protected override bool OnStart(PowerUpContext context, PowerUpLevelSettings settings, PowerUpLifetimeBase lifetime)
        {
            if (_beamOrigin == null || _targeting == null || _holdAction == null ||
                _holdAction.action == null || lifetime is not TimedPowerUpLifetime timedLifetime)
                return false;

            if (_areaVisual != null)
            {
                if (!_areaVisual.StartVisual(_targeting, _beamOrigin))
                    return false;

                _areaVisual.StopVisual();
            }

            _context = context;
            _lifetime = timedLifetime;
            _lifetime.Pause();

            InputAction action = _holdAction.action;
            _actionEnabledHere = !action.enabled;
            if (_actionEnabledHere)
                action.Enable();

            action.performed += HandleHoldChanged;
            action.canceled += HandleHoldChanged;

            if (action.IsPressed())
                StartBeam();

            return true;
        }

        protected override void OnStop()
        {
            InputAction action = _holdAction != null ? _holdAction.action : null;
            if (action != null)
            {
                action.performed -= HandleHoldChanged;
                action.canceled -= HandleHoldChanged;

                if (_actionEnabledHere)
                    action.Disable();
            }

            _actionEnabledHere = false;
            StopBeam();
            _context = null;
            _lifetime = null;
        }

        private void HandleHoldChanged(InputAction.CallbackContext input)
        {
            if (input.action.IsPressed())
                StartBeam();
            else
                StopBeam();
        }

        private void StartBeam()
        {
            if (_beamActive || _context == null || _lifetime == null || !_lifetime.IsRunning)
                return;

            if (_areaVisual != null && !_areaVisual.StartVisual(_targeting, _beamOrigin))
                return;

            _beamActive = true;
            _lifetime.Resume();
            _cts = new CancellationTokenSource();
            UpdateBeam(_cts.Token).Forget();
        }

        private void StopBeam()
        {
            if (!_beamActive)
                return;

            _beamActive = false;
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            _areaVisual?.StopVisual();
            _locked.Clear();
            _found.Clear();
            _release.Clear();
            _lifetime?.Pause();
        }

        private async UniTask UpdateBeam(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    _areaVisual?.UpdateVisual(_beamOrigin);
                    _targeting.FindTargets(_beamOrigin, _found);
                    foreach (MagnetAttractable attractable in _found)
                        if (attractable != null && !_locked.ContainsKey(attractable))
                            _locked.Add(attractable, attractable.transform.position);

                    _release.Clear();
                    foreach (KeyValuePair<MagnetAttractable, Vector3> entry in _locked)
                    {
                        MagnetAttractable item = entry.Key;
                        if (item == null || !item.gameObject.activeInHierarchy ||
                            !_targeting.IsAimedAt(_beamOrigin, entry.Value) ||
                            !item.AttractTo(_context.CollectionTarget.position, _pullSpeed, _collectRadius,
                                _context.Player, Time.deltaTime))
                            _release.Add(item);
                    }

                    foreach (MagnetAttractable item in _release)
                        _locked.Remove(item);

                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
            catch (OperationCanceledException) { }
        }
    }
}
