Shader "VirtualRunner/VR World Curvature Lit"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)

        [Enum(Opaque, 0, Transparent, 1)] _Surface("Surface Type", Float) = 0
        [Enum(Alpha, 0, Premultiply, 1, Additive, 2, Multiply, 3)] _Blend("Blending Mode", Float) = 0
        [ToggleUI] _PreserveSpecularLighting("Preserve Specular Lighting", Float) = 0
        [Enum(Front, 2, Back, 1, Both, 0)] _Cull("Render Face", Float) = 2
        [ToggleUI] _CastShadows("Cast Shadows", Float) = 1
        [ToggleUI] _ReceiveShadows("Receive Shadows", Float) = 1

        [Toggle(_ALPHATEST_ON)] _AlphaClip("Alpha Clipping", Float) = 0
        _Cutoff("Alpha Cutoff", Range(0, 1)) = 0.5

        [Toggle(_NORMALMAP)] _NormalMapEnabled("Normal Mapping", Float) = 0
        [Normal] _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Scale", Range(0, 2)) = 1

        _Metallic("Metallic", Range(0, 1)) = 0
        [Toggle(_METALLICSPECGLOSSMAP)] _UseMetallicMap("Use Metallic Map", Float) = 0
        _MetallicGlossMap("Metallic Map (R: Metallic, A: Smoothness)", 2D) = "white" {}
        _Smoothness("Smoothness", Range(0, 1)) = 0.5

        [Toggle(_OCCLUSIONMAP)] _UseOcclusionMap("Use Occlusion Map", Float) = 0
        _OcclusionMap("Occlusion Map (G)", 2D) = "white" {}
        _OcclusionStrength("Occlusion Strength", Range(0, 1)) = 1

        [Toggle(_EMISSION)] _EmissionEnabled("Emission", Float) = 0
        [HDR] _EmissionColor("Emission Color", Color) = (0, 0, 0, 0)
        _EmissionMap("Emission Map", 2D) = "white" {}

        [HideInInspector] _SrcBlend("__src", Float) = 1
        [HideInInspector] _DstBlend("__dst", Float) = 0
        [HideInInspector] _SrcBlendAlpha("__srcA", Float) = 1
        [HideInInspector] _DstBlendAlpha("__dstA", Float) = 0
        [HideInInspector] _ZWrite("__zw", Float) = 1
        [HideInInspector] _BlendModePreserveSpecular("_BlendModePreserveSpecular", Float) = 0
        [HideInInspector] _QueueOffset("Queue offset", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "UniversalMaterialType" = "Lit"
        }

        Cull [_Cull]

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half4 _EmissionColor;
            half _Cutoff;
            half _Metallic;
            half _Smoothness;
            half _BumpScale;
            half _OcclusionStrength;
            half _Surface;
            half _Blend;
            half _PreserveSpecularLighting;
        CBUFFER_END

        TEXTURE2D(_MetallicGlossMap);
        SAMPLER(sampler_MetallicGlossMap);
        TEXTURE2D(_OcclusionMap);
        SAMPLER(sampler_OcclusionMap);

        // Set globally by WorldCurvatureController. These are deliberately not material properties:
        // every compatible environment material must use the same world-space curve.
       float4 _VRWorldCurveOriginWS;
float4 _VRWorldCurveForwardWS;
float4 _VRWorldCurveRightWS;

float _VRWorldCurveStrength;
float _VRWorldCurveLateralStrength;
float _VRWorldCurveBackwardStrength;

float3 VRWorldCurveForward()
{
    float3 forwardWS = _VRWorldCurveForwardWS.xyz;
    float lengthSquared = dot(forwardWS, forwardWS);

    return lengthSquared > 0.000001
        ? forwardWS * rsqrt(lengthSquared)
        : float3(0.0, 0.0, 1.0);
}

float3 VRWorldCurveRight()
{
    float3 rightWS = _VRWorldCurveRightWS.xyz;
    float lengthSquared = dot(rightWS, rightWS);

    return lengthSquared > 0.000001
        ? rightWS * rsqrt(lengthSquared)
        : float3(1.0, 0.0, 0.0);
}

float VRWorldCurveDepth(float3 positionWS)
{
    float3 forwardWS = VRWorldCurveForward();

    return max(
        0.0,
        dot(positionWS - _VRWorldCurveOriginWS.xyz, forwardWS));
}

