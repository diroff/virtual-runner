using Unity.XR.CoreUtils;
using UnityEngine;

namespace Gameplay.Components
{
    public class XRCharacterControllerHeight : MonoBehaviour
    {
        [SerializeField] private XROrigin _xrOrigin;
        [SerializeField] private CharacterController _controller;
        [SerializeField, Min(0.1f)] private float _minHeight = 0.5f;
        [SerializeField, Min(0.1f)] private float _maxHeight = 2.5f;

        private void LateUpdate()
        {
            float height = Mathf.Clamp(
                _xrOrigin.CameraInOriginSpaceHeight,
                Mathf.Max(_minHeight, _controller.radius * 2f),
                _maxHeight
            );

            Vector3 center = _xrOrigin.CameraInOriginSpacePos;
            center.y = height * 0.5f + _controller.skinWidth;

            _controller.height = height;
            _controller.center = center;
        }
    }
}