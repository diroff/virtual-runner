#ifndef VIRTUAL_RUNNER_WORLD_CURVATURE_LIT_INPUT_INCLUDED
#define VIRTUAL_RUNNER_WORLD_CURVATURE_LIT_INPUT_INCLUDED

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
            half4 _ColorAddSubDiff;
        CBUFFER_END

        TEXTURE2D(_MetallicGlossMap);
        SAMPLER(sampler_MetallicGlossMap);
        TEXTURE2D(_OcclusionMap);
        SAMPLER(sampler_OcclusionMap);

        // Matches the Color Mode blending used by URP Particles Lit.
        half4 VRMixVertexColor(half4 baseColor, half4 vertexColor)
        {
            #if defined(_COLOROVERLAY_ON)
                half4 result = baseColor;
                result.rgb = lerp(
                    1.0h - 2.0h * (1.0h - baseColor.rgb) * (1.0h - vertexColor.rgb),
                    2.0h * baseColor.rgb * vertexColor.rgb,
                    step(baseColor.rgb, 0.5h));
                result.a *= vertexColor.a;
                return result;
            #elif defined(_COLORCOLOR_ON)
                half3 baseHSV = RgbToHsv(baseColor.rgb);
                half3 vertexHSV = RgbToHsv(vertexColor.rgb);
                return half4(HsvToRgb(half3(vertexHSV.x, vertexHSV.y, baseHSV.z)),
                    baseColor.a * vertexColor.a);
            #elif defined(_COLORADDSUBDIFF_ON)
                half4 result = baseColor;
                result.rgb = baseColor.rgb + vertexColor.rgb * _ColorAddSubDiff.x;
                result.rgb = lerp(result.rgb, abs(result.rgb), _ColorAddSubDiff.y);
                result.a *= vertexColor.a;
                return result;
            #else
                return baseColor * vertexColor;
            #endif
        }

        struct CurvedAttributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            half4 color : COLOR;
            float2 texcoord : TEXCOORD0;
            float2 staticLightmapUV : TEXCOORD1;
            float2 dynamicLightmapUV : TEXCOORD2;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

#endif