float VRWorldCurveBackwardDepth(float3 positionWS)
{
    float3 forwardWS = VRWorldCurveForward();

    return max(
        0.0,
        -dot(positionWS - _VRWorldCurveOriginWS.xyz, forwardWS));
}

float VRWorldCurveLateral(float3 positionWS)
{
    float3 rightWS = VRWorldCurveRight();

    return dot(positionWS - _VRWorldCurveOriginWS.xyz, rightWS);
}

float3 VRApplyWorldCurve(float3 positionWS)
{
    float forwardDepth = VRWorldCurveDepth(positionWS);
    float backwardDepth = VRWorldCurveBackwardDepth(positionWS);
    float lateral = VRWorldCurveLateral(positionWS);

    float forwardOffset =
        _VRWorldCurveStrength * forwardDepth * forwardDepth;

    float backwardOffset =
        _VRWorldCurveBackwardStrength * backwardDepth * backwardDepth;

    float lateralOffset =
        _VRWorldCurveLateralStrength * lateral * lateral;

    positionWS.y -= forwardOffset + backwardOffset + lateralOffset;

    return positionWS;
}

float3 VRApplyWorldCurveNormal(float3 originalPositionWS, float3 normalWS)
{
    float forwardDepth = VRWorldCurveDepth(originalPositionWS);
    float backwardDepth = VRWorldCurveBackwardDepth(originalPositionWS);
    float lateral = VRWorldCurveLateral(originalPositionWS);

    float3 forwardWS = VRWorldCurveForward();
    float3 rightWS = VRWorldCurveRight();

    normalWS += forwardWS *
        (2.0 * _VRWorldCurveStrength * forwardDepth * normalWS.y);

    normalWS -= forwardWS *
        (2.0 * _VRWorldCurveBackwardStrength * backwardDepth * normalWS.y);

    normalWS += rightWS *
        (2.0 * _VRWorldCurveLateralStrength * lateral * normalWS.y);

    return normalize(normalWS);
}

