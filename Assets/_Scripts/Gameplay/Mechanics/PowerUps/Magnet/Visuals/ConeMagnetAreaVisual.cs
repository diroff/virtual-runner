using UnityEngine;

namespace Gameplay.Mechanics.PowerUps.Magnet.Visuals
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class ConeMagnetAreaVisual : MagnetAreaVisualBase
    {
        [SerializeField, Range(8, 64)] private int _radialSegments = 32;
        [SerializeField, Range(2, 24)] private int _polarSegments = 8;

        private ConeMagnetTargeting _targeting;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private Mesh _mesh;
        private float _renderedRange = -1f;
        private float _renderedAngle = -1f;
        private int _renderedRadialSegments;
        private int _renderedPolarSegments;

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshRenderer.enabled = false;
        }

        protected override bool OnStart(MagnetTargetingBase targeting)
        {
            if (targeting is not ConeMagnetTargeting coneTargeting)
                return false;

            _targeting = coneTargeting;
            _meshFilter ??= GetComponent<MeshFilter>();
            _meshRenderer ??= GetComponent<MeshRenderer>();

            if (_mesh == null)
            {
                _mesh = new Mesh { name = "Magnet Cone Area" };
                _mesh.MarkDynamic();
                _meshFilter.sharedMesh = _mesh;
            }

            RebuildIfNeeded();
            _meshRenderer.enabled = true;
            return true;
        }

        protected override void OnUpdate(Transform origin)
        {
            RebuildIfNeeded();
            transform.SetPositionAndRotation(origin.position, origin.rotation);
        }

        protected override void OnStop()
        {
            if (_meshRenderer != null)
                _meshRenderer.enabled = false;

            _targeting = null;
        }

        private void RebuildIfNeeded()
        {
            float range = _targeting.Range;
            float fullAngle = _targeting.FullAngle;

            if (Mathf.Approximately(_renderedRange, range) &&
                Mathf.Approximately(_renderedAngle, fullAngle) &&
                _renderedRadialSegments == _radialSegments &&
                _renderedPolarSegments == _polarSegments)
                return;

            int radialSegments = _radialSegments;
            int polarSegments = _polarSegments;
            Vector3[] vertices = new Vector3[2 + radialSegments * polarSegments];
            int[] triangles = new int[radialSegments * polarSegments * 6];

            vertices[0] = Vector3.zero;
            vertices[1] = Vector3.forward * range;

            float halfAngle = fullAngle * 0.5f * Mathf.Deg2Rad;
            for (int ring = 0; ring < polarSegments; ring++)
            {
                float angle = halfAngle * (ring + 1) / polarSegments;
                float ringRadius = Mathf.Sin(angle) * range;
                float depth = Mathf.Cos(angle) * range;

                for (int segment = 0; segment < radialSegments; segment++)
                {
                    float rotation = segment * Mathf.PI * 2f / radialSegments;
                    vertices[2 + ring * radialSegments + segment] = new Vector3(
                        Mathf.Cos(rotation) * ringRadius,
                        Mathf.Sin(rotation) * ringRadius,
                        depth);
                }
            }

            int triangleIndex = 0;
            for (int segment = 0; segment < radialSegments; segment++)
            {
                int next = (segment + 1) % radialSegments;
                triangles[triangleIndex++] = 1;
                triangles[triangleIndex++] = 2 + segment;
                triangles[triangleIndex++] = 2 + next;
            }

            for (int ring = 0; ring < polarSegments - 1; ring++)
            {
                int innerStart = 2 + ring * radialSegments;
                int outerStart = innerStart + radialSegments;

                for (int segment = 0; segment < radialSegments; segment++)
                {
                    int next = (segment + 1) % radialSegments;
                    triangles[triangleIndex++] = innerStart + segment;
                    triangles[triangleIndex++] = outerStart + segment;
                    triangles[triangleIndex++] = innerStart + next;

                    triangles[triangleIndex++] = innerStart + next;
                    triangles[triangleIndex++] = outerStart + segment;
                    triangles[triangleIndex++] = outerStart + next;
                }
            }

            int rimStart = 2 + (polarSegments - 1) * radialSegments;
            for (int segment = 0; segment < radialSegments; segment++)
            {
                int next = (segment + 1) % radialSegments;
                triangles[triangleIndex++] = 0;
                triangles[triangleIndex++] = rimStart + next;
                triangles[triangleIndex++] = rimStart + segment;
            }

            _mesh.Clear();
            _mesh.vertices = vertices;
            _mesh.triangles = triangles;
            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();

            _renderedRange = range;
            _renderedAngle = fullAngle;
            _renderedRadialSegments = radialSegments;
            _renderedPolarSegments = polarSegments;
        }

        private void OnDestroy()
        {
            if (_mesh == null)
                return;

            if (_meshFilter != null && _meshFilter.sharedMesh == _mesh)
                _meshFilter.sharedMesh = null;

            Destroy(_mesh);
        }
    }
}
