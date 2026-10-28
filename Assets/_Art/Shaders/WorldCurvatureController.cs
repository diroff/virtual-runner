using UnityEngine;

namespace Gameplay.Components.Rendering
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class WorldCurvatureController : MonoBehaviour
    {
        private static readonly int CurveOriginId = Shader.PropertyToID("_VRWorldCurveOriginWS");
        private static readonly int CurveForwardId = Shader.PropertyToID("_VRWorldCurveForwardWS");
        private static readonly int CurveRightId = Shader.PropertyToID("_VRWorldCurveRightWS");
        private static readonly int CurveStrengthId = Shader.PropertyToID("_VRWorldCurveStrength");
        private static readonly int CurveLateralStrengthId = Shader.PropertyToID("_VRWorldCurveLateralStrength");
        private static readonly int CurveBackwardStrengthId = Shader.PropertyToID("_VRWorldCurveBackwardStrength");

        [Header("Curve Reference")]
        [SerializeField] private Transform _reference;
        [SerializeField] private Transform _directionReference;
        [SerializeField, Min(0f)] private float _curvature = 0.001f;
        [SerializeField, Min(0f)] private float _lateralCurvature = 0.001f;
        [SerializeField, Min(0f)] private float _backwardCurvature;

        [Header("Culling Bounds")]
        [SerializeField] private Transform _boundsRoot;
        [SerializeField, Min(1f)] private float _maximumCurveDistance = 50f;
        [SerializeField, Min(1f)] private float _maximumLateralDistance = 20f;
        [SerializeField, Min(1f)] private float _maximumBackwardDistance = 20f;
        [SerializeField] private bool _expandMeshRendererBounds = true;

        public float Curvature => _curvature;
        public float LateralCurvature => _lateralCurvature;
        public float BackwardCurvature => _backwardCurvature;

        private void OnEnable()
        {
            UpdateShaderGlobals();
            RefreshRendererBounds();
        }

        private void LateUpdate()
        {
            UpdateShaderGlobals();
        }

        private void OnValidate()
        {
            _curvature = Mathf.Max(0f, _curvature);
            _lateralCurvature = Mathf.Max(0f, _lateralCurvature);
            _backwardCurvature = Mathf.Max(0f, _backwardCurvature);
            _maximumCurveDistance = Mathf.Max(1f, _maximumCurveDistance);
            _maximumLateralDistance = Mathf.Max(1f, _maximumLateralDistance);
            _maximumBackwardDistance = Mathf.Max(1f, _maximumBackwardDistance);

            if (!isActiveAndEnabled)
                return;

            UpdateShaderGlobals();
            RefreshRendererBounds();
        }

        private void OnDisable()
        {
            Shader.SetGlobalFloat(CurveStrengthId, 0f);
            Shader.SetGlobalFloat(CurveLateralStrengthId, 0f);
            Shader.SetGlobalFloat(CurveBackwardStrengthId, 0f);
        }

        private void UpdateShaderGlobals()
        {
            Transform reference = _reference != null ? _reference : transform;
            Transform directionReference = _directionReference != null ? _directionReference : reference;

            Vector3 forward = directionReference.forward;
            forward.y = 0f;

            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.forward;
            else
                forward.Normalize();

            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            Vector3 origin = reference.position;

            Shader.SetGlobalVector(CurveOriginId, new Vector4(origin.x, origin.y, origin.z, 0f));
            Shader.SetGlobalVector(CurveForwardId, new Vector4(forward.x, forward.y, forward.z, 0f));
            Shader.SetGlobalVector(CurveRightId, new Vector4(right.x, right.y, right.z, 0f));

            Shader.SetGlobalFloat(CurveStrengthId, _curvature);
            Shader.SetGlobalFloat(CurveLateralStrengthId, _lateralCurvature);
            Shader.SetGlobalFloat(CurveBackwardStrengthId, _backwardCurvature);
        }

        [ContextMenu("Refresh Renderer Bounds")]
        public void RefreshRendererBounds()
        {
            if (!_expandMeshRendererBounds)
                return;

            Transform root = _boundsRoot != null ? _boundsRoot : transform;

            float maximumDownwardOffset =
                _curvature * _maximumCurveDistance * _maximumCurveDistance
                + _lateralCurvature * _maximumLateralDistance * _maximumLateralDistance
                + _backwardCurvature * _maximumBackwardDistance * _maximumBackwardDistance;

            foreach (MeshRenderer meshRenderer in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                MeshFilter meshFilter = meshRenderer.GetComponent<MeshFilter>();

                if (meshFilter == null || meshFilter.sharedMesh == null)
                    continue;

                Bounds sourceBounds = meshFilter.sharedMesh.bounds;

                Vector3 localDownOffset = meshRenderer.transform.InverseTransformVector(
                    Vector3.down * maximumDownwardOffset);

                Bounds expandedBounds = sourceBounds;

                Bounds shiftedBounds = new Bounds(
                    sourceBounds.center + localDownOffset,
                    sourceBounds.size);

                expandedBounds.Encapsulate(shiftedBounds.min);
                expandedBounds.Encapsulate(shiftedBounds.max);

                meshRenderer.localBounds = expandedBounds;
            }
        }
    }
}