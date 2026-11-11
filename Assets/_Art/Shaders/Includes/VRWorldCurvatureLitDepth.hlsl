#ifndef VIRTUAL_RUNNER_WORLD_CURVATURE_LIT_DEPTH_INCLUDED
#define VIRTUAL_RUNNER_WORLD_CURVATURE_LIT_DEPTH_INCLUDED

struct CurvedDepthVaryings
        {
            half alpha : TEXCOORD1;
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
            output.alpha = input.color.a;
            return output;
        }

        half4 VRWorldCurveDepthFragment(CurvedDepthVaryings input) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            #if defined(_ALPHATEST_ON)
                Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a * input.alpha, _BaseColor, _Cutoff);
            #endif
            return 0;
        }

        struct CurvedDepthNormalsVaryings
        {
            half alpha : TEXCOORD4;
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
            output.alpha = input.color.a;
            return output;
        }

        half4 VRWorldCurveDepthNormalsFragment(CurvedDepthNormalsVaryings input) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

            #if defined(_ALPHATEST_ON)
                Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a * input.alpha, _BaseColor, _Cutoff);
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

#endif
