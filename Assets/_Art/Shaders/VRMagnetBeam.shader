Shader "VirtualRunner/VR Magnet Beam"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (0.15, 0.8, 1, 0.3)
        [HDR] _EmissionColor("Energy Color", Color) = (0.05, 0.65, 1.5, 1)
        _EmissionIntensity("Energy Intensity", Range(0, 4)) = 1.2
        _Opacity("Opacity", Range(0, 1)) = 0.4
        [Enum(LocalPosition, 0, MeshUV, 1)] _MappingMode("Texture Coordinates", Float) = 0

        _FlowSpeed("Flow Speed", Range(0, 20)) = 8
        _FlowFrequency("Flow Frequency", Range(0.1, 20)) = 4
        _FlowSharpness("Flow Sharpness", Range(1, 12)) = 5
        _FlowStrength("Flow Strength", Range(0, 2)) = 0.65
        [IntRange] _StreamCount("Stream Count", Range(1, 16)) = 6
        _StreamTwist("Stream Twist", Range(-10, 10)) = 1
        _SwirlSpeed("Swirl Speed", Range(-10, 10)) = 0.8
        _StreamSharpness("Stream Sharpness", Range(1, 16)) = 7
        _StreamStrength("Stream Strength", Range(0, 2)) = 0.8
        _RimPower("Rim Power", Range(0.5, 8)) = 2.5
        _RimStrength("Rim Strength", Range(0, 3)) = 0.8
        _SparkleDensity("Detail Density", Range(0.5, 15)) = 5
        _SparkleSpeed("Detail Speed", Range(0, 5)) = 0.7
        _SparkleStrength("Detail Strength", Range(0, 2)) = 0.5
        _NearFade("Near Fade Distance", Range(0.01, 3)) = 0.4

        [Enum(Alpha, 0, Additive, 1, Premultiply, 2)] _BlendMode("Blending Mode", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Render Face", Float) = 0
        [Toggle] _ZWrite("Depth Write", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("Depth Test", Float) = 4
        _QueueOffset("Queue Offset", Range(-50, 50)) = 0

        [HideInInspector] _Premultiply("__premultiply", Float) = 0
        [HideInInspector] _SrcBlend("__src", Float) = 5
        [HideInInspector] _DstBlend("__dst", Float) = 10
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "Beam"
            Tags { "LightMode" = "UniversalForward" }
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            ZTest [_ZTest]
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex VRMagnetBeamVertex
            #pragma fragment VRMagnetBeamFragment
            #pragma multi_compile_instancing
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Includes/VRWorldCurvature.hlsl"
            #include "Includes/VRMagnetBeam.hlsl"
            ENDHLSL
        }
    }

    FallBack Off
    CustomEditor "EditorTools.MagnetBeamShaderGUI"
}
