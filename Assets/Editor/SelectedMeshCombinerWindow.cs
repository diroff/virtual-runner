using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace EditorTools
{
    public sealed class SelectedMeshCombinerWindow : EditorWindow
    {
        private const string DefaultMeshesFolder = "Assets/GeneratedMeshes";

        private string _outputName = "CombinedMeshes";
        private string _meshesFolder = DefaultMeshesFolder;
        private bool _disableSourceRenderers = true;
        private bool _includeInactive;

        private readonly List<string> _warnings = new();

        [MenuItem("Tools/Mesh Combiner/Open")]
        private static void Open()
        {
            GetWindow<SelectedMeshCombinerWindow>("Mesh Combiner");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Selected Mesh Combiner", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            GameObject[] selection = Selection.gameObjects;

            EditorGUILayout.LabelField($"Selected objects: {selection.Length}");

            if (selection.Length == 0)
                EditorGUILayout.HelpBox("Select one or more objects in the Hierarchy.", MessageType.Info);

            EditorGUILayout.Space();

            _outputName = EditorGUILayout.TextField("Output Name", _outputName);
            _includeInactive = EditorGUILayout.Toggle("Include Inactive", _includeInactive);
            _disableSourceRenderers = EditorGUILayout.Toggle("Disable Source Renderers", _disableSourceRenderers);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Mesh Assets", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.TextField("Folder", _meshesFolder);

                if (GUILayout.Button("Choose", GUILayout.Width(70f)))
                    ChooseMeshesFolder();
            }

            EditorGUILayout.HelpBox(
                "Meshes are grouped by material and compatible Renderer settings. " +
                "Lightmapped objects and objects using MaterialPropertyBlock are skipped.",
                MessageType.Info);

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(selection.Length == 0 || string.IsNullOrWhiteSpace(_outputName)))
            {
                if (GUILayout.Button("Combine Selected Objects", GUILayout.Height(32f)))
                    CombineSelection(selection);
            }
        }

        private void ChooseMeshesFolder()
        {
            string absolutePath = EditorUtility.OpenFolderPanel(
                "Choose Mesh Asset Folder",
                Path.GetFullPath(_meshesFolder),
                string.Empty);

            if (string.IsNullOrEmpty(absolutePath))
                return;

            string assetsPath = Path.GetFullPath(Application.dataPath).Replace("\\", "/");
            absolutePath = absolutePath.Replace("\\", "/");

            if (!absolutePath.StartsWith(assetsPath + "/", StringComparison.OrdinalIgnoreCase))
            {
                EditorUtility.DisplayDialog(
                    "Invalid Folder",
                    "Choose a folder inside this project's Assets directory.",
                    "OK");

                return;
            }

            _meshesFolder = "Assets" + absolutePath.Substring(assetsPath.Length);
        }

        private void CombineSelection(GameObject[] selection)
        {
            _warnings.Clear();

            string folder = _meshesFolder.Replace("\\", "/");

            if (!AssetDatabase.IsValidFolder(folder))
                EnsureAssetFolder(folder);

            if (!AssetDatabase.IsValidFolder(folder))
            {
                EditorUtility.DisplayDialog(
                    "Invalid Folder",
                    $"Could not create or access folder:\n{folder}",
                    "OK");

                return;
            }

            List<Transform> roots = GetSelectionRoots(selection);

            if (roots.Count == 0)
                return;

            Transform outputParent = FindCommonAncestor(roots);
            Matrix4x4 outputWorldToLocal = outputParent != null
                ? outputParent.worldToLocalMatrix
                : Matrix4x4.identity;

            List<MeshFilter> sourceFilters = new();
            HashSet<MeshRenderer> sourceRenderers = new();

            foreach (Transform root in roots)
            {
                MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>(_includeInactive);

                foreach (MeshFilter filter in filters)
                {
                    if (filter == null || !filter.gameObject.scene.IsValid())
                        continue;

                    MeshRenderer renderer = filter.GetComponent<MeshRenderer>();

                    if (renderer == null || !renderer.enabled)
                        continue;

                    if (!_includeInactive && !renderer.gameObject.activeInHierarchy)
                        continue;

                    if (filter.sharedMesh == null)
                    {
                        _warnings.Add($"'{filter.name}': no mesh assigned.");
                        continue;
                    }

                    if (renderer.lightmapIndex >= 0)
                    {
                        _warnings.Add($"'{renderer.name}': skipped because it uses lightmapping.");
                        continue;
                    }

                    if (renderer.HasPropertyBlock())
                    {
                        _warnings.Add($"'{renderer.name}': skipped because it uses MaterialPropertyBlock.");
                        continue;
                    }

                    Material[] materials = renderer.sharedMaterials;
                    int subMeshCount = filter.sharedMesh.subMeshCount;

                    if (materials.Length < subMeshCount)
                    {
                        _warnings.Add($"'{renderer.name}': not enough materials for its submeshes.");
                        continue;
                    }

                    bool hasMissingMaterial = false;

                    for (int i = 0; i < subMeshCount; i++)
                    {
                        if (materials[i] != null)
                            continue;

                        hasMissingMaterial = true;
                        break;
                    }

                    if (hasMissingMaterial)
                    {
                        _warnings.Add($"'{renderer.name}': at least one submesh has no material.");
                        continue;
                    }

                    sourceFilters.Add(filter);
                    sourceRenderers.Add(renderer);
                }
            }

            if (sourceFilters.Count == 0)
            {
                ShowResult("No compatible meshes were found.");
                return;
            }

            Dictionary<RendererGroupKey, List<CombineInstance>> groups = new();

            foreach (MeshFilter filter in sourceFilters)
            {
                MeshRenderer renderer = filter.GetComponent<MeshRenderer>();
                Mesh mesh = filter.sharedMesh;
                Material[] materials = renderer.sharedMaterials;

                for (int i = 0; i < mesh.subMeshCount; i++)
                {
                    RendererGroupKey key = new(renderer, materials[i]);

                    if (!groups.TryGetValue(key, out List<CombineInstance> instances))
                    {
                        instances = new List<CombineInstance>();
                        groups.Add(key, instances);
                    }

                    instances.Add(new CombineInstance
                    {
                        mesh = mesh,
                        subMeshIndex = i,
                        transform = outputWorldToLocal * filter.transform.localToWorldMatrix
                    });
                }
            }

            List<Mesh> combinedMeshes = new();
            List<RendererGroupKey> combinedKeys = new();

            try
            {
                foreach (KeyValuePair<RendererGroupKey, List<CombineInstance>> group in groups)
                {
                    List<CombineInstance> instances = group.Value;

                    if (instances.Count == 0)
                        continue;

                    int vertexCount = instances.Sum(instance => instance.mesh.vertexCount);

                    Mesh combinedMesh = new()
                    {
                        name = $"{_outputName}_{group.Key.Material.name}",
                        indexFormat = vertexCount > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16
                    };

                    combinedMesh.CombineMeshes(instances.ToArray(), true, true, false);

                    combinedMeshes.Add(combinedMesh);
                    combinedKeys.Add(group.Key);
                }
            }
            catch (Exception exception)
            {
                foreach (Mesh mesh in combinedMeshes)
                    DestroyImmediate(mesh);

                Debug.LogException(exception);
                ShowResult("Mesh combination failed. Source renderers were not changed.");
                return;
            }

            GameObject outputRoot = null;
            List<string> createdAssetPaths = new();

            try
            {
                outputRoot = new GameObject(_outputName);

                if (outputParent != null)
                    outputRoot.transform.SetParent(outputParent, false);

                outputRoot.transform.localPosition = Vector3.zero;
                outputRoot.transform.localRotation = Quaternion.identity;
                outputRoot.transform.localScale = Vector3.one;

                for (int i = 0; i < combinedMeshes.Count; i++)
                {
                    Mesh mesh = combinedMeshes[i];
                    RendererGroupKey key = combinedKeys[i];

                    string meshPath = AssetDatabase.GenerateUniqueAssetPath(
                        $"{folder}/{SanitizeFileName(mesh.name)}.asset");

                    AssetDatabase.CreateAsset(mesh, meshPath);
                    createdAssetPaths.Add(meshPath);

                    GameObject combinedObject = new(key.Material.name);
                    combinedObject.transform.SetParent(outputRoot.transform, false);

                    MeshFilter combinedFilter = combinedObject.AddComponent<MeshFilter>();
                    combinedFilter.sharedMesh = mesh;

                    MeshRenderer combinedRenderer = combinedObject.AddComponent<MeshRenderer>();

                    combinedRenderer.sharedMaterial = key.Material;
                    combinedRenderer.shadowCastingMode = key.ShadowCastingMode;
                    combinedRenderer.receiveShadows = key.ReceiveShadows;
                    combinedRenderer.lightProbeUsage = key.LightProbeUsage;
                    combinedRenderer.reflectionProbeUsage = key.ReflectionProbeUsage;
                    combinedRenderer.renderingLayerMask = key.RenderingLayerMask;
                    combinedRenderer.sortingLayerID = key.SortingLayerID;
                    combinedRenderer.sortingOrder = key.SortingOrder;
                    combinedRenderer.motionVectorGenerationMode = key.MotionVectorGenerationMode;
                    combinedRenderer.allowOcclusionWhenDynamic = key.AllowOcclusionWhenDynamic;
                }

                AssetDatabase.SaveAssets();

                if (_disableSourceRenderers)
                {
                    foreach (MeshRenderer renderer in sourceRenderers)
                        renderer.enabled = false;
                }

                Undo.RegisterCreatedObjectUndo(outputRoot, "Combine Selected Meshes");

                Selection.activeGameObject = outputRoot;
                EditorGUIUtility.PingObject(outputRoot);

                ShowResult(
                    $"Created '{_outputName}'.\n" +
                    $"Combined mesh assets: {combinedMeshes.Count}\n" +
                    $"Source renderers: {sourceRenderers.Count}");
            }
            catch (Exception exception)
            {
                if (outputRoot != null)
                    DestroyImmediate(outputRoot);

                foreach (string assetPath in createdAssetPaths)
                    AssetDatabase.DeleteAsset(assetPath);

                foreach (Mesh mesh in combinedMeshes)
                {
                    if (!AssetDatabase.Contains(mesh))
                        DestroyImmediate(mesh);
                }

                Debug.LogException(exception);
                ShowResult("An error occurred. Check the Console for details.");
            }
        }

        private List<Transform> GetSelectionRoots(GameObject[] selection)
        {
            HashSet<Transform> selected = new();

            foreach (GameObject gameObject in selection)
            {
                if (gameObject != null)
                    selected.Add(gameObject.transform);
            }

            List<Transform> roots = new();

            foreach (Transform transform in selected)
            {
                bool hasSelectedAncestor = false;
                Transform parent = transform.parent;

                while (parent != null)
                {
                    if (selected.Contains(parent))
                    {
                        hasSelectedAncestor = true;
                        break;
                    }

                    parent = parent.parent;
                }

                if (!hasSelectedAncestor)
                    roots.Add(transform);
            }

            return roots;
        }

        private static Transform FindCommonAncestor(List<Transform> transforms)
        {
            if (transforms.Count == 0)
                return null;

            Transform candidate = transforms[0];

            while (candidate != null)
            {
                bool isCommonAncestor = true;

                foreach (Transform transform in transforms)
                {
                    if (transform == candidate || transform.IsChildOf(candidate))
                        continue;

                    isCommonAncestor = false;
                    break;
                }

                if (isCommonAncestor)
                    return candidate;

                candidate = candidate.parent;
            }

            return null;
        }

        private void ShowResult(string message)
        {
            if (_warnings.Count > 0)
                message += "\n\nWarnings:\n" + string.Join("\n", _warnings.Take(15));

            Debug.Log(message);

            EditorUtility.DisplayDialog("Mesh Combiner", message, "OK");
        }

        private static string SanitizeFileName(string value)
        {
            foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
                value = value.Replace(invalidCharacter, '_');

            return value;
        }

        private static void EnsureAssetFolder(string folder)
        {
            folder = folder.TrimEnd('/');

            if (AssetDatabase.IsValidFolder(folder))
                return;

            string parent = Path.GetDirectoryName(folder)?.Replace("\\", "/");
            string name = Path.GetFileName(folder);

            if (string.IsNullOrEmpty(parent))
                return;

            if (!AssetDatabase.IsValidFolder(parent))
                EnsureAssetFolder(parent);

            if (AssetDatabase.IsValidFolder(parent))
                AssetDatabase.CreateFolder(parent, name);
        }

        private readonly struct RendererGroupKey : IEquatable<RendererGroupKey>
        {
            public readonly Material Material;
            public readonly ShadowCastingMode ShadowCastingMode;
            public readonly bool ReceiveShadows;
            public readonly LightProbeUsage LightProbeUsage;
            public readonly ReflectionProbeUsage ReflectionProbeUsage;
            public readonly uint RenderingLayerMask;
            public readonly int SortingLayerID;
            public readonly int SortingOrder;
            public readonly MotionVectorGenerationMode MotionVectorGenerationMode;
            public readonly bool AllowOcclusionWhenDynamic;

            public RendererGroupKey(MeshRenderer renderer, Material material)
            {
                Material = material;
                ShadowCastingMode = renderer.shadowCastingMode;
                ReceiveShadows = renderer.receiveShadows;
                LightProbeUsage = renderer.lightProbeUsage;
                ReflectionProbeUsage = renderer.reflectionProbeUsage;
                RenderingLayerMask = renderer.renderingLayerMask;
                SortingLayerID = renderer.sortingLayerID;
                SortingOrder = renderer.sortingOrder;
                MotionVectorGenerationMode = renderer.motionVectorGenerationMode;
                AllowOcclusionWhenDynamic = renderer.allowOcclusionWhenDynamic;
            }

            public bool Equals(RendererGroupKey other)
            {
                return Material == other.Material
                    && ShadowCastingMode == other.ShadowCastingMode
                    && ReceiveShadows == other.ReceiveShadows
                    && LightProbeUsage == other.LightProbeUsage
                    && ReflectionProbeUsage == other.ReflectionProbeUsage
                    && RenderingLayerMask == other.RenderingLayerMask
                    && SortingLayerID == other.SortingLayerID
                    && SortingOrder == other.SortingOrder
                    && MotionVectorGenerationMode == other.MotionVectorGenerationMode
                    && AllowOcclusionWhenDynamic == other.AllowOcclusionWhenDynamic;
            }

            public override bool Equals(object obj)
            {
                return obj is RendererGroupKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(
                    Material,
                    ShadowCastingMode,
                    ReceiveShadows,
                    LightProbeUsage,
                    ReflectionProbeUsage,
                    RenderingLayerMask,
                    SortingLayerID,
                    SortingOrder);
            }
        }
    }
}