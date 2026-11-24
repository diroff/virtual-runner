using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Abstractions.UI
{
    [DisallowMultipleComponent]
    public sealed class UIButtonInput : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
    {
        private readonly HashSet<int> _hoveredPointers = new();
        private readonly HashSet<int> _pressedPointers = new();
        private Button _button;

        public event Action OnStateChanged;
        public bool IsHovered => _hoveredPointers.Count > 0;
        public bool IsPressed => _pressedPointers.Overlaps(_hoveredPointers);
        public bool IsSelected => EventSystem.current != null &&
                                  EventSystem.current.currentSelectedGameObject == gameObject;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnDisable()
        {
            _hoveredPointers.Clear();
            _pressedPointers.Clear();
            OnStateChanged?.Invoke();
        }

        private void LateUpdate()
        {
            if (_button != null && _button.isActiveAndEnabled && _button.IsInteractable())
                return;

            if (_pressedPointers.Count == 0)
                return;

            _pressedPointers.Clear();
            OnStateChanged?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hoveredPointers.Add(eventData.pointerId);
            OnStateChanged?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hoveredPointers.Remove(eventData.pointerId);
            OnStateChanged?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || _button == null ||
                !_button.isActiveAndEnabled || !_button.IsInteractable())
                return;

            _hoveredPointers.Add(eventData.pointerId);
            _pressedPointers.Add(eventData.pointerId);
            OnStateChanged?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            _pressedPointers.Remove(eventData.pointerId);
            OnStateChanged?.Invoke();
        }

        public void OnSelect(BaseEventData eventData)
        {
            OnStateChanged?.Invoke();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            OnStateChanged?.Invoke();
        }
    }
}