float3 VRApplyWorldCurveTangent(float3 originalPositionWS, float3 tangentWS)
{
    float forwardDepth = VRWorldCurveDepth(originalPositionWS);
    float backwardDepth = VRWorldCurveBackwardDepth(originalPositionWS);
    float lateral = VRWorldCurveLateral(originalPositionWS);

    float3 forwardWS = VRWorldCurveForward();
    float3 rightWS = VRWorldCurveRight();

    float forwardCorrection =
        2.0 * _VRWorldCurveStrength * forwardDepth
        * dot(forwardWS, tangentWS);

    float backwardCorrection =
        2.0 * _VRWorldCurveBackwardStrength * backwardDepth
        * dot(forwardWS, tangentWS);

    float lateralCorrection =
        2.0 * _VRWorldCurveLateralStrength * lateral
        * dot(rightWS, tangentWS);

    tangentWS -= float3(
        0.0,
        1.0,
        0.0) * (forwardCorrection - backwardCorrection + lateralCorrection);

    return normalize(tangentWS);
}

        struct CurvedAttributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float2 texcoord : TEXCOORD0;
            float2 staticLightmapUV : TEXCOORD1;
            float2 dynamicLightmapUV : TEXCOORD2;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct CurvedVaryings
        {
            float2 uv : TEXCOORD0;
            float3 positionWS : TEXCOORD1;
            half3 normalWS : TEXCOORD2;
            half4 tangentWS : TEXCOORD3;
            half4 fogFactorAndVertexLight : TEXCOORD5;
            DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 8);
            #ifdef DYNAMICLIGHTMAP_ON
                float2 dynamicLightmapUV : TEXCOORD9;
            #endif
            float4 positionCS : SV_POSITION;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };

        CurvedVaryings VRWorldCurveVertex(CurvedAttributes input)
        {
            CurvedVaryings output = (CurvedVaryings)0;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_TRANSFER_INSTANCE_ID(input, output);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
            VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);

            float3 originalPositionWS = positionInputs.positionWS;
            float3 positionWS = VRApplyWorldCurve(originalPositionWS);
            float3 normalWS = VRApplyWorldCurveNormal(originalPositionWS, normalInputs.normalWS);

            output.positionWS = positionWS;
            output.positionCS = TransformWorldToHClip(positionWS);
            output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
            output.normalWS = normalWS;
            #if defined(_NORMALMAP)
                float3 tangentWS = VRApplyWorldCurveTangent(originalPositionWS, normalInputs.tangentWS);
                output.tangentWS = half4(tangentWS, input.tangentOS.w * GetOddNegativeScale());
            #else
                output.tangentWS = half4(1.0h, 0.0h, 0.0h, input.tangentOS.w * GetOddNegativeScale());
            #endif

            half3 vertexLight = 0.0h;
            #if defined(_ADDITIONAL_LIGHTS_VERTEX)
                vertexLight = VertexLighting(positionWS, normalWS);
            #endif
            half fogFactor = ComputeFogFactor(output.positionCS.z);
            output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

            OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
            #ifdef DYNAMICLIGHTMAP_ON
                output.dynamicLightmapUV = input.dynamicLightmapUV * unity_DynamiclightmapST.xy + unity_DynamiclightmapST.zw;
            #endif
            OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

            return output;
        }

        void VRInitializeInputData(CurvedVaryings input, half3 normalTS, out InputData inputData)
        {
            inputData = (InputData)0;
            inputData.positionWS = input.positionWS;
            inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);

            #if defined(_NORMALMAP)
                half3 tangentWS = normalize(input.tangentWS.xyz);
                half3 bitangentWS = input.tangentWS.w * cross(input.normalWS, tangentWS);
                half3x3 tangentToWorld = half3x3(tangentWS, bitangentWS, input.normalWS);
                inputData.normalWS = TransformTangentToWorld(normalTS, tangentToWorld);
            #else
                inputData.normalWS = input.normalWS;
            #endif

            inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
            inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
            inputData.fogCoord = input.fogFactorAndVertexLight.x;
            inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
            inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

            #if defined(DYNAMICLIGHTMAP_ON)
                inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.dynamicLightmapUV, input.vertexSH, inputData.normalWS);
            #else
                inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
            #endif
            inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);
        }

        half4 VRWorldCurveFragment(CurvedVaryings input) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

            half4 albedoAlpha = SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
            SurfaceData surfaceData = (SurfaceData)0;
            surfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff);
            surfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;
            surfaceData.specular = half3(0.0, 0.0, 0.0);

            #if defined(_METALLICSPECGLOSSMAP)
                half4 metallicGloss = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, input.uv);
                surfaceData.metallic = metallicGloss.r;
                surfaceData.smoothness = metallicGloss.a * _Smoothness;
            #else
                surfaceData.metallic = _Metallic;
                surfaceData.smoothness = _Smoothness;
            #endif

            surfaceData.normalTS = SampleNormal(input.uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);

            #if defined(_OCCLUSIONMAP)
                half occlusion = SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, input.uv).g;
                surfaceData.occlusion = lerp(1.0h, occlusion, _OcclusionStrength);
            #else
                surfaceData.occlusion = 1.0h;
            #endif

            surfaceData.emission = SampleEmission(input.uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));

            #if defined(_PRESERVE_SPECULAR_LIGHTING)
                surfaceData.albedo *= surfaceData.alpha;
                surfaceData.emission *= surfaceData.alpha;
            #endif

            #if defined(_ALPHAMODULATE_ON)
                surfaceData.albedo = lerp(half3(1.0h, 1.0h, 1.0h), surfaceData.albedo, surfaceData.alpha);
            #endif

            surfaceData.clearCoatMask = 0.0h;
            surfaceData.clearCoatSmoothness = 0.0h;

            InputData inputData;
            VRInitializeInputData(input, surfaceData.normalTS, inputData);

            half4 color = UniversalFragmentPBR(inputData, surfaceData);
            color.rgb = MixFog(color.rgb, inputData.fogCoord);

            #if defined(_ALPHAPREMULTIPLY_ON)
                color.rgb *= color.a;
            #endif

            color.a = OutputAlpha(color.a, _Surface > 0.5h);
            return color;
        }

        struct CurvedDepthVaryings
        {
            float2 uv : TEXCOORD0;
            float4 positionCS : SV_POSITION;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };

        CurvedDepthVaryings VRWorldCurveDepthVertex(CurvedAttributes input)
        {
            CurvedDepthVaryings output = (CurvedDepthVaryings)0;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_TRANSFER_INSTANCE_ID(input, output);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
            float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
            positionWS = VRApplyWorldCurve(positionWS);
            output.positionCS = TransformWorldToHClip(positionWS);
            output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
            return output;
        }

        half4 VRWorldCurveDepthFragment(CurvedDepthVaryings input) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            #if defined(_ALPHATEST_ON)
                Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);
            #endif
            return 0;
        }

        struct CurvedDepthNormalsVaryings
        {
            float2 uv : TEXCOORD0;
            float3 positionWS : TEXCOORD1;
            half3 normalWS : TEXCOORD2;
            half4 tangentWS : TEXCOORD3;
            float4 positionCS : SV_POSITION;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };

        CurvedDepthNormalsVaryings VRWorldCurveDepthNormalsVertex(CurvedAttributes input)
        {
            CurvedDepthNormalsVaryings output = (CurvedDepthNormalsVaryings)0;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_TRANSFER_INSTANCE_ID(input, output);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            float3 originalPositionWS = TransformObjectToWorld(input.positionOS.xyz);
            float3 originalNormalWS = TransformObjectToWorldNormal(input.normalOS);
            float3 originalTangentWS = TransformObjectToWorldDir(input.tangentOS.xyz);

            output.positionWS = VRApplyWorldCurve(originalPositionWS);
            output.positionCS = TransformWorldToHClip(output.positionWS);
            output.normalWS = VRApplyWorldCurveNormal(originalPositionWS, originalNormalWS);
            #if defined(_NORMALMAP)
                output.tangentWS = half4(
                    VRApplyWorldCurveTangent(originalPositionWS, originalTangentWS),
                    input.tangentOS.w * GetOddNegativeScale());
            #else
                output.tangentWS = half4(1.0h, 0.0h, 0.0h, input.tangentOS.w * GetOddNegativeScale());
            #endif
            output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
            return output;
        }

        half4 VRWorldCurveDepthNormalsFragment(CurvedDepthNormalsVaryings input) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

            #if defined(_ALPHATEST_ON)
                Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);
            #endif

            #if defined(_NORMALMAP)
                half3 tangentWS = normalize(input.tangentWS.xyz);
                half3 bitangentWS = input.tangentWS.w * cross(input.normalWS, tangentWS);
                half3 normalTS = SampleNormal(input.uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
                half3 normalWS = TransformTangentToWorld(normalTS, half3x3(tangentWS, bitangentWS, input.normalWS));
            #else
                half3 normalWS = input.normalWS;
            #endif

            return half4(NormalizeNormalPerPixel(normalWS), 0.0h);
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Blend [_SrcBlend] [_DstBlend], [_SrcBlendAlpha] [_DstBlendAlpha]
            ZWrite [_ZWrite]
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex VRWorldCurveVertex
            #pragma fragment VRWorldCurveFragment

            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF
            #pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local_fragment _ALPHAMODULATE_ON
            #pragma shader_feature_local_fragment _PRESERVE_SPECULAR_LIGHTING
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            #pragma shader_feature_local_fragment _OCCLUSIONMAP

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_ATLAS
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex VRWorldCurveShadowVertex
            #pragma fragment VRWorldCurveShadowFragment
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _CAST_SHADOWS_OFF
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct CurvedShadowVaryings
            {
                #if defined(_ALPHATEST_ON)
                    float2 uv : TEXCOORD0;
                #endif
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CurvedShadowVaryings VRWorldCurveShadowVertex(CurvedAttributes input)
            {
                CurvedShadowVaryings output = (CurvedShadowVaryings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                #if defined(_ALPHATEST_ON)
                    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                #endif

                float3 originalPositionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 originalNormalWS = TransformObjectToWorldNormal(input.normalOS);
                float3 positionWS = VRApplyWorldCurve(originalPositionWS);
                float3 normalWS = VRApplyWorldCurveNormal(originalPositionWS, originalNormalWS);
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
                output.positionCS = ApplyShadowClamping(positionCS);
                return output;
            }

            half4 VRWorldCurveShadowFragment(CurvedShadowVaryings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                #if defined(_CAST_SHADOWS_OFF)
                    discard;
                #endif
                #if defined(_ALPHATEST_ON)
                    Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);
                #endif
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite [_ZWrite]
            ColorMask R
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex VRWorldCurveDepthVertex
            #pragma fragment VRWorldCurveDepthFragment
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma multi_compile_instancing
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite [_ZWrite]
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex VRWorldCurveDepthNormalsVertex
            #pragma fragment VRWorldCurveDepthNormalsFragment
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma multi_compile_instancing
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/Meta"
    }

    FallBack Off
    CustomEditor "EditorTools.WorldCurvatureLitShaderGUI"
}