using System;
using Data.PowerUps;
using Gameplay.Mechanics.PowerUps.Lifetimes;
using UnityEngine;

namespace Gameplay.Mechanics.PowerUps.Effects
{
    public sealed class PowerUpEquipmentVisualEffect : PowerUpEffectBase
    {
        [SerializeField] private Renderer[] _renderers = Array.Empty<Renderer>();
        [SerializeField] private GameObject[] _visualObjects = Array.Empty<GameObject>();

        private void Awake() => SetVisible(false);

        protected override bool OnStart(PowerUpContext context, PowerUpLevelSettings settings, PowerUpLifetimeBase lifetime)
        {
            SetVisible(true);
            return true;
        }

        protected override void OnStop() => SetVisible(false);

        private void SetVisible(bool visible)
        {
            foreach (Renderer renderer in _renderers)
                if (renderer != null)
                    renderer.enabled = visible;

            foreach (GameObject visual in _visualObjects)
                if (visual != null)
                    visual.SetActive(visible);
        }
    }
}
