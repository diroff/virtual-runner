#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace EditorTools
{
    public sealed class MagnetBeamShaderGUI : ShaderGUI
    {
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            MaterialProperty baseMap = FindProperty("_BaseMap", properties, false);
            MaterialProperty baseColor = FindProperty("_BaseColor", properties, false);

            if (baseMap != null)
            {
                materialEditor.TexturePropertySingleLine(new GUIContent("Base Map"), baseMap, baseColor);
                materialEditor.TextureScaleOffsetProperty(baseMap);
            }

            Draw(materialEditor, properties, "_EmissionColor");
            Draw(materialEditor, properties, "_EmissionIntensity");
            Draw(materialEditor, properties, "_Opacity");
            Draw(materialEditor, properties, "_MappingMode");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Energy Flow", EditorStyles.boldLabel);
            Draw(materialEditor, properties, "_FlowSpeed");
            Draw(materialEditor, properties, "_FlowFrequency");
            Draw(materialEditor, properties, "_FlowSharpness");
            Draw(materialEditor, properties, "_FlowStrength");
            Draw(materialEditor, properties, "_StreamCount");
            Draw(materialEditor, properties, "_StreamTwist");
            Draw(materialEditor, properties, "_SwirlSpeed");
            Draw(materialEditor, properties, "_StreamSharpness");
            Draw(materialEditor, properties, "_StreamStrength");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Surface Details", EditorStyles.boldLabel);
            Draw(materialEditor, properties, "_RimPower");
            Draw(materialEditor, properties, "_RimStrength");
            Draw(materialEditor, properties, "_SparkleDensity");
            Draw(materialEditor, properties, "_SparkleSpeed");
            Draw(materialEditor, properties, "_SparkleStrength");
            Draw(materialEditor, properties, "_NearFade");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rendering", EditorStyles.boldLabel);
            Draw(materialEditor, properties, "_BlendMode");
            Draw(materialEditor, properties, "_Cull");
            Draw(materialEditor, properties, "_ZWrite");
            Draw(materialEditor, properties, "_ZTest");
            Draw(materialEditor, properties, "_QueueOffset");

            foreach (Object target in materialEditor.targets)
            {
                if (target is Material material)
                    SetupMaterial(material);
            }
        }

        private static void Draw(MaterialEditor editor, MaterialProperty[] properties, string propertyName)
        {
            MaterialProperty property = FindProperty(propertyName, properties, false);
            if (property != null)
                editor.ShaderProperty(property, property.displayName);
        }

        private static void SetupMaterial(Material material)
        {
            int blend = Mathf.RoundToInt(material.GetFloat("_BlendMode"));
            BlendMode src = BlendMode.SrcAlpha;
            BlendMode dst = BlendMode.OneMinusSrcAlpha;

            switch (blend)
            {
                case 1:
                    dst = BlendMode.One;
                    break;
                case 2:
                    src = BlendMode.One;
                    break;
            }

            SetFloat(material, "_SrcBlend", (float)src);
            SetFloat(material, "_DstBlend", (float)dst);
            SetFloat(material, "_Premultiply", blend == 2 ? 1f : 0f);

            material.SetOverrideTag("RenderType", "Transparent");
            int queue = (int)RenderQueue.Transparent + Mathf.RoundToInt(material.GetFloat("_QueueOffset"));
            if (material.renderQueue != queue)
                material.renderQueue = queue;
        }

        private static void SetFloat(Material material, string name, float value)
        {
            if (!Mathf.Approximately(material.GetFloat(name), value))
                material.SetFloat(name, value);
        }
    }
}
#endif
