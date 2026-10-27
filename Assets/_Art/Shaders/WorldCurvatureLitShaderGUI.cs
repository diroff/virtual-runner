#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace EditorTools
{
    public sealed class WorldCurvatureLitShaderGUI : ShaderGUI
    {
        private static readonly GUIContent SurfaceOptionsLabel = new("Surface Options");
        private static readonly GUIContent SurfaceInputsLabel = new("Surface Inputs");
        private static readonly GUIContent AdvancedLabel = new("Advanced");

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            Material[] materials = new Material[materialEditor.targets.Length];

            for (int i = 0; i < materialEditor.targets.Length; i++)
                materials[i] = materialEditor.targets[i] as Material;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField(SurfaceOptionsLabel, EditorStyles.boldLabel);
            DrawProperty(materialEditor, properties, "_Surface", "Surface Type");

            bool transparent = GetFloat(materials, "_Surface", 0f) >= 0.5f;
            int blend = Mathf.RoundToInt(GetFloat(materials, "_Blend", 0f));

            if (transparent)
            {
                DrawProperty(materialEditor, properties, "_Blend", "Blending Mode");

                blend = Mathf.RoundToInt(GetFloat(materials, "_Blend", 0f));
                if (blend == 0 || blend == 2)
                    DrawProperty(materialEditor, properties, "_PreserveSpecularLighting", "Preserve Specular Lighting");
            }

            DrawProperty(materialEditor, properties, "_Cull", "Render Face");
            DrawProperty(materialEditor, properties, "_AlphaClip", "Alpha Clipping");

            if (GetFloat(materials, "_AlphaClip", 0f) >= 0.5f)
                DrawProperty(materialEditor, properties, "_Cutoff", "Threshold");

            DrawProperty(materialEditor, properties, "_CastShadows", "Cast Shadows");
            DrawProperty(materialEditor, properties, "_ReceiveShadows", "Receive Shadows");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(SurfaceInputsLabel, EditorStyles.boldLabel);
            DrawBaseMap(materialEditor, properties);
            DrawNormalMap(materialEditor, properties, materials);
            DrawMetallic(materialEditor, properties, materials);
            DrawOcclusion(materialEditor, properties, materials);
            DrawEmission(materialEditor, properties, materials);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(AdvancedLabel, EditorStyles.boldLabel);
            DrawProperty(materialEditor, properties, "_QueueOffset", "Queue Offset");

            EditorGUI.EndChangeCheck();

            foreach (Material material in materials)
            {
                if (material != null)
                    SetupMaterial(material);
            }
        }

        private static void DrawBaseMap(MaterialEditor editor, MaterialProperty[] properties)
        {
            MaterialProperty texture = Find("_BaseMap", properties);
            MaterialProperty color = Find("_BaseColor", properties);

            if (texture != null && color != null)
            {
                editor.TexturePropertySingleLine(new GUIContent("Base Map"), texture, color);
                editor.TextureScaleOffsetProperty(texture);
            }
            else
            {
                DrawProperty(editor, properties, "_BaseMap", "Base Map");
                DrawProperty(editor, properties, "_BaseColor", "Base Color");
            }
        }

        private static void DrawNormalMap(MaterialEditor editor, MaterialProperty[] properties, Material[] materials)
        {
            DrawProperty(editor, properties, "_NormalMapEnabled", "Normal Mapping");

            if (GetFloat(materials, "_NormalMapEnabled", 0f) < 0.5f)
                return;

            DrawProperty(editor, properties, "_BumpMap", "Normal Map");
            DrawProperty(editor, properties, "_BumpScale", "Normal Scale");
        }

        private static void DrawMetallic(MaterialEditor editor, MaterialProperty[] properties, Material[] materials)
        {
            DrawProperty(editor, properties, "_UseMetallicMap", "Use Metallic Map");

            if (GetFloat(materials, "_UseMetallicMap", 0f) >= 0.5f)
                DrawProperty(editor, properties, "_MetallicGlossMap", "Metallic Map (R: Metallic, A: Smoothness)");
            else
                DrawProperty(editor, properties, "_Metallic", "Metallic");

            DrawProperty(editor, properties, "_Smoothness", "Smoothness");
        }

        private static void DrawOcclusion(MaterialEditor editor, MaterialProperty[] properties, Material[] materials)
        {
            DrawProperty(editor, properties, "_UseOcclusionMap", "Use Occlusion Map");

            if (GetFloat(materials, "_UseOcclusionMap", 0f) < 0.5f)
                return;

            DrawProperty(editor, properties, "_OcclusionMap", "Occlusion Map (G)");
            DrawProperty(editor, properties, "_OcclusionStrength", "Occlusion Strength");
        }

        private static void DrawEmission(MaterialEditor editor, MaterialProperty[] properties, Material[] materials)
        {
            DrawProperty(editor, properties, "_EmissionEnabled", "Emission");

            if (GetFloat(materials, "_EmissionEnabled", 0f) < 0.5f)
                return;

            DrawProperty(editor, properties, "_EmissionColor", "Emission Color");
            DrawProperty(editor, properties, "_EmissionMap", "Emission Map");
        }

        private static void DrawProperty(MaterialEditor editor, MaterialProperty[] properties, string propertyName, string label)
        {
            MaterialProperty property = Find(propertyName, properties);

            if (property != null)
                editor.ShaderProperty(property, label);
        }

        private static MaterialProperty Find(string propertyName, MaterialProperty[] properties)
        {
            return FindProperty(propertyName, properties, false);
        }

        private static float GetFloat(Material[] materials, string propertyName, float fallback)
        {
            if (materials == null || materials.Length == 0 || materials[0] == null || !materials[0].HasProperty(propertyName))
                return fallback;

            return materials[0].GetFloat(propertyName);
        }

        private static void SetupMaterial(Material material)
        {
            bool transparent = material.GetFloat("_Surface") >= 0.5f;
            bool alphaClip = material.GetFloat("_AlphaClip") >= 0.5f;
            int blend = Mathf.RoundToInt(material.GetFloat("_Blend"));
            bool preserveSpecular = transparent
                && material.GetFloat("_PreserveSpecularLighting") >= 0.5f
                && (blend == 0 || blend == 2);

            BlendMode sourceBlend = BlendMode.One;
            BlendMode destinationBlend = BlendMode.Zero;
            BlendMode sourceBlendAlpha = BlendMode.One;
            BlendMode destinationBlendAlpha = BlendMode.Zero;
            int zWrite = transparent ? 0 : 1;

            if (transparent)
            {
                switch (blend)
                {
                    case 0: // Alpha
                        sourceBlend = preserveSpecular ? BlendMode.One : BlendMode.SrcAlpha;
                        destinationBlend = BlendMode.OneMinusSrcAlpha;
                        sourceBlendAlpha = BlendMode.One;
                        destinationBlendAlpha = BlendMode.OneMinusSrcAlpha;
                        break;

                    case 1: // Premultiply
                        sourceBlend = BlendMode.One;
                        destinationBlend = BlendMode.OneMinusSrcAlpha;
                        sourceBlendAlpha = BlendMode.One;
                        destinationBlendAlpha = BlendMode.OneMinusSrcAlpha;
                        break;

                    case 2: // Additive
                        sourceBlend = preserveSpecular ? BlendMode.One : BlendMode.SrcAlpha;
                        destinationBlend = BlendMode.One;
                        sourceBlendAlpha = BlendMode.One;
                        destinationBlendAlpha = BlendMode.One;
                        break;

                    case 3: // Multiply
                        sourceBlend = BlendMode.DstColor;
                        destinationBlend = BlendMode.Zero;
                        sourceBlendAlpha = BlendMode.Zero;
                        destinationBlendAlpha = BlendMode.One;
                        break;
                }
            }

            SetFloatIfDifferent(material, "_SrcBlend", (float)sourceBlend);
            SetFloatIfDifferent(material, "_DstBlend", (float)destinationBlend);
            SetFloatIfDifferent(material, "_SrcBlendAlpha", (float)sourceBlendAlpha);
            SetFloatIfDifferent(material, "_DstBlendAlpha", (float)destinationBlendAlpha);
            SetFloatIfDifferent(material, "_ZWrite", zWrite);
            SetFloatIfDifferent(material, "_BlendModePreserveSpecular", preserveSpecular ? 1f : 0f);

            SetKeyword(material, "_SURFACE_TYPE_TRANSPARENT", transparent);
            SetKeyword(material, "_ALPHATEST_ON", alphaClip);
            SetKeyword(material, "_ALPHAPREMULTIPLY_ON", transparent && blend == 1);
            SetKeyword(material, "_ALPHAMODULATE_ON", transparent && blend == 3);
            SetKeyword(material, "_PRESERVE_SPECULAR_LIGHTING", preserveSpecular);
            SetKeyword(material, "_CAST_SHADOWS_OFF", material.GetFloat("_CastShadows") < 0.5f);
            SetKeyword(material, "_RECEIVE_SHADOWS_OFF", material.GetFloat("_ReceiveShadows") < 0.5f);
            SetKeyword(material, "_NORMALMAP", material.GetFloat("_NormalMapEnabled") >= 0.5f);
            SetKeyword(material, "_METALLICSPECGLOSSMAP", material.GetFloat("_UseMetallicMap") >= 0.5f);
            SetKeyword(material, "_OCCLUSIONMAP", material.GetFloat("_UseOcclusionMap") >= 0.5f);
            SetKeyword(material, "_EMISSION", material.GetFloat("_EmissionEnabled") >= 0.5f);

            material.SetOverrideTag("RenderType", transparent ? "Transparent" : alphaClip ? "TransparentCutout" : string.Empty);

            int baseQueue = transparent
                ? (int)RenderQueue.Transparent
                : alphaClip ? (int)RenderQueue.AlphaTest : (int)RenderQueue.Geometry;
            int queueOffset = material.HasProperty("_QueueOffset") ? Mathf.RoundToInt(material.GetFloat("_QueueOffset")) : 0;
            int renderQueue = baseQueue + queueOffset;

            if (material.renderQueue != renderQueue)
                material.renderQueue = renderQueue;
        }

        private static void SetFloatIfDifferent(Material material, string propertyName, float value)
        {
            if (material.HasProperty(propertyName) && !Mathf.Approximately(material.GetFloat(propertyName), value))
                material.SetFloat(propertyName, value);
        }

        private static void SetKeyword(Material material, string keyword, bool enabled)
        {
            if (enabled)
            {
                if (!material.IsKeywordEnabled(keyword))
                    material.EnableKeyword(keyword);
            }
            else if (material.IsKeywordEnabled(keyword))
            {
                material.DisableKeyword(keyword);
            }
        }
    }
}
#endif