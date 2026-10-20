using Abstractions.Components.View;
using TMPro;
using UnityEngine;

namespace Gameplay.Components.View
{
    public class HealthTextView : BaseHealthView
    {
        [SerializeField] protected TMP_Text Text;

        protected virtual void Awake()
        {
            if (Text == null)
                Text = GetComponentInChildren<TMP_Text>();
        }

        protected override void UpdateHealthView(float currentValue, float maxValue)
        {
            Text.text = $"{currentValue:0.#}/{maxValue:0}";
        }
    }
}